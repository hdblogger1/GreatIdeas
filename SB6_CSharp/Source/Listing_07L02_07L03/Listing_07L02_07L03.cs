//=================================================================================================
// The code herein has been adapted from the book "OpenGL SuperBible - Sixth Edition" and its
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
    /// Our OpenTK GameWindow derived application class which takes care of creating a window, 
    /// handling input, and displaying the rendered results to the user.
    /// </summary>
    class Listing_07L02_07L03 : GameWindow
    {
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// The Statics container class holding class-wide static globals
        /// </summary>
        static class Statics
        {
            public static readonly float[] colorGreen = { 0.0f, 0.25f, 0.0f, 1.0f };
            public static readonly float[] one = { 1.0f };

            public static readonly string vertexShaderSource = @"
                #version 430 core

                in vec4 position;

                out VS_OUT
                {
                    vec4 color;
                } vs_out;

                uniform mat4 mv_matrix;
                uniform mat4 proj_matrix;

                void main(void)
                {
                    gl_Position  = proj_matrix * mv_matrix * position;
                    vs_out.color = position * 2.0 + vec4(0.5, 0.5, 0.5, 0.0);
                }
                ";
                           
            public static readonly string fragmentShaderSource = @"
                #version 430 core

                out vec4 color;

                in VS_OUT
                {
                    vec4 color;
                } fs_in;

                void main(void)
                {
                    color = fs_in.color;
                }
                ";

            public static readonly ushort[] vertexIndices = new ushort[]
                {
                    0, 1, 2,
                    2, 1, 3,
                    2, 3, 4,
                    4, 3, 5,
                    4, 5, 6,
                    6, 5, 7,
                    6, 7, 0,
                    0, 7, 1,
                    6, 0, 2,
                    2, 4, 6,
                    7, 5, 3,
                    7, 3, 1
                };

            public static readonly float[] vertexPositions = new float[]
                {
                    -0.25f, -0.25f, -0.25f,
                    -0.25f,  0.25f, -0.25f,
                     0.25f, -0.25f, -0.25f,
                     0.25f,  0.25f, -0.25f,
                     0.25f, -0.25f,  0.25f,
                     0.25f,  0.25f,  0.25f,
                    -0.25f, -0.25f,  0.25f,
                    -0.25f,  0.25f,  0.25f,
                };
        }

        private int _shaderProgramName;
        private int _vao;

        private struct Buffers
        {
            public int positionBuffer;
            public int indexBuffer;
        };
        private Buffers _buffers;

        private struct UniformNames
        {
            public int mvMatrixName;
            public int prjMatrixName;
        }
        private UniformNames _uniforms;

        //private int _vertexArrayBufferName;
        //private int[] _uniformLocations = new int[2];
        //private static class UniformLocation 
        ///{ 
        //    public const int MV_MATRIX = 0;
        //    public const int PRJ_MATRIX = 1;
        //};
        
        private Matrix4 _projMatrix;

        //-----------------------------------------------------------------------------------------
        public Listing_07L02_07L03() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Indexed Cube", 
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitProgram()
        {
            int vertexShaderName = Framework.Shader.Compile( ShaderType.VertexShader, 
                                                              Statics.vertexShaderSource );
            int fragmentShaderName = Framework.Shader.Compile( ShaderType.FragmentShader, 
                                                                Statics.fragmentShaderSource );
                
            _shaderProgramName = Framework.Shader.Link( new int[] { vertexShaderName, fragmentShaderName }, 2 );

            _uniforms.mvMatrixName = GL.GetUniformLocation( _shaderProgramName, "mv_matrix" );
            _uniforms.prjMatrixName = GL.GetUniformLocation( _shaderProgramName, "proj_matrix" );

            GL.GenVertexArrays( 1, out _vao );
            GL.BindVertexArray( _vao );

            return true;
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitBuffers()
        {
            GL.GenBuffers( 1, out _buffers.positionBuffer );
            GL.BindBuffer( BufferTarget.ArrayBuffer, _buffers.positionBuffer );
            GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(Statics.vertexPositions.Length * sizeof(float)), 
                           Statics.vertexPositions, BufferUsageHint.StaticDraw);
            
            GL.VertexAttribPointer( 0, 3, VertexAttribPointerType.Float, false, 0, 0 );
            GL.EnableVertexAttribArray( 0 );

            GL.GenBuffers( 1, out _buffers.indexBuffer );
            GL.BindBuffer( BufferTarget.ElementArrayBuffer, _buffers.indexBuffer );
            GL.BufferData( BufferTarget.ElementArrayBuffer, (IntPtr)(Statics.vertexIndices.Length * sizeof(uint)),
                           Statics.vertexIndices, BufferUsageHint.StaticDraw );

            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );

            return true;
        }
        
        //-----------------------------------------------------------------------------------------
        private void _UpdateUniforms( float elapsedSeconds )
        {
            Matrix4 mvMatrix;

            float f = elapsedSeconds * 0.3f;

            mvMatrix = Matrix4.CreateFromAxisAngle( new Vector3(1.0f, 0.0f, 0.0f), 
                                                    elapsedSeconds * MathHelper.DegreesToRadians(81.0f) );
            mvMatrix *= Matrix4.CreateFromAxisAngle( new Vector3(0.0f, 1.0f, 0.0f), 
                                                     elapsedSeconds * MathHelper.DegreesToRadians(45.0f) );
            mvMatrix *= Matrix4.CreateTranslation( (float)(Math.Sin(2.1f * f) * 0.5f), 
                                                   (float)(Math.Cos(1.7f * f) * 0.5f),
                                                   (float)(Math.Sin(1.3f * f) * Math.Cos(1.5f * f) * 2.0f) );
            mvMatrix *= Matrix4.CreateTranslation( 0.0f, 0.0f, -4.0f );

            // Set the model-view and projection matrices
            GL.UniformMatrix4( _uniforms.mvMatrixName, false, ref mvMatrix );
            GL.UniformMatrix4( _uniforms.prjMatrixName, false, ref _projMatrix );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnResize( EventArgs e )
        {
            GL.Viewport( 0, 0, Width, Height );
            float aspect = (float)Width / (float)Height;
            _projMatrix = Matrix4.CreatePerspectiveFieldOfView( MathHelper.DegreesToRadians(50.0f), 
                                                                aspect, 0.1f, 1000.0f );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteProgram( _shaderProgramName );
            GL.DeleteBuffer( _buffers.indexBuffer );
            GL.DeleteBuffer( _buffers.positionBuffer );
            GL.DeleteVertexArray( _vao );
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            this._InitProgram();
            this._InitBuffers();

            GL.Enable( EnableCap.CullFace );
            //GL.FrontFace( FrontFaceDirection.Cw );

            GL.Enable( EnableCap.DepthTest );
            GL.DepthFunc( DepthFunction.Lequal );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            // Clear the window with given color
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorGreen );
            GL.ClearBuffer( ClearBuffer.Depth, 0, Statics.one );

            // Use the program object we created earlier for rendering
            GL.UseProgram( _shaderProgramName );

            this._UpdateUniforms( (float)Program.ElapsedTimeSeconds );

            GL.DrawElements( PrimitiveType.Triangles, 36, DrawElementsType.UnsignedShort, 0 );
            
            SwapBuffers();
        }
    }
}
