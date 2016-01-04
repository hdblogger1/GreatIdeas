using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SB6_CSharp
{
    class Example_02L01 : GameWindow
    {
        static readonly float[] _red = new float[] { 1.0f, 0.0f, 0.0f, 1.0f };
        
        //-----------------------------------------------------------------------------------------
        public Example_02L01() 
            : base( 640, 480, GraphicsMode.Default, "OpenTK Example", 0, DisplayDevice.Default
                    // ask for an OpenGL 4.3 or higher default(core?) context
                    , 4, 3, GraphicsContextFlags.Default)
        {
        }

        //-----------------------------------------------------------------------------------------
        //Our rendering function
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            //Simply clear the window with red
            GL.ClearBuffer(ClearBuffer.Color, 0, _red);
 
            SwapBuffers();
        }
    }
}
