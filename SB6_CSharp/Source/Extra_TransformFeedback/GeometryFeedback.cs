//=================================================================================================
// The code herein has been adapted from the examples given at  https://open.gl/feedback. Please 
//see 'Copyright_SB6.txt' for copyright information.
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
    class GeometryFeedback : GameWindow
    {
        static class Statics
        {
            public static readonly string vertexShaderSource = @"
                #version 430 core

                in float  vs_value;
                out float gs_value;

                void main()
                {
                    gs_value = sqrt(vs_value);
                }
                ";
            
            public static readonly string geometryShaderSource = @"
                #version 430 core

                layout(points) in;
                layout(triangle_strip, max_vertices = 3) out;

                in float[] gs_value;
                out float tf_value;

                void main()
                {
                    for( int i = 0; i < 3; i++ )
                    {
                        tf_value = gs_value[0] + i;
                        EmitVertex();
                    }
                    EndPrimitive();
                }
                ";
        }
        
        private int _shaderProgram;
        private int _vao;
        private int _vboInput;
        private int _vboOutput;

        private int _attrInValue;

        //-----------------------------------------------------------------------------------------
        public GeometryFeedback() 
            : base( 800, 600, GraphicsMode.Default, "SB6_CSharp - Geometry Shader Transform Feedback", 
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitProgram()
        {
            int vertexShader = GL.CreateShader( ShaderType.VertexShader );
            GL.ShaderSource( vertexShader, Statics.vertexShaderSource );
            GL.CompileShader( vertexShader );
            
            int geometryShader = GL.CreateShader( ShaderType.GeometryShader );
            GL.ShaderSource( geometryShader, Statics.geometryShaderSource );
            GL.CompileShader( geometryShader );
            
            _shaderProgram = GL.CreateProgram();
            GL.AttachShader( _shaderProgram, vertexShader );
            GL.AttachShader( _shaderProgram, geometryShader );

            // Before linking the program we need to inform OpenGL which outputs need to be captured
            string[] feedbackVaryings = { "tf_value" };
            GL.TransformFeedbackVaryings( _shaderProgram, 1, feedbackVaryings, TransformFeedbackMode.InterleavedAttribs );

            GL.LinkProgram( _shaderProgram );
            GL.UseProgram( _shaderProgram );

            GL.DeleteShader( vertexShader );
            GL.DeleteShader( geometryShader );

            _vao = GL.GenVertexArray();
            GL.BindVertexArray( _vao );

            return true;
        }
        
        //-----------------------------------------------------------------------------------------
        private bool _InitBuffers()
        {
            float[] data = new float[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };

            _vboInput = GL.GenBuffer();
            GL.BindBuffer( BufferTarget.ArrayBuffer, _vboInput );
            GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * data.Length),
                           data, BufferUsageHint.StaticDraw );

            _attrInValue = GL.GetAttribLocation( _shaderProgram, "vs_value" );
            GL.EnableVertexAttribArray( _attrInValue );
            GL.VertexAttribPointer( _attrInValue, 1, VertexAttribPointerType.Float, false, 0, 0 );

            // Create buffer to hold captured values. Because each input vertex will generate 3 
            // vertices as output, the buffer needs to be 3 times as big as the input buffer
            _vboOutput = GL.GenBuffer();
            GL.BindBuffer( BufferTarget.ArrayBuffer, _vboOutput );
            GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * data.Length * 3),
                           IntPtr.Zero, BufferUsageHint.StaticRead );
            
            return true;
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            this._InitProgram();
            this._InitBuffers();

            // Because we don't intend on drawing anything, we can disable the rasterizer
            GL.Enable( EnableCap.RasterizerDiscard );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteProgram( _shaderProgram );
            GL.DeleteVertexArray( _vao );
            GL.DeleteBuffer( _vboInput );
            GL.DeleteBuffer( _vboOutput );
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            GL.BindBufferBase( BufferRangeTarget.TransformFeedbackBuffer, 0, _vboOutput );
            // Primitive must match the output type of the geometry shader
            GL.BeginTransformFeedback( TransformFeedbackPrimitiveType.Triangles );       
            GL.DrawArrays( PrimitiveType.Points, 0, 5 );
            GL.EndTransformFeedback();
            
            // Make sure rendering operation has finished before trying to access results
            GL.Flush();
            //SwapBuffers();

            float[] feedback = new float[15];
            GL.GetBufferSubData( BufferTarget.TransformFeedbackBuffer, (IntPtr)0, 
                                 (IntPtr)(sizeof(float) * feedback.Length), feedback );

            for( int i = 0; i < 15; i++ )
            {
                Console.Write( String.Format( "{0:F2} ", feedback[i] ) );
            }
            Console.Write( "\n" );
        }
    }
}
