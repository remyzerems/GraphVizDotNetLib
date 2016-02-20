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
using System.Runtime.InteropServices;
using System.IO;

namespace GraphVizDotNetLib
{
    public static class GraphVizCore
    {

        #region Helper functions
        /// <summary>
        /// Add paths to the PATH environnement variable
        /// </summary>
        /// <param name="paths">Path list to add</param>
        /// For more details on this code see <see cref="http://stackoverflow.com/questions/2864673/specify-the-search-path-for-dllimport-in-net"/>
        public static void AddEnvironmentPaths(params string[] paths)
        {
            string path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            foreach (string p in paths)
            {
                // Append the current path to the path string if it is not already present
                if (!path.Contains(p))
                {
                    path += ";" + string.Join(";", p);
                }
            }

            Environment.SetEnvironmentVariable("PATH", path);
        }

        /// <summary>
        /// Function to try to automatically get the GraphViz install path
        /// </summary>
        /// <returns>The GraphViz path if found, "" if not found</returns>
        /// <remarks>This code is meant for Windows environnement</remarks>
        public static string GetGraphVizPath()
        {
            // Referencing the GraphViz required dlls to find
            string[] GraphVizRequiredDLLs = { "cgraph.dll", "gvc.dll" };

            string GraphVizDirectory = "";
            // First try to find GraphViz in the Program files directory
            var GraphVizDirectorySearch = Directory.EnumerateDirectories(@"C:\Program Files\", "graph*viz*", SearchOption.TopDirectoryOnly);
            foreach (string dir in GraphVizDirectorySearch)
            {
                // Pick up the first directory occurence found that matches the pattern
                GraphVizDirectory = dir;
                break;
            }
            // If we didn't find the base path
            if (GraphVizDirectory == "")
            {
                // Search the (x86) folder
                GraphVizDirectorySearch = Directory.EnumerateDirectories(@"C:\Program Files (x86)", "graph*viz*", SearchOption.TopDirectoryOnly);
                foreach (string dir in GraphVizDirectorySearch)
                {
                    // Pick up the first directory occurence found that matches the pattern
                    GraphVizDirectory = dir;
                    break;
                }
            }

            // If we found the base path
            if (GraphVizDirectory != "")
            {
                // Go to the /bin directory
                GraphVizDirectory = Path.Combine(GraphVizDirectory, "bin");

                // Assume we found all GraphViz required files
                bool allDLLsFound = true;
                // Flow through all the required files
                for (int i = 0; i < GraphVizRequiredDLLs.Length; i++)
                {
                    // And check that each of them exists
                    allDLLsFound &= File.Exists(Path.Combine(GraphVizDirectory, GraphVizRequiredDLLs[i]));
                }

                // If at least one has not been found, set the return to empty string
                if (allDLLsFound == false)
                {
                    GraphVizDirectory = "";
                }
            }

            return GraphVizDirectory;
        }
        #endregion

        #region GraphViz DLL Imports

        [DllImport("cgraph.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void agclose(IntPtr file);

        [DllImport("cgraph.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr agmemread(string graphVizData);

        [DllImport("gvc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gvContext();

        [DllImport("gvc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int gvFreeLayout(IntPtr context, IntPtr graph);

        [DllImport("gvc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int gvLayout(IntPtr context, IntPtr graph, string engine);

        [DllImport("gvc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int gvRenderData(IntPtr context, IntPtr graph, string format, out byte* result, out uint length);

        [DllImport("gvc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void gvFreeRenderData(byte* buffer);

        [DllImport("gvc.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern int gvFreeContext(IntPtr gvc);

        #endregion
    }
}
