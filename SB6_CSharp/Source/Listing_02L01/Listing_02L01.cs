using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SB6_CSharp
{
    //=============================================================================================
    /// <summary>
    /// Our OpenTK GameWindow derived application class which takes care of creating a window, 
    /// handling input, and displaying the rendered results to the user.
    /// </summary>
    class Listing_02L01 : GameWindow
    {        
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// The Statics container class holding class-wide static globals
        /// </summary>
        static class Statics
        {
            public static readonly float[] colorRed = { 1.0f, 0.0f, 0.0f, 1.0f };
        }

        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// A constructor requesting an OpenGL 4.3 Core context
        /// </summary>
        public Listing_02L01() 
            : base( 800, 600,                            // 800 x 600 window
                    GraphicsMode.Default, 
                    "OpenGL SuperBible - Simple Clear",  // window title
                    0, DisplayDevice.Default, 
                    4, 3,                                // OpenGL 4.3 Core context
                    GraphicsContextFlags.Default )
        {
        }

        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            // Simply clear the window with red
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorRed );
 
            // OpenTK creates double-buffered contexts by default: a 'back' buffer, where all 
            // rendering takes place, and a 'front' buffer which is displayed to the user. The
            // following SwapBuffers() method 'swaps' the front and back buffers and displays the
            // rendered frame to the user.
            SwapBuffers();
        }
    }
}
