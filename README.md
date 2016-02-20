# GraphViz Dot Net Library
GraphViz DLL wrapper library. This library does not use intermediary files but rather directly call the GraphViz DLLs.

### Features
* Draws images from GraphViz code
* Only memory streams : no intermediary files when rendering the image, no console tricks
* Configurable image output format
* Configurable layout engine (dot, neato...)
* Automated determination of the GraphViz install path (DLLs location)

### Basic example

```csharp
// Add this using directive
using GraphVizDotNetLib;

// Start creating a graph
GraphVizRenderer gv = new GraphVizRenderer();

// Draw the graph described by the code
Bitmap bmp = gv.DrawGraphFromDotCode("digraph{a -> b; b -> c; c -> a;}");

// Do whatever you want to do with the bitmap
...

// Once finished, free the resources
gv.Dispose();
```