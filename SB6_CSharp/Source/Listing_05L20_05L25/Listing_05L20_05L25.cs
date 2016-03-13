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
    class Listing_05L20_05L25 : GameWindow
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

        private int _vertexArrayBufferName;
        private int[] _uniformLocations = new int[2];
        private static class UniformLocation 
        { 
            public const int MV_MATRIX = 0;
            public const int PRJ_MATRIX = 1;
        };
        
        private Matrix4 _projMatrix;

        //-----------------------------------------------------------------------------------------
        public Listing_05L20_05L25() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Spinny Cube", 
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
                -0.25f, 0.25f,-0.25f,   -0.25f,-0.25f,-0.25f,    0.25f,-0.25f,-0.25f,
                 0.25f,-0.25f,-0.25f,    0.25f, 0.25f,-0.25f,   -0.25f, 0.25f,-0.25f,
                // right
                 0.25f,-0.25f,-0.25f,    0.25f,-0.25f, 0.25f,    0.25f, 0.25f,-0.25f,
                 0.25f,-0.25f, 0.25f,    0.25f, 0.25f, 0.25f,    0.25f, 0.25f,-0.25f,
                // front
                 0.25f,-0.25f, 0.25f,   -0.25f,-0.25f, 0.25f,    0.25f, 0.25f, 0.25f,
                -0.25f,-0.25f, 0.25f,   -0.25f, 0.25f, 0.25f,    0.25f, 0.25f, 0.25f,
                // left
                -0.25f,-0.25f, 0.25f,   -0.25f,-0.25f,-0.25f,   -0.25f, 0.25f, 0.25f,
                -0.25f,-0.25f,-0.25f,   -0.25f, 0.25f,-0.25f,   -0.25f, 0.25f, 0.25f,
                // bottom
                -0.25f,-0.25f, 0.25f,    0.25f,-0.25f, 0.25f,    0.25f,-0.25f,-0.25f,
                 0.25f,-0.25f,-0.25f,   -0.25f,-0.25f,-0.25f,   -0.25f,-0.25f, 0.25f,
                // top
                -0.25f, 0.25f,-0.25f,    0.25f, 0.25f,-0.25f,    0.25f, 0.25f, 0.25f,
                 0.25f, 0.25f, 0.25f,   -0.25f, 0.25f, 0.25f,   -0.25f, 0.25f,-0.25f
            };

            GL.GenBuffers( 1, out _vertexArrayBufferName );

            GL.BindBuffer( BufferTarget.ArrayBuffer, _vertexArrayBufferName );
            GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(vertexData.Length * sizeof(float)), 
                           vertexData, BufferUsageHint.StaticDraw);
            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );

            return true;
        }
        
        //-----------------------------------------------------------------------------------------
        private bool _InitVao()
        {
            // Create VAO object to hold vertex shader inputs and attach it to our context. As 
            // OpenGL requires the VAO object (whether or not it's used) we do this here.
            GL.GenVertexArrays( 1, out _vertexArrayName );
            GL.BindVertexArray( _vertexArrayName );

            // Set up our vertex attribute
            GL.BindBuffer( BufferTarget.ArrayBuffer, _vertexArrayBufferName );
            GL.VertexAttribPointer( 0, 3, VertexAttribPointerType.Float, false, 0, 0 );
            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );

            GL.EnableVertexAttribArray( 0 );
            
            return true;
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitUniforms()
        {
            _uniformLocations[UniformLocation.MV_MATRIX] 
                 = GL.GetUniformLocation( _shaderProgramName, "mv_matrix" );
            _uniformLocations[UniformLocation.PRJ_MATRIX]
                = GL.GetUniformLocation( _shaderProgramName, "proj_matrix" );

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
            GL.UniformMatrix4( _uniformLocations[UniformLocation.MV_MATRIX], false, ref mvMatrix );
            GL.UniformMatrix4( _uniformLocations[UniformLocation.PRJ_MATRIX], false, ref _projMatrix );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnResize( EventArgs e )
        {
            GL.Viewport( ClientRectangle.X, ClientRectangle.Y, 
                         ClientRectangle.Width, ClientRectangle.Height );
            float aspect = (float)Width / (float)Height;
            _projMatrix = Matrix4.CreatePerspectiveFieldOfView( MathHelper.DegreesToRadians(50.0f), 
                                                                aspect, 0.1f, 1000.0f );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteProgram( _shaderProgramName );
            GL.DeleteBuffers( 1, ref _vertexArrayBufferName );
            GL.DeleteVertexArrays( 1, ref _vertexArrayName );
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            this._InitProgram();
            this._InitBuffers();             /* note: this has to come before _InitVao() */
            this._InitVao();
            this._InitUniforms();

            GL.Enable( EnableCap.CullFace );
            GL.FrontFace( FrontFaceDirection.Cw );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            // Clear the window with given color
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorGreen );
 
            // Use the program object we created earlier for rendering
            GL.UseProgram( _shaderProgramName );

            this._UpdateUniforms( (float)Program.ElapsedTimeSeconds );

            // Draw 6 faces of 2 triangles of 3 vertices each = 36 vertices
            GL.DrawArrays( PrimitiveType.Triangles, 0, 36 );
            
            SwapBuffers();
        }
    }
}
