using System;
using System.Drawing;
using System.Diagnostics;
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
    class Example_02L02 : GameWindow
    {
        //-----------------------------------------------------------------------------------------
        public Example_02L02() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Listing 2.2",
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            double elapsedSeconds = Program.ElapsedTimeSeconds;
            
            // Animate color based on elasped seconds since application startup
            float[] color = { (float)(Math.Sin( elapsedSeconds ) * 0.5f + 0.5f),
                              (float)(Math.Cos( elapsedSeconds ) * 0.5f + 0.5f),
                              0.0f, 1.0f };

            // Clear the window with our calculated color
            GL.ClearBuffer( ClearBuffer.Color, 0, color );
 
            SwapBuffers();
        }
    }
}
