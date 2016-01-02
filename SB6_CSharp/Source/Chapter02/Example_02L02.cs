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
        //Counter for holding elapsed time since application instantiation
        Stopwatch _counter = Stopwatch.StartNew();

        float[] _color = new float[] { 1.0f, 0.0f, 0.0f, 1.0f };
        
        //Our rendering function
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            //Animate color
            _color[0] = (float) (Math.Sin(_counter.Elapsed.Seconds) * 0.5f + 0.5f);
            _color[1] = (float) (Math.Cos(_counter.Elapsed.Seconds) * 0.5f + 0.5f);
            
            //Clear the window with given color
            GL.ClearBuffer(ClearBuffer.Color, 0, _color);
 
            SwapBuffers();
        }
    }
}
