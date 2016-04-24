//=================================================================================================
// The code herein is based wholly or in part from the book "OpenGL SuperBible - Sixth Edition" and
// its accompanying C++ example source code. See 'Copyright_SB6.txt' for copyright information.
//=================================================================================================
using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

using SB6_CSharp.Framework;

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
        private static class Statics
        {
            public static readonly float[] clearColor = Color4.Red.ToArrayExt(); //extention functions end in Ext
            //...we need this as there are no Color4 overloads for ClearBuffer
        }

        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// A constructor requesting an OpenGL 4.3 Core (Forward Compatible/Debug) context. By
        /// specifying a Forward Compatible context flag, we are disabling all 'deprecated' 
        /// functionality in both the OpenGL API *and* the shader GLSL.
        /// </summary>
        public Listing_02L01() 
            : base( 800, 600,                               // 800 x 600 window
                    GraphicsMode.Default, 
                    "SB6_CSharp - Simple Clear",            // window title
                    0, DisplayDevice.Default, 
                    4, 3,                                   // *request* OpenGL 4.3 context
                    GraphicsContextFlags.ForwardCompatible
                    | GraphicsContextFlags.Debug )
        {
            // https://www.opengl.org/registry/specs/ARB/wgl_create_context.txt
            // InitFramework(); // setup error message callbacks like sb6 so they are displyed in output window in debug mode
            //Context.ErrorChecking = false;
        }

        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            // Clear the window with a red background
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.clearColor );

            /* OpenTK creates double-buffered contexts by default: a 'back' buffer, where all 
               rendering takes place, and a 'front' buffer which is displayed to the user. The
               following SwapBuffers() method 'swaps' the front and back buffers and displays the
               rendered frame to the user. */
            SwapBuffers();
        }
    }
}
