﻿using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SB6_CSharp
{
    class Example_02L03_02L07 : GameWindow
    {
        //Counter for holding elapsed time since application instantiation
        Stopwatch _counter = Stopwatch.StartNew();

        float[] _color = new float[] { 1.0f, 0.0f, 0.0f, 1.0f };

        private int _renderingProgramHandle;
        private int _vaoHandle;

        //-----------------------------------------------------------------------------------------
        public int CompileShaders()
        {
            int vertexShaderHandle, fragmentShaderHandle;
            int shaderProgramHandle;
                
            //Source code for vertex shader
            string vertexShaderSource = @"
                #version 430 core
                void main(void)
                {
                    gl_Position = vec4(0.0, 0.0, 0.5, 1.0);
                }
                ";
                
            //Source code for fragment shader
            string fragmentShaderSource = @"
                #version 430 core

                out vec4 color;

                void main(void)
                {
                    color = vec4(0.0, 0.8, 1.0, 1.0);
                }
                ";

            //Create and compile vertex shader
            vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource( vertexShaderHandle, vertexShaderSource );
            GL.CompileShader( vertexShaderHandle );

            //Create and compile fragment shader
            fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource( fragmentShaderHandle, fragmentShaderSource );
            GL.CompileShader( fragmentShaderHandle );

            //Create program, attach shaders to it, and link it
            shaderProgramHandle = GL.CreateProgram();
            GL.AttachShader( shaderProgramHandle, vertexShaderHandle );
            Console.WriteLine(GL.GetShaderInfoLog(vertexShaderHandle));
            GL.AttachShader( shaderProgramHandle, fragmentShaderHandle );
            Console.WriteLine(GL.GetShaderInfoLog(fragmentShaderHandle));
            GL.LinkProgram( shaderProgramHandle );

            //Delete the shaders as the program has them now
            GL.DeleteShader( vertexShaderHandle );
            GL.DeleteShader( fragmentShaderHandle );

            return shaderProgramHandle;
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnLoad (EventArgs e)
        {
            _renderingProgramHandle = CompileShaders();
                
            //Create VAO object to hold vertex shader inputs and attach it to our context
            GL.GenVertexArrays(1, out _vaoHandle);
            GL.BindVertexArray(_vaoHandle);
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload(EventArgs e)
        {
            GL.DeleteVertexArrays(1, ref _vaoHandle);
            GL.DeleteProgram(_renderingProgramHandle);
        }
        
        //-----------------------------------------------------------------------------------------
        //Our rendering function
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            //Animate color
            _color[0] = (float) (Math.Sin(_counter.Elapsed.Seconds) * 0.5f + 0.5f);
            _color[1] = (float) (Math.Cos(_counter.Elapsed.Seconds) * 0.5f + 0.5f);
            
            //Clear the window with given color
            GL.ClearBuffer(ClearBuffer.Color, 0, _color);
 
            //Use the program object we created earlier for rendering
            GL.UseProgram(_renderingProgramHandle);

            //Uncomment the following to set point size to at least 64 pixels
            //GL.PointSize(40.0f);

            //Draw one point
            GL.DrawArrays(PrimitiveType.Points, 0, 1);
            
            SwapBuffers();
        }
    }
}