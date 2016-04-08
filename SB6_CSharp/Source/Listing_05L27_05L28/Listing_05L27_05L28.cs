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

using System.Runtime.InteropServices;

namespace SB6_CSharp
{
    //=============================================================================================
    /// <summary>
    /// Our OpenTK GameWindow derived application class which takes care of creating a window, 
    /// handling input, and displaying the rendered results to the user.
    /// </summary>
    class Listing_05L27_05L28 : GameWindow
    {
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// The Statics container class holding class-wide static globals
        /// </summary>
        static class Statics
        {
            public static readonly float[] colorGreen = { 0.0f, 0.25f, 0.0f, 1.0f };
        }

        private int _shaderProgramName;
        private int _vertexArrayName;

        private int _shaderStorageBufferName;
        private int _uniformLocation;

        private Matrix4 _projMatrix;
        private Matrix4 _transformMatrix;


        //-----------------------------------------------------------------------------------------
        public Listing_05L27_05L28() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Listing 5.27 thru 5.28", 
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitProgram()
        {
            int vertexShaderName, fragmentShaderName;
                
            // Source code for vertex shader
            string vertexShaderSource = @"
                #version 430 core

                struct vertex
                {
                    vec4 position;
                    vec3 color;
                };

                layout(binding=0, std430) buffer my_vertices
                {
                    vertex vertices[];
                };

                uniform mat4 transform_matrix;

                out VS_OUT
                {
                    vec3 color;
                } vs_out;

                void main(void)
                {
                    gl_Position  = transform_matrix * vertices[gl_VertexID].position;
                    vs_out.color = vertices[gl_VertexID].color;
                }
                ";
                
            // Source code for fragment shader
            string fragmentShaderSource = @"
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

            // Create and compile vertex shader
            vertexShaderName = GL.CreateShader( ShaderType.VertexShader );
            GL.ShaderSource( vertexShaderName, vertexShaderSource );
            GL.CompileShader( vertexShaderName );

            // Create and compile fragment shader
            fragmentShaderName = GL.CreateShader( ShaderType.FragmentShader );
            GL.ShaderSource( fragmentShaderName, fragmentShaderSource );
            GL.CompileShader( fragmentShaderName );

            // Create program, attach shaders to it, and link it
            _shaderProgramName = GL.CreateProgram();
            GL.AttachShader( _shaderProgramName, vertexShaderName );
            Console.WriteLine( GL.GetShaderInfoLog( vertexShaderName ) );
            GL.AttachShader( _shaderProgramName, fragmentShaderName );
            Console.WriteLine( GL.GetShaderInfoLog( fragmentShaderName ) );
            GL.LinkProgram( _shaderProgramName );

            // Delete the shaders as the program has them now
            GL.DeleteShader( vertexShaderName );
            GL.DeleteShader( fragmentShaderName );

            return true;
        }
        
        //-----------------------------------------------------------------------------------------
        private bool _InitBuffers()
        {
            float[] vertexData = new float[] 
            {
                // back
                -0.25f, 0.25f,-0.25f,1,   -0.25f,-0.25f,-0.25f,1,    0.25f,-0.25f,-0.25f,1,
                 0.25f,-0.25f,-0.25f,1,    0.25f, 0.25f,-0.25f,1,   -0.25f, 0.25f,-0.25f,1,
                // right
                 0.25f,-0.25f,-0.25f,1,    0.25f,-0.25f, 0.25f,1,    0.25f, 0.25f,-0.25f,1,
                 0.25f,-0.25f, 0.25f,1,    0.25f, 0.25f, 0.25f,1,    0.25f, 0.25f,-0.25f,1,
                // front
                 0.25f,-0.25f, 0.25f,1,   -0.25f,-0.25f, 0.25f,1,    0.25f, 0.25f, 0.25f,1,
                -0.25f,-0.25f, 0.25f,1,   -0.25f, 0.25f, 0.25f,1,    0.25f, 0.25f, 0.25f,1,
                // left
                -0.25f,-0.25f, 0.25f,1,   -0.25f,-0.25f,-0.25f,1,   -0.25f, 0.25f, 0.25f,1,
                -0.25f,-0.25f,-0.25f,1,   -0.25f, 0.25f,-0.25f,1,   -0.25f, 0.25f, 0.25f,1,
                // bottom
                -0.25f,-0.25f, 0.25f,1,    0.25f,-0.25f, 0.25f,1,    0.25f,-0.25f,-0.25f,1,
                 0.25f,-0.25f,-0.25f,1,   -0.25f,-0.25f,-0.25f,1,   -0.25f,-0.25f, 0.25f,1,
                // top
                -0.25f, 0.25f,-0.25f,1,    0.25f, 0.25f,-0.25f,1,    0.25f, 0.25f, 0.25f,1,
                 0.25f, 0.25f, 0.25f,1,   -0.25f, 0.25f, 0.25f,1,   -0.25f, 0.25f,-0.25f,1
            };

            GL.GenBuffers( 1, out _shaderStorageBufferName );
            GL.BindBuffer( BufferTarget.ShaderStorageBuffer, _shaderStorageBufferName );
            GL.BufferData( BufferTarget.ShaderStorageBuffer, (IntPtr)(1024 * sizeof(float)),
                           IntPtr.Zero, BufferUsageHint.StaticDraw );

            float[] color2 = new float[3];
            
            int bufferOffset;
            for( int vertexNum = 0; vertexNum < 36; vertexNum++ )
            {
                bufferOffset = vertexNum * (8 * sizeof(float));
                GL.BufferSubData( BufferTarget.ShaderStorageBuffer, (IntPtr)bufferOffset, 
                                  (IntPtr)(4*sizeof(float)), ref vertexData[vertexNum * 4] );

                color2[0] = vertexData[(vertexNum * 4) + 0] * 2.0f + 0.5f;
                color2[1] = vertexData[(vertexNum * 4) + 1] * 2.0f + 0.5f;
                color2[2] = vertexData[(vertexNum * 4) + 2] * 2.0f + 0.5f;
                
                GL.BufferSubData( BufferTarget.ShaderStorageBuffer, (IntPtr)(bufferOffset + (4 * sizeof(float))), 
                                  (IntPtr)(3 * sizeof(float)), color2 );
            }

            GL.BindBufferBase( BufferRangeTarget.ShaderStorageBuffer, 0, _shaderStorageBufferName );


            return true;
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitVao()
        {
            // Create VAO object to hold vertex shader inputs and attach it to our context. As 
            // OpenGL requires the VAO object (whether or not it's used) we do this here.
            GL.GenVertexArrays( 1, out _vertexArrayName );
            GL.BindVertexArray( _vertexArrayName );
            
            return true;
        }

        //-----------------------------------------------------------------------------------------
        private bool _initUniforms()
        {
            _uniformLocation = GL.GetUniformLocation( _shaderProgramName, "transform_matrix" );

            return true;
        }

        //-----------------------------------------------------------------------------------------
        private void _UpdateTransform()
        {
            float elapsedSeconds = (float)Program.ElapsedTimeSeconds;

            float f = elapsedSeconds * 0.3f;

            _transformMatrix = Matrix4.CreateFromAxisAngle( new Vector3( 1.0f, 0.0f, 0.0f ), 
                                                            elapsedSeconds * MathHelper.DegreesToRadians( 81.0f ) );
            _transformMatrix *= Matrix4.CreateFromAxisAngle( new Vector3( 0.0f, 1.0f, 0.0f ), 
                                                             elapsedSeconds * MathHelper.DegreesToRadians( 45.0f ) );
            _transformMatrix *= Matrix4.CreateTranslation( (float)(Math.Sin( 2.1f * f ) * 0.5f), 
                                                           (float)(Math.Cos( 1.7f * f ) * 0.5f),
                                                           (float)(Math.Sin( 1.3f * f ) * Math.Cos( 1.5f * f ) * 2.0f) );
            _transformMatrix *= Matrix4.CreateTranslation( 0.0f, 0.0f, -4.0f );
            _transformMatrix *= Matrix4.CreateScale( 1.0f, 1.0f, 1.0f );
            _transformMatrix *= _projMatrix;
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnResize( EventArgs e )
        {
            GL.Viewport( ClientRectangle.X, ClientRectangle.Y, 
                         ClientRectangle.Width, ClientRectangle.Height );
            float aspect = (float)Width / (float)Height;
            _projMatrix = Matrix4.CreatePerspectiveFieldOfView( MathHelper.DegreesToRadians( 50.0f ), 
                                                                aspect, 0.1f, 1000.0f );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteProgram( _shaderProgramName );
            GL.DeleteBuffers( 1, ref _shaderStorageBufferName );
            GL.DeleteVertexArrays( 1, ref _vertexArrayName );
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            this._InitProgram();
            this._InitBuffers();             /* note: this has to come before _InitVao() */
            this._InitVao();
            this._initUniforms();

            GL.Enable( EnableCap.CullFace );
            GL.FrontFace( FrontFaceDirection.Cw );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            // Clear the window with given color
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorGreen );

            this._UpdateTransform();

            // Use the program object we created earlier for rendering
            GL.UseProgram( _shaderProgramName );
            
            // Set the transformation matrix for current program object
            GL.UniformMatrix4( _uniformLocation, false, ref _transformMatrix );

            //GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Line );

            // Draw 6 faces of 2 triangles of 3 vertices each = 36 vertices
            GL.DrawArrays( PrimitiveType.Triangles, 0, 36 );
            
            SwapBuffers();
        }
    }
}
