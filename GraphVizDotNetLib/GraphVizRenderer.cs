/*************************************************************************
 * Copyright (c) 2016 Remy Dispagne, freely inspired from the AT&T source code
 * For more details, see https://github.com/ellson/graphviz/tree/master/windows
 * 
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *************************************************************************/

using System;
using System.IO;
using System.Drawing;

namespace GraphVizDotNetLib
{
    /// <summary>
    /// Class that represents a GraphViz renderer
    /// </summary>
    public class GraphVizRenderer:IDisposable
    {
        const string GV_DEFAULT_LAYOUT = "dot";

        // Disposed flag to handle resources disposal
        private bool disposed = false;

        /// <summary>
        /// Constructs a GraphVizRenderer by trying to automatically find the GraphViz install path
        /// </summary>
        public GraphVizRenderer()
        {
            // Get the install path
            string graphVizInstallPath = GraphVizCore.GetGraphVizPath();
            // If path has been found
            if (graphVizInstallPath != "")
            {
                Initialize(graphVizInstallPath);
            }
            else
            {
                throw new DirectoryNotFoundException("Unable to automatically find GraphViz install path. Use Graph constructor with explicit path instead, or check GraphViz is installed on your computer.");
            }
        }

        /// <summary>
        /// Constructs a GraphVizRenderer being given the GraphViz install path
        /// </summary>
        /// <param name="graphVizInstallPath"></param>
        public GraphVizRenderer(string graphVizInstallPath)
        {
            Initialize(graphVizInstallPath);
        }

        /// <summary>
        /// Initializes the GraphViz renderer
        /// </summary>
        /// <param name="graphVizInstallPath">GraphViz install path</param>
        private void Initialize(string graphVizInstallPath)
        {
            GraphVizCore.AddEnvironmentPaths(graphVizInstallPath);
            GVContext = GraphVizCore.gvContext();

            LayoutEngine = GV_DEFAULT_LAYOUT;
        }

        /// <summary>
        /// GraphViz context property
        /// </summary>
        protected IntPtr GVContext
        {
            get;
            set;
        }

        /// <summary>
        /// GraphViz graph property
        /// </summary>
        protected IntPtr GVGraph
        {
            get;
            set;
        }

        /// <summary>
        /// Layout engine used to draw the graphs
        /// </summary>
        public string LayoutEngine
        {
            get;
            set;
        }

        /// <summary>
        /// Renders a graph to a data stream
        /// </summary>
        /// <param name="gvContext">A GraphViz context pointer</param>
        /// <param name="gvGraph">A GraphViz graph pointer</param>
        /// <param name="format">The GraphViz renderer format string</param>
        /// <returns>The stream representing the rendered graph</returns>
        protected static GraphVizRenderStream Render(IntPtr gvContext, IntPtr gvGraph, string format)
        {
            unsafe
            {
                byte* result;
                uint length;
                //  Render the graph to a byte stream
                if (GraphVizCore.gvRenderData(gvContext, gvGraph, format, out result, out length) != 0)
                {
                    throw new InvalidDataException("GraphViz gvRenderData function return empty pointer");
                }

                // Turn the byte stream to a "managed" stream
                return new GraphVizRenderStream(result, length);
            }
        }

        /// <summary>
        /// Draws a graph from a .gv file
        /// </summary>
        /// <param name="filename">Filename of the file</param>
        /// <returns>A Bitmap image representing the graph described in the file</returns>
        public Bitmap DrawGraphFromFile(String filename)
        {
            // Load the file
            StreamReader sr = new StreamReader(filename);
            // Get it as a string
            string fileContent = sr.ReadToEnd();
            // Close the file
            sr.Close();
            // Render the file content using the unified fonction
            return DrawGraphFromDotCode(fileContent);
        }

        /// <summary>
        /// Draws a graph from a string using the png output format
        /// </summary>
        /// <param name="dotGraphCode">String representing a graph</param>
        /// <returns>A Bitmap image representing the graph described in the string</returns>
        public Bitmap DrawGraphFromDotCode(String dotGraphCode)
        {
            return DrawGraphFromDotCode(dotGraphCode, "png");
        }

        /// <summary>
        /// Draws a graph from a string using the given output format
        /// </summary>
        /// <param name="dotGraphCode">String representing a graph</param>
        /// <param name="outputType">GraphViz output format string</param>
        /// <returns>A Bitmap image representing the graph described in the string</returns>
        public Bitmap DrawGraphFromDotCode(String dotGraphCode, string outputType)
        {
            // Create the graph from the code
            CreateGraph(dotGraphCode);

            return DrawGraph(outputType);
        }

        /// <summary>
        /// Draws the current graph using the png output format
        /// </summary>
        /// <param name="outputType">GraphViz output format string</param>
        /// <returns>A Bitmap image representing the graph described by the current graph</returns>
        public Bitmap DrawGraph(string outputType)
        {
            Bitmap res = null;
            if (GVGraph != IntPtr.Zero)
            {
                // Specify the layout we want to use
                SetLayout(LayoutEngine);

                // Render the graph to a stream
                Stream rs = Render(GVContext, GVGraph, outputType);
                // Create a bitmap out of the stream
                res = new Bitmap(rs);
                // Release the stream
                rs.Dispose();

                // Free the layout
                FreeLayout();
            }
            return res;
        }

        /// <summary>
        /// Creates a graph from the given code
        /// </summary>
        /// <param name="dotGraphCode">GraphViz code</param>
        public void CreateGraph(String dotGraphCode)
        {
            // Create the graph from the string
            GVGraph = GraphVizCore.agmemread(dotGraphCode);
            // Did it succeed
            if (GVGraph == IntPtr.Zero)
            {
                throw new InvalidDataException("Unable to read the given data string");
            }
        }

        /// <summary>
        /// Sets the layout that will be used to render the graph
        /// </summary>
        /// <param name="layout">Layout engine name</param>
        protected void SetLayout(string layout)
        {
            if (GraphVizCore.gvLayout(GVContext, GVGraph, layout) != 0)
                throw new ArgumentException("Error while setting layout to \"" + layout + "\"");
        }

        /// <summary>
        /// Free the layout
        /// </summary>
        protected void FreeLayout()
        {
            if (GraphVizCore.gvFreeLayout(GVContext, GVGraph) != 0)
                throw new Exception("Error while freeing the layout");
        }

        /// <summary>
        /// Dispose the graph
        /// </summary>
        public void Dispose() 
        { 
            Dispose(true); 
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if(disposing)
                {
                    // Release managed resources

                }

                // Release unmanaged resources
                GraphVizCore.agclose(GVGraph);
                GraphVizCore.gvFreeContext(GVContext);

                disposed = true;
            }
        }

        ~GraphVizRenderer() { Dispose(false); }
    }

    /// <summary>
    /// GraphVis memory stream of rendered graph
    /// </summary>
    public unsafe class GraphVizRenderStream : UnmanagedMemoryStream
    {
        // Disposed flag to handle resources disposal
        private bool disposed = false;

        private readonly byte* _pointer;

        public GraphVizRenderStream(byte* pointer, long length)
            : base(pointer, length)
        {
            _pointer = pointer;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposed)
            {
                if (disposing)
                {
                    
                }

                // Free the stream
                GraphVizCore.gvFreeRenderData(_pointer);

                disposed = true;
            }
        }

        ~GraphVizRenderStream() { Dispose(false); }
    }
}
