//With USE_SHARED_LAYOUT defined, the vertex shader will use a 'shared' uniform block. With it *NOT*
//defined, the vertex shader will use a 'standard' (std140) uniform block.
#define USE_SHARED_LAYOUT

//With USE_BOUNDED_LAYOUT defined, the vertex shader will use a 'standard' (std140) uniform block
//which is bound to a fixed uniform block binding point. With it *NOT* defined, the vertex shader 
//will use a 'standard' (std140) uniform block that is unbounded.
//#define USE_BOUNDED_LAYOUT

using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using System.Runtime.InteropServices;

namespace SB6_CSharp
{
    class Example_05L09_05L17 : GameWindow
    {
        float[] _color = new float[] { 1.0f, 0.0f, 0.0f, 1.0f };

        //Because this could be used in a 'standard' uniform, we need to place each vertice on a
        //4N-byte boundry
        float[] _vertices = new float[] 
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

        int _renderingProgramHandle;
        int _vaoHandle;

        int _vabHandle;
        int _uboHandle;

        class UniformInfo
        {
            public string[] names = new string[] {
                "TransformBlock.scale",
                "TransformBlock.translation",
                "TransformBlock.rotation",
                "TransformBlock.proj_matrix",
            };        
            public int[] indices = new int[4];
            public int[] offsets = new int[4];
            public int[] arrayStrides = new int[4];
            public int[] matrixStrides = new int[4];
        };
        
        UniformInfo _uniformInfo = new UniformInfo();
        Matrix4 _projMatrix;

        //-----------------------------------------------------------------------------------------
        public Example_05L09_05L17() 
            : base( 640, 480, GraphicsMode.Default, "OpenTK Example", 0, DisplayDevice.Default
                    // ask for an OpenGL 4.3 or higher default(core?) context
                    , 4, 3, GraphicsContextFlags.Default)
        {
        }

        //-----------------------------------------------------------------------------------------
        public int CompileShaders()
        {
            int vertexShaderHandle, fragmentShaderHandle;
            int shaderProgramHandle;
                
            //Source code for vertex shader
            string vertexShaderSource = @"#version 430 core
                
                layout(location = 1) in vec4 position;

                out vec4 vs_color;" +
              
#if USE_SHARED_LAYOUT
                @"// 'shared' (default) layout
                uniform TransformBlock
                {
                    float scale;       // Global scal to apply to everything
                    vec3  translation; // Translation in X, Y, and Z
                    float rotation[3]; // Rotation around X, Y, and Z axes
                    mat4  proj_matrix; // A generalized projection matrix to apply
                                       //   after scale and rotate
                } transform;" +
#else
#  if USE_BOUNDED_LAYOUT
                @"// 'standard' with fixed binding point layout
                layout(std140, binding=2) uniform TransformBlock
                {                       // base alignment | offset | aligned offset
                    float scale;        //       4             0        0
                    vec3  translation;  //      16             4       16
                    float rotation[3];  //      16            28       32 (rotation[0])
                                        //                             48 (rotation[1])
                                        //                             64 (rotation[2])
                    mat4  proj_matrix;  //      16            80       80 (column 0)
                                        //                             96 (column 1)
                                        //                            112 (column 2)
                                        //                            128 (column 3) 
                } transform;" +
#  else
                @"// 'standard' layout
                layout(std140) uniform TransformBlock
                {                       // base alignment | offset | aligned offset
                    float scale;        //       4             0        0
                    vec3  translation;  //      16             4       16
                    float rotation[3];  //      16            28       32 (rotation[0])
                                        //                             48 (rotation[1])
                                        //                             64 (rotation[2])
                    mat4  proj_matrix;  //      16            80       80 (column 0)
                                        //                             96 (column 1)
                                        //                            112 (column 2)
                                        //                            128 (column 3) 
                } transform;" +
#  endif
#endif 
                @"mat4 translate(vec3 axis)
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
               
            //Source code for fragment shader
            string fragmentShaderSource = @"
                #version 430 core
                out vec4 color;
                in vec4 vs_color;

                void main(void)
                {
                    //color = vec4(0.0, 0.8, 1.0, 1.0);
                    color = vs_color;
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
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            float aspect = (float)Width / (float)Height;
            _projMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(50.0f), aspect, 0.1f, 1000.0f);
        }

        //-----------------------------------------------------------------------------------------
        public int SetupVertexAttributeBuffer()
        {
            int vabHandle;

            //Generate a vertex attribute buffer and fill it with our global vertices array
            //Generate a name for the buffer
            GL.GenBuffers(1, out vabHandle);

            //Bind it to the context using the GL_ARRAY_BUFFER binding point
            GL.BindBuffer(BufferTarget.ArrayBuffer, vabHandle);

            //Specify the amount of storage we want and fill it with our vetices array
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(_vertices.Length * sizeof(float)), 
                          _vertices, BufferUsageHint.StaticDraw);

            //Set up our vertex attribute with the attributes location layout qualifier
            int vabIndex = 1;
            GL.VertexAttribPointer(vabIndex, 4, VertexAttribPointerType.Float, false, 0, 0 );
            GL.EnableVertexAttribArray(vabIndex);
            
            return vabHandle;
        }

        //-----------------------------------------------------------------------------------------
        public int SetupUniform()
        {
            int uboHandle;
            
            //Generate a name for the buffer
            GL.GenBuffers(1, out uboHandle);

            //Bind it to the context using the GL_UNIFORM_BUFFER binding point
            GL.BindBuffer(BufferTarget.UniformBuffer, uboHandle);

            //Specify the amount of storage we want to allocate for our uniform
            GL.BufferData(BufferTarget.UniformBuffer, (IntPtr)(1024 * sizeof(float)), 
                          IntPtr.Zero, BufferUsageHint.DynamicDraw);
            
            int ubpIndex; //buffer binding point of the uniform block

#if (!USE_SHARED_LAYOUT && USE_BOUNDED_LAYOUT)
            //Set the uniform block binding point index to the 'binding' layout qualifier of the 
            //uniform block
            ubpIndex = 2; 
#else
            //Get the index of the uniform block
            int uboIndex = GL.GetUniformBlockIndex(_renderingProgramHandle, "TransformBlock" );
            
            //Pick an arbitrary uniform buffer binding point to use for the uniform block
            ubpIndex = 3; 
            
            //Assign a uniform buffer binding point of <ubpIndex> for the uniform block <uboIndex>
            GL.UniformBlockBinding(_renderingProgramHandle, uboIndex, ubpIndex);
#endif
            //Tell OpenGL that we're binding the buffer to the <ubpIndex> uniform buffer binding 
            //point
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, ubpIndex, uboHandle);

            return uboHandle;
        }

        //-----------------------------------------------------------------------------------------
        public void InitUniformData()
        {
#if USE_SHARED_LAYOUT
            // Retrieve the indices of the uniform block members
            GL.GetUniformIndices(_renderingProgramHandle, 4, _uniformInfo.names, _uniformInfo.indices);

            // Retrieve the information about the uniform block members
            GL.GetActiveUniforms(_renderingProgramHandle, 4, _uniformInfo.indices, 
                                 ActiveUniformParameter.UniformOffset, _uniformInfo.offsets );
            GL.GetActiveUniforms(_renderingProgramHandle, 4, _uniformInfo.indices, 
                                 ActiveUniformParameter.UniformArrayStride, _uniformInfo.arrayStrides );
            GL.GetActiveUniforms(_renderingProgramHandle, 4, _uniformInfo.indices, 
                                 ActiveUniformParameter.UniformMatrixStride, _uniformInfo.matrixStrides );
#else
            // Set the 'Standard' indices of the uniform block members
            _uniformInfo.offsets[0] = 0;
            _uniformInfo.offsets[1] = 16;
            _uniformInfo.offsets[2] = 32;
            _uniformInfo.offsets[3] = 80;

            //Set strides of the uniform block memebers. Entrys not arrays or matrices are defaulted
            _uniformInfo.arrayStrides[2] = 16;
            _uniformInfo.matrixStrides[3] = 16;
#endif

            //Scale is the only static uniform data we have so we can set it here. Translation, 
            //rotation and projection is done dynamically during frame rendering (OnRenderFrame).
            float scale = 1.5f;
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)_uniformInfo.offsets[0], 
                             (IntPtr)(1 * sizeof(float)), ref scale );            
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad (EventArgs e)
        {
            _renderingProgramHandle = this.CompileShaders();
                
            //Create VAO object to hold vertex shader inputs and attach it to our context
            GL.GenVertexArrays(1, out _vaoHandle);
            GL.BindVertexArray(_vaoHandle);

            _vabHandle = this.SetupVertexAttributeBuffer();

            _uboHandle = this.SetupUniform();

            this.InitUniformData();
            
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Cw);
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload(EventArgs e)
        {
            GL.DeleteBuffer(_uboHandle);
            GL.DeleteBuffer(_vabHandle);
            GL.DeleteVertexArrays(1, ref _vaoHandle);
            GL.DeleteProgram(_renderingProgramHandle);
        }
        
        //-----------------------------------------------------------------------------------------
        //Our rendering function
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // Get elapsed time since application startup
            float elapsedSeconds = (float)(Program.ElapsedTimeSeconds);

            //Set color to green
            _color[0] = 0.0f;
            _color[1] = 0.2f;
            
            //Clear the window with given color
            GL.ClearBuffer(ClearBuffer.Color, 0, _color);
 
            //Use the program object we created earlier for rendering
            GL.UseProgram(_renderingProgramHandle);

            GL.BindBuffer(BufferTarget.UniformBuffer, _uboHandle);

            //SCALE
            float scale = 1.0f;
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)_uniformInfo.offsets[0], 
                             (IntPtr)(1 * sizeof(float)), ref scale );  

            //TRANSLATION
            Vector3 translation;
            float f = elapsedSeconds * 0.3f;
            translation.X = (float)(Math.Sin(2.1f * f) * 0.5f);
            translation.Y = (float)(Math.Cos(1.7f * f) * 0.5f);
            translation.Z = (float)(Math.Sin(1.3f * f) * Math.Cos(1.5f * f) * 2.0f) - 3.0f;

            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)_uniformInfo.offsets[1], 
                             (IntPtr)(3 * sizeof(float)), ref translation );  

            //ROTATION
            float[] rotation = new float[3];
            rotation[0] = elapsedSeconds * MathHelper.DegreesToRadians(81.0f);
            rotation[1] = elapsedSeconds * MathHelper.DegreesToRadians(45.0f);
            rotation[2] = rotation[0];

            int offset = _uniformInfo.offsets[2];
            for(int n=0; n<3; n++)
            {
                GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)offset, 
                                 (IntPtr)(1 * sizeof(float)), ref rotation[n] );
                offset += _uniformInfo.arrayStrides[2];
            }
            
            //PROJECTION
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)_uniformInfo.offsets[3], 
                             (IntPtr)(16 * sizeof(float)), ref _projMatrix );
 
            //Tell OpenGL to draw outlines only
            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line );

            //Draw one cube
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            
            SwapBuffers();
        }
    }
}
