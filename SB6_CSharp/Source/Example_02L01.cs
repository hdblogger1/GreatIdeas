using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SB6_CSharp
{
    class Example_02L01 : GameWindow
    {
        float[] _colorRed = new float[] { 1.0f, 0.0f, 0.0f, 1.0f };
        
        //Our rendering function
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            //Simply clear the window with red
            GL.ClearBuffer(ClearBuffer.Color, 0, _colorRed);
 
            SwapBuffers();
        }
    }
}
