using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elements.Serialization.JSON
{
    internal class ElementConverterFactory : JsonConverterFactory
    {
        private readonly bool _elementwiseSerialization;

        /// <summary>
        /// Should the elements be serialized completely? If this option is false,
        /// elements will be serialized to ids.
        /// </summary>
        /// <param name="elementwiseSerialization"></param>
        public ElementConverterFactory(bool elementwiseSerialization = false)
        {
            _elementwiseSerialization = elementwiseSerialization;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(Element).IsAssignableFrom(typeToConvert);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var elementConverter = typeof(ElementConverter<>);
            var typeArgs = new[] { typeToConvert };
            var converterType = elementConverter.MakeGenericType(typeArgs);
            var converter = Activator.CreateInstance(converterType) as JsonConverter;
            var pi = converterType.GetProperty("ElementwiseSerialization");
            pi.SetValue(converter, _elementwiseSerialization);
            return converter;
        }
    }

    /// <summary>
    /// Convert elements, lists of elements and dictionaries of elements.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ElementConverter<T> : JsonConverter<T>
    {
        public bool ElementwiseSerialization { get; internal set; } = false;

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (options.ReferenceHandler == null)
            {
                throw new Exception("You are deserializing an element, but you don't have a reference resolver. Try using Element.Deserialize<T> instead.");
            }

            var resolver = options.ReferenceHandler.CreateResolver() as ElementReferenceResolver;

            if (reader.TokenType == JsonTokenType.String)
            {
                // Convert an id reference into an element.
                var id = reader.GetString();
                return (T)resolver.ResolveReference(id);
            }
            else
            {
                if (PropertySerializationExtensions.IsAcceptedCollectionType(typeToConvert, out var collectionType))
                {
                    var elements = Activator.CreateInstance(typeToConvert);
                    var mi = typeToConvert.GetMethod("Add");
                    switch (collectionType)
                    {
                        case CollectionType.List:
                            // At this point we'll be at the start of an array.
                            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                            {
                                var id = reader.GetString();
                                mi.Invoke(elements, new[] { resolver.ResolveReference(id) });
                            }

                            break;
                        case CollectionType.Dictionary:
                            var args = typeToConvert.GetGenericArguments();
                            // At this point we'll be at the start of an object
                            // This will be a dictionary that looks like id: id
                            using (var doc = JsonDocument.ParseValue(ref reader))
                            {
                                var root = doc.RootElement;
                                foreach (var prop in root.EnumerateObject())
                                {
                                    if (args[0] == typeof(Guid))
                                    {
                                        mi.Invoke(elements, new[] { Guid.Parse(prop.Name), resolver.ResolveReference(prop.Value.GetString()) });
                                    }
                                    else
                                    {
                                        mi.Invoke(elements, new[] { prop.Name, resolver.ResolveReference(prop.Value.GetString()) });
                                    }
                                }
                            }
                            break;
                    }
                    return (T)elements;
                }
                else
                {

                    using (var doc = JsonDocument.ParseValue(ref reader))
                    {
                        // Deserialize an element.
                        var root = doc.RootElement;

                        var discriminator = root.GetProperty("discriminator").GetString();

                        if (!resolver.TypeCache.TryGetValue(discriminator, out var derivedType))
                        {
                            // The type could not be found. See if it has the hallmarks
                            // of a geometric element and deserialize it as such if possible.
                            if (root.TryGetProperty("Representation", out _))
                            {
                                derivedType = typeof(GeometricElement);
                            }
                            else
                            {
                                return default;
                            }
                        }

                        // Use the type info to get all properties which are Element
                        // references, and deserialize those first.

                        // TODO: This *should* support serialization of elements in
                        // any order, removing the requirement to do any kind of recursive
                        // sub-element searching. We can remove that code from the model.
                        PropertySerializationExtensions.DeserializeElementProperties(derivedType, root, resolver, resolver.DocumentElements);

                        T e = (T)root.Deserialize(derivedType, options);
                        if (typeof(Element).IsAssignableFrom(derivedType))
                        {
                            resolver.AddReference(((Element)(object)e).Id.ToString(), e);
                        }
                        return e;
                    }
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var isElement = typeof(Element).IsAssignableFrom(value.GetType());
            if (writer.CurrentDepth > 2 && isElement && !ElementwiseSerialization)
            {
                writer.WriteStringValue(((Element)(object)value).Id.ToString());
            }
            else
            {
                writer.WriteStartObject();
                value.WriteProperties(writer, options);
                writer.WriteEndObject();
            }
        }
    }
}