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
    class Example_05L09_05L17A : GameWindow
    {
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// The Statics container class holding class-wide static globals
        /// </summary>
        static class Statics
        {
            public static readonly float[] colorGreen = { 0.0f, 0.25f, 0.0f, 1.0f };
        }
        
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// A management class for OpenGL buffer object names
        /// </summary>
        static class Buffers
        {
            public enum Type { VERTEX_ARRAY, UNIFORM, NUM_BUFFERS };
            public static uint Name( Type type ) { return Names[(int)type]; }
            public static int Count()            { return (int)Type.NUM_BUFFERS; }

            public static uint[] Names = new uint[(int)Type.NUM_BUFFERS];
        }

        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// A management class for OpenGL uniform information
        /// </summary>
        class UniformInfo
        {
            static public string[] Names = new string[] {
                "TransformBlock.scale",
                "TransformBlock.translation",
                "TransformBlock.rotation",
                "TransformBlock.proj_matrix",
            };        
            public int[] Indices = new int[Names.Length];
            public int[] Offsets = new int[Names.Length];
            public int[] ArrayStrides = new int[Names.Length];
            public int[] MatrixStrides = new int[Names.Length];
        };

        private int _shaderProgramName;
        private int _vertexArrayName;

        private UniformInfo _uniformInfo = new UniformInfo();
        private Matrix4 _projMatrix;

        //-----------------------------------------------------------------------------------------
        public Example_05L09_05L17A() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Listing 5.6 thru 5.7 (Shared UBO)", 
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
                layout(location = 1) in vec4 position;
                out vec4 vs_color;
              
                // 'shared' (default) layout
                uniform TransformBlock
                {
                    float scale;       // Global scal to apply to everything
                    vec3  translation; // Translation in X, Y, and Z
                    float rotation[3]; // Rotation around X, Y, and Z axes
                    mat4  proj_matrix; // A generalized projection matrix to apply
                                       //   after scale and rotate
                } transform;

                mat4 translate(vec3 axis)
                {
                    // row-major memory layout
                    const float m[16] = float[16]( 1.0, 0.0, 0.0, axis.x,
                                                   0.0, 1.0, 0.0, axis.y,
                                                   0.0, 0.0, 1.0, axis.z,
                                                   0.0, 0.0, 0.0, 1.0 );
                    // column-major construction
                    return mat4( m[0], m[4], m[8],  m[12],       // column 1
                                 m[1], m[5], m[9],  m[13],       // column 2
                                 m[2], m[6], m[10], m[14],       // column 3
                                 m[3], m[7], m[11], m[15] );     // column 4
                }

                mat4 rotate(vec3 axis, float angle)
                {
                    axis = normalize(axis);
                    float s = sin(angle); // use -sin(angle) for counter-clockwise rotation
                    float c = cos(angle);
                    float oc = 1.0 - c;
    
                    // row-major memory layout
                    const float m[16] = float[16](oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,  0.0,
                                                  oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,  0.0,
                                                  oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c,           0.0,
                                                  0.0,                                0.0,                                0.0,                                1.0);
                    // column-major construction
                    return mat4( m[0], m[4], m[8],  m[12],       // column 1
                                 m[1], m[5], m[9],  m[13],       // column 2
                                 m[2], m[6], m[10], m[14],       // column 3
                                 m[3], m[7], m[11], m[15] );     // column 4
                }

                mat4 scale(vec3 axis)
                {
                    // same for both row-major and column-major memory layouts
                    return mat4( axis.x, 0.0,    0.0,    0.0,
                                 0.0,    axis.y, 0.0,    0.0,
                                 0.0,    0.0,    axis.z, 0.0,
                                 0.0,    0.0,    0.0,    1.0 );
                }

                mat4 default_projection()
                {
                    // row-major memory layout
                    const float m[16] = float[16]( 1.60838008, 0.0,      0.0,     0.0,
                                                   0.0,        2.144507, 0.0,     0.0,
                                                   0.0,        0.0,     -1.0002, -0.20002,
                                                   0.0,        0.0,     -1.0,     0.0 );
                    // column-major construction
                    return mat4( m[0], m[4], m[8],  m[12],
                                 m[1], m[5], m[9],  m[13],
                                 m[2], m[6], m[10], m[14],
                                 m[3], m[7], m[11], m[15] );
                }
                
                void main(void)
                {
                    //ROTATION
                    mat4 rot;
                    rot =  rotate(vec3(1,0,0), transform.rotation[0]);  // x-axis
                    rot *= rotate(vec3(0,1,0), transform.rotation[1]);  // y-axis
                    rot *= rotate(vec3(0,0,1), transform.rotation[2]);  // z-axis
                    
                    //SCALE
                    mat4 scl = scale(vec3(transform.scale, transform.scale, transform.scale));
                    
                    //TRANSLATION
                    mat4 trans = translate(transform.translation);

                    mat4 mv_matrix = trans * rot * scl;
                    
                    //column-major multiplication (v'=NMv)
                    gl_Position = transform.proj_matrix * mv_matrix * position;

                    //row-major multiplication (v'=vMN)
                    //gl_Position = position * transpose(mv_matrix) * transpose(transform.proj_matrix);

                    vs_color = position * 2.0 + vec4(0.5, 0.5, 0.5, 0.0);
                }
                ";
               
            // Source code for fragment shader
            string fragmentShaderSource = @"
                #version 430 core
                out vec4 color;
                in vec4 vs_color;

                void main(void)
                {
                    color = vs_color;
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
                 0.25f, 0.25f, 0.25f,   -0.25f, 0.25f, 0.25f,   -0.25f, 0.25f,-0.25f,
            };

            // Generate our vertext and uniform buffers
            GL.GenBuffers( Buffers.Count(), Buffers.Names );

            // Bind our vertext array and fill it with our vertex data
            GL.BindBuffer( BufferTarget.ArrayBuffer, Buffers.Name( Buffers.Type.VERTEX_ARRAY ) );
            GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(vertexData.Length * sizeof(float)), 
                           vertexData, BufferUsageHint.StaticDraw );
            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );

            // Bind our uniform buffer and intitalize it (zero-fill)
            GL.BindBuffer( BufferTarget.UniformBuffer, Buffers.Name( Buffers.Type.UNIFORM ) );
            GL.BufferData( BufferTarget.UniformBuffer, (IntPtr)(1024 * sizeof(float)), 
                           IntPtr.Zero, BufferUsageHint.DynamicDraw );

            GL.BindBuffer( BufferTarget.UniformBuffer, 0 );

            return true;
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitVao()
        {
            // Create VAO object to hold vertex shader inputs and attach it to our context. As 
            // OpenGL requires the VAO object (whether or not it's used) we do this here.
            GL.GenVertexArrays( 1, out _vertexArrayName );
            GL.BindVertexArray( _vertexArrayName );
            
            // Set up our vertex attribute with the attributes location layout qualifier
            GL.BindBuffer( BufferTarget.ArrayBuffer, Buffers.Name(Buffers.Type.VERTEX_ARRAY) );
            GL.VertexAttribPointer( 1, 3, VertexAttribPointerType.Float, false, 0, 0 );
            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );

            // Enable the attribute
            GL.EnableVertexAttribArray( 1 );

            return true;
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitUniform()
        {
            int ubpIndex;
            
            // Pick an arbitrary uniform buffer binding point to use for the uniform block
            ubpIndex = 3; 
            
            // Get the index of the uniform block
            int uboIndex = GL.GetUniformBlockIndex( _shaderProgramName, "TransformBlock" );
            
            // Assign a uniform buffer binding point of <ubpIndex> for the uniform block <uboIndex>
            GL.UniformBlockBinding( _shaderProgramName, uboIndex, ubpIndex );

            // Retrieve the indices of the uniform block members
            GL.GetUniformIndices( _shaderProgramName, 4, UniformInfo.Names, _uniformInfo.Indices );

            // Retrieve the information about the uniform block members
            GL.GetActiveUniforms( _shaderProgramName, 4, _uniformInfo.Indices, 
                                  ActiveUniformParameter.UniformOffset, _uniformInfo.Offsets );
            GL.GetActiveUniforms( _shaderProgramName, 4, _uniformInfo.Indices, 
                                  ActiveUniformParameter.UniformArrayStride, _uniformInfo.ArrayStrides );
            GL.GetActiveUniforms( _shaderProgramName, 4, _uniformInfo.Indices, 
                                  ActiveUniformParameter.UniformMatrixStride, _uniformInfo.MatrixStrides );

            // Tell OpenGL that we're binding the buffer to the <ubpIndex> uniform buffer binding point
            GL.BindBuffer( BufferTarget.UniformBuffer, Buffers.Name( Buffers.Type.UNIFORM ) );
            GL.BindBufferBase( BufferRangeTarget.UniformBuffer, ubpIndex, 
                               (int) Buffers.Name( Buffers.Type.UNIFORM ) );
    
            // Scale is the only static uniform data we have so we can set it here. Translation, 
            // rotation and projection is done dynamically during frame rendering in _UpdateUniform().
            float scale = 1.5f;
            GL.BufferSubData( BufferTarget.UniformBuffer, (IntPtr)_uniformInfo.Offsets[0], 
                              (IntPtr)(1 * sizeof(float)), ref scale );            
           
            GL.BindBuffer( BufferTarget.UniformBuffer, 0 );

            return true;
        }

        //-----------------------------------------------------------------------------------------
        private void _UpdateUniform( float elapsedSeconds )
        {
            GL.BindBuffer( BufferTarget.UniformBuffer, Buffers.Name( Buffers.Type.UNIFORM ) );

            // ========== SCALE ==========
            // Already set _InitUniform() due to its static nature, but we could set it here also.
            //float scale = 1.0f;
            //GL.BufferSubData( BufferTarget.UniformBuffer, (IntPtr)_uniformInfo.Offsets[0], 
            //                 (IntPtr)(1 * sizeof(float)), ref scale );  

            // ========== TRANSLATION ==========
            Vector3 translation;
            float f = elapsedSeconds * 0.3f;
            translation.X = (float)(Math.Sin(2.1f * f) * 0.5f);
            translation.Y = (float)(Math.Cos(1.7f * f) * 0.5f);
            translation.Z = (float)(Math.Sin(1.3f * f) * Math.Cos(1.5f * f) * 2.0f) - 3.0f;

            GL.BufferSubData( BufferTarget.UniformBuffer, (IntPtr)_uniformInfo.Offsets[1], 
                              (IntPtr)(3 * sizeof(float)), ref translation );  

            // ========== ROTATION ==========
            float[] rotation = new float[3];
            rotation[0] = elapsedSeconds * MathHelper.DegreesToRadians( 81.0f );
            rotation[1] = elapsedSeconds * MathHelper.DegreesToRadians( 45.0f );
            rotation[2] = rotation[0];

            int offset = _uniformInfo.Offsets[2];
            for( int n=0; n<3; n++ )
            {
                GL.BufferSubData( BufferTarget.UniformBuffer, (IntPtr)offset, 
                                  (IntPtr)(1 * sizeof(float)), ref rotation[n] );
                offset += _uniformInfo.ArrayStrides[2];
            }
            
            // ========== PROJECTION ==========
            GL.BufferSubData( BufferTarget.UniformBuffer, (IntPtr)_uniformInfo.Offsets[3], 
                              (IntPtr)(16 * sizeof(float)), ref _projMatrix );
            
            GL.BindBuffer( BufferTarget.UniformBuffer, 0 );
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
            GL.DeleteBuffers( Buffers.Count(), Buffers.Names );
            GL.DeleteVertexArrays( 1, ref _vertexArrayName );
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            this._InitProgram();
            this._InitBuffers();             /* note: this has to come before _InitVao() */
            this._InitVao();
            this._InitUniform();
            
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

            // Tell OpenGL to draw outlines only
            //GL.PolygonMode( MaterialFace.FrontAndBack, PolygonMode.Line );

            this._UpdateUniform( (float) Program.ElapsedTimeSeconds );

            // Draw one cube
            GL.DrawArrays( PrimitiveType.Triangles, 0, 36 );
            
            SwapBuffers();
        }
    }
}
