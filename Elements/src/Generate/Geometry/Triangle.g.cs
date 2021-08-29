//----------------------
// <auto-generated>
//     Generated using the NJsonSchema v10.1.21.0 (Newtonsoft.Json v11.0.0.0) (http://NJsonSchema.org)
// </auto-generated>
//----------------------
using Elements;
using Elements.GeoJSON;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Spatial;
using Elements.Validators;
using Elements.Serialization.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using Line = Elements.Geometry.Line;
using Polygon = Elements.Geometry.Polygon;

namespace Elements.Geometry
{
    #pragma warning disable // Disable all warnings

    /// <summary>A mesh triangle.</summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.1.21.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class Triangle 
    {
        [Newtonsoft.Json.JsonConstructor]
        public Triangle(IList<Vertex> @vertices, Vector3 @normal)
        {
            // var validator = Validator.Instance.GetFirstValidatorForType<Triangle>();
            // if(validator != null)
            // {
            //     validator.PreConstruct(new object[]{ @vertices, @normal});
            // }
        
            this.Vertices = @vertices;
            this.Normal = @normal;
            
            // if(validator != null)
            // {
            //     validator.PostConstruct(this);
            // }
        }
    
        /// <summary>The triangle's vertices.</summary>
        [Newtonsoft.Json.JsonProperty("Vertices", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public IList<Vertex> Vertices { get; set; } = new List<Vertex>();
    
        /// <summary>The triangle's normal.</summary>
        [Newtonsoft.Json.JsonProperty("Normal", Required = Newtonsoft.Json.Required.AllowNull)]
        public Vector3 Normal { get; set; }
    
    
    }
}