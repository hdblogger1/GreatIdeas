//=================================================================================================
// The code herein is an adaptation from the book "OpenGL SuperBible - Sixth Edition" and its
// accompanying C++ example source code. Please see 'Copyright_SB6.txt' for copyright information.
//=================================================================================================
using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using SB6_CSharp.Framework;

namespace SB6_CSharp
{
    //=============================================================================================
    /// <summary>
    /// 
    /// </summary>
    class Listing_07L08_07L09 : GameWindow
    {
        static class Statics
        {
            public static readonly float[] colorBlack = { 0.0f, 0.0f, 0.0f, 1.0f };

            public static readonly string vertexShaderSource = @"
                #version 410 core

                layout (location = 0) in vec4 position;
                layout (location = 1) in vec4 instance_color;
                layout (location = 2) in vec4 instance_position;

                out Fragment
                {
                    vec4 color;
                } fragment;

                void main(void)
                {
                    gl_Position = (position + instance_position) * vec4(0.25, 0.25, 1.0, 1.0);
                    fragment.color = instance_color;
                }
                ";
            
            public static readonly string fragmentShaderSource = @"
                #version 410 core
                precision highp float;

                in Fragment
                {
                    vec4 color;
                } fragment;

                out vec4 color;

                void main(void)
                {
                    color = fragment.color;
                }
                ";

            public static readonly float[] squareVertices =
                {
                    -1.0f, -1.0f, 0.0f, 1.0f,
                     1.0f, -1.0f, 0.0f, 1.0f,
                     1.0f,  1.0f, 0.0f, 1.0f,
                    -1.0f,  1.0f, 0.0f, 1.0f
                };

            public static readonly float[] instanceColors =
                {
                    1.0f, 0.0f, 0.0f, 1.0f,
                    0.0f, 1.0f, 0.0f, 1.0f,
                    0.0f, 0.0f, 1.0f, 1.0f,
                    1.0f, 1.0f, 0.0f, 1.0f
                };

            public static readonly float[] instancePositions =
                {
                    -2.0f, -2.0f, 0.0f, 0.0f,
                     2.0f, -2.0f, 0.0f, 0.0f,
                     2.0f,  2.0f, 0.0f, 0.0f,
                    -2.0f,  2.0f, 0.0f, 0.0f
                };
        }
        
        private int _shaderProgram;
        private int _vao;

        private int _squareBuffer;

        //-----------------------------------------------------------------------------------------
        public Listing_07L08_07L09() 
            : base( 800, 600, GraphicsMode.Default, "SB6_CSharp - Instanced Attributes", 
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitProgram()
        {
            int vertexShader = Shader.Compile( ShaderType.VertexShader, Statics.vertexShaderSource );
            int fragmentShader = Shader.Compile( ShaderType.FragmentShader, Statics.fragmentShaderSource );
            _shaderProgram = Shader.Link( new int[] { vertexShader, fragmentShader }, 2 );

            _vao = GL.GenVertexArray();
            GL.BindVertexArray( _vao );

            return true;
        }
        
        //-----------------------------------------------------------------------------------------
        private bool _InitBuffers()
        {
            _squareBuffer = GL.GenBuffer();
            GL.BindBuffer( BufferTarget.ArrayBuffer, _squareBuffer );
            GL.BufferData( BufferTarget.ArrayBuffer, 
                           (IntPtr)((sizeof(float) * Statics.squareVertices.Length)
                                    + (sizeof(float) * Statics.instanceColors.Length)
                                    + (sizeof(float) * Statics.instancePositions.Length)),
                           IntPtr.Zero, BufferUsageHint.StaticDraw );
            
            int offset = 0;
            int size = sizeof(float) * Statics.squareVertices.Length;
            GL.BufferSubData( BufferTarget.ArrayBuffer, (IntPtr)offset, (IntPtr)size, Statics.squareVertices );
            GL.VertexAttribPointer( 0, sizeof(float), VertexAttribPointerType.Float, false, 0, offset );

            offset += size;
            size = sizeof(float) * Statics.instanceColors.Length;
            GL.BufferSubData( BufferTarget.ArrayBuffer, (IntPtr)offset, (IntPtr)size, Statics.instanceColors );
            GL.VertexAttribPointer( 1, sizeof(float), VertexAttribPointerType.Float, false, 0, offset );

            offset += size;
            size = sizeof(float) * Statics.instancePositions.Length;
            GL.BufferSubData( BufferTarget.ArrayBuffer, (IntPtr)offset, (IntPtr)size, Statics.instancePositions );
            GL.VertexAttribPointer( 2, sizeof(float), VertexAttribPointerType.Float, false, 0, offset );

            GL.EnableVertexAttribArray( 0 );
            GL.EnableVertexAttribArray( 1 );
            GL.EnableVertexAttribArray( 2 );

            GL.VertexAttribDivisor( 1, 1 );
            GL.VertexAttribDivisor( 2, 1 );

            return true;
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            this._InitProgram();
            this._InitBuffers();
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteProgram( _shaderProgram );
            GL.DeleteVertexArray( _vao );
            GL.DeleteBuffer( _squareBuffer );
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorBlack );
            
            GL.UseProgram( _shaderProgram );
            GL.DrawArraysInstanced( PrimitiveType.TriangleFan, 0, 4, 4 );
            
            SwapBuffers();
        }
    }
}
