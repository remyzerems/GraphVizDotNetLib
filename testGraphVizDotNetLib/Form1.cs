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
using System.Windows.Forms;

// Declare we are using the library
using GraphVizDotNetLib;

namespace testGraphVizDotNetLib
{
    public partial class Form1 : Form
    {
        // Declare the GV renderer
        GraphVizRenderer gv;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Create the GV object
            gv = new GraphVizRenderer();

            // Free the previous image if there was one before
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
            }

            // Draw the graph image using the given code and set it to the picturebox
            pictureBox1.Image = gv.DrawGraphFromDotCode("digraph{a -> b; b -> c; c -> a;}");

            // Free the gv object
            gv.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
