using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SB6_CSharp
{
    class Example_02L02 : GameWindow
    {
        static float[] _color = new float[] { 1.0f, 0.0f, 0.0f, 1.0f };
        
        //-----------------------------------------------------------------------------------------
        public Example_02L02() 
            : base( 640, 480, GraphicsMode.Default, "OpenTK Example", 0, DisplayDevice.Default
                    // ask for an OpenGL 4.3 or higher default(core?) context
                    , 4, 3, GraphicsContextFlags.Default)
        {
        }
        
        //-----------------------------------------------------------------------------------------
        //Our rendering function
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            //Get elapsed time since application startup
            double elapsedSeconds = Program.Counter.ElapsedMilliseconds / 1000.0;
            
            //Animate color
            _color[0] = (float) (Math.Sin(elapsedSeconds) * 0.5f + 0.5f);
            _color[1] = (float) (Math.Cos(elapsedSeconds) * 0.5f + 0.5f);
            
            //Clear the window with given color
            GL.ClearBuffer(ClearBuffer.Color, 0, _color);
 
            SwapBuffers();
        }
    }
}
