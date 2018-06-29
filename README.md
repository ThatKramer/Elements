# Hypar SDK
The Hypar SDK is a library for creating functions that execute on Hypar. 

- `Hypar.Elements` provides abstractions for building elements like beams and slabs.
- `Hypar.Geometry` provides a minimal geometry library that supports points, lines, curves, and extrusions.

The Hypar SDK also interacts with several open standards like [GeoJson](http://geojson.org/) and [glTF](https://www.khronos.org/gltf/).

## Getting Started
- The Hypar SDK is currently in beta. Contact beta@hypar.io to have an account created. Functions can be authored and executed locally. A login is only required when publishing your function to the world!
- Install [.net](https://www.microsoft.com/net/) - Hypar Elements is compatible with .net standard 2.1.
- Install the hypar cli. The hypar cli can be used to create, publish, and execute functions on Hypar.
```
hypar new <function name>
cd <function name>
hypar publish
```
- Preview `.glb` models generated by Hypar locally using the [glTF Extension for Visual Studio Code](https://github.com/AnalyticalGraphicsInc/gltf-vscode), or [Don McCurdy's online glTF Viewer](https://gltf-viewer.donmccurdy.com/).

## Examples
The best examples are those provided in the [tests](https://github.com/hypar-io/elements/tree/master/test), where we demonstrate usage of almost every function in the library.

## Third Party Libraries

[LibTessDotNet](https://github.com/speps/LibTessDotNet)  
[Verb](https://github.com/pboyer/verb)
