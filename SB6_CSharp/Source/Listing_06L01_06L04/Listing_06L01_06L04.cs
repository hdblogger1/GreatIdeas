//=================================================================================================
// The code herein has been adapted from the book "OpenGL SuperBible - Sixth Edition" and its
// accompanying C++ example source code. Please see 'Copyright_SB6.txt' for copyright information.
//=================================================================================================
using System;
using System.Drawing;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace SB6_CSharp
{     
    //=============================================================================================
    /// <summary>
    /// Our OpenTK GameWindow derived application class which takes care of creating a window, 
    /// handling input, and displaying the rendered results to the user.
    /// </summary>
    class Listing_06L01_06L04 : GameWindow
    {
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// The Statics container class holding class-wide static globals
        /// </summary>
        static class Statics
        {
            public static readonly float[] colorBlack = { 0.0f, 0.0f, 0.0f, 1.0f };

            public static readonly System.Collections.Generic.Dictionary<All,string> typeNames = 
                new System.Collections.Generic.Dictionary<All,string>()
                {
                    { All.Float,      "float" },
                    { All.FloatVec2,  "vec2" },
                    { All.FloatVec3,  "vec3" },
                    { All.FloatVec4,  "vec4" },
                    { All.Double,     "double" },
                    { All.DoubleVec2, "dvec2" },
                    { All.DoubleVec3, "dvec3" },
                    { All.DoubleVec4, "dvec4" },
                    { All.Int,        "int" },
                    { All.IntVec2,    "ivec2" },
                    { All.IntVec3,    "ivec3" },
                    { All.IntVec4,    "ivec4" },
                    { All.Bool,       "bool" },
                    { All.BoolVec2,   "bvec2" },
                    { All.BoolVec3,   "bvec3" },
                    { All.BoolVec4,   "bvec4" },
                    { All.None,       "?UNK?" }
                };
            
            public static readonly string vertexShaderSource = @"
                #version 430 core

                // Note: To use any built-in input or output variable from the gl_PerVertex and
                // gl_PerFragment blocks in separable program objects, we must redeclare those 
                // blocks before referencing them or the program will fail to link
                out gl_PerVertex { vec4 gl_Position; };

                void main(void)
                {
                    const vec4 vertices[] = vec4[]( vec4(  0.25, -0.25, 0.5, 1.0 ),
                                                    vec4( -0.25, -0.25, 0.5, 1.0 ),
                                                    vec4(  0.25,  0.25, 0.5, 1.0 ));
                    gl_Position = vertices[gl_VertexID];

                    // Note: Uncomment this to generate a shader compiler error
                    //gl_Position = vec4(1.0, 0.5, 0.2, 1.0) * scale + bias;
                }
                ";
                           
            public static readonly string fragmentShaderSource = @"
                #version 430 core

                out vec4 color;
                layout (location = 2) out ivec2 data;
                out float extra;

                in BLOCK0
                {
                    vec2 tc;
                    vec4 color;
                    flat int foo;
                } fs_in0;

                in BLOCK1
                {
                    vec3 normal[4];
                    flat ivec3 layers;
                    double bar;
                } fs_in1;

                void main(void)
                {
                  //dummy assigns so compiler dosn't optomize them away
                  data = ivec2( 0, 1 );
                  extra = 23.4f;
                    
                  color = vec4( 0.0, 0.8, 1.0, 1.0 );
                }
                ";
        }

        private int _programPipeline;
        private int  _vao;

        //-----------------------------------------------------------------------------------------
        public Listing_06L01_06L04() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Program Information",  
                    GameWindowFlags.Default, DisplayDevice.Default, 4, 3, 
                    GraphicsContextFlags.Debug )
        {
        }

        //-----------------------------------------------------------------------------------------
        private void _PrintInterfaceInformation( int program, ProgramInterface iface )
        {
            string[] propName = new string[] { "type", "location", "array size" };
            System.Text.StringBuilder name = new  System.Text.StringBuilder();
            int[] myParams = new int[4] {0,0,0,0};
            ProgramProperty[] myProps = new ProgramProperty[] { ProgramProperty.Type, 
                                                                ProgramProperty.Location, 
                                                                ProgramProperty.ArraySize };
            int interfaces;
            GL.GetProgramInterface( program, iface, ProgramInterfaceParameter.ActiveResources, out interfaces );

            for( int i = 0; i < interfaces; i++ )
            {
                int dummy;
                string typeName;

                GL.GetProgramResourceName( program, iface, i, 1024, out dummy, name );
                GL.GetProgramResource( program, iface, i, 3, myProps, 3, out dummy, myParams );

                if( !Statics.typeNames.TryGetValue( (All)myParams[0], out typeName ) )
                    { typeName = Statics.typeNames[All.None]; }

                if( myParams[2] != 0 )
                {
                    Console.WriteLine( String.Format( "Index {0}: {1,-7} {2,-16}[{3}] @ location {4}.",
                                                      i, typeName, name, myParams[2], myParams[1] ) );
                }
                else
                {
                    Console.WriteLine( String.Format( "Index {0}: {1,-7} {2,-16} @ location {3}.",
                                                      i, typeName, name, myParams[1] ) );
                }
            }        
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitProgram()
        {
            int vertexShader, fragmentShader;
            int vertexProgram, fragmentProgram;

            // Create a vertex shader
            vertexShader = GL.CreateShader( ShaderType.VertexShader );
            
            // Attach source and compile
            GL.ShaderSource( vertexShader, Statics.vertexShaderSource );
            GL.CompileShader( vertexShader );

            // [Listing 6.1]
            // Did our program compile correctly? Try uncommenting the last 'gl_Position =' line in
            // the vertex shader source and see what error message is displayed on the console
            Console.WriteLine( GL.GetShaderInfoLog( (int)vertexShader ) );

            // Create a program for our vertex stage and attach the vertex shader to it
            vertexProgram = GL.CreateProgram();
            GL.AttachShader( vertexProgram, vertexShader );

            // Important part - set the GL_PROGRAM_SEPARABLE flag to GL_TRUE *then* link
            GL.ProgramParameter( vertexProgram, ProgramParameterName.ProgramSeparable, (int)All.True );
            GL.LinkProgram( vertexProgram );

            // [Listing 6.2]
            // Did our program linked correctly? Try commenting out the declaration block for
            // gl_PerVertex in our vertex shader and see what error message is displayed on the 
            // console
            Console.WriteLine( GL.GetProgramInfoLog( vertexProgram ) );
            
            // Delete the vertex shader as the program has it now
            GL.DeleteShader( vertexShader );

            // Now do the same with a fragment shader
            fragmentShader = GL.CreateShader( ShaderType.FragmentShader );
            GL.ShaderSource( (int)fragmentShader, Statics.fragmentShaderSource );
            GL.CompileShader( fragmentShader );
            fragmentProgram = GL.CreateProgram();
            GL.AttachShader( fragmentProgram, fragmentShader );
            GL.ProgramParameter( fragmentProgram, ProgramParameterName.ProgramSeparable, (int)All.True );
            GL.LinkProgram( fragmentProgram );
            Console.WriteLine( GL.GetProgramInfoLog( fragmentProgram ) );

            GL.DeleteShader( fragmentShader );

            // [Listing 6.3]
            // The program pipeline represents the collection of programs in use:
            // Generate the name for it here.
            GL.GenProgramPipelines( 1, out _programPipeline );

            // [Listing 6.4]
            Console.WriteLine( "====={ FRAGMENT SHADER INPUTS }=====" );
            this._PrintInterfaceInformation( fragmentProgram, ProgramInterface.ProgramInput );   
            Console.WriteLine( "====={ FRAGMENT SHADER OUTPUTS }=====" );
            this._PrintInterfaceInformation( fragmentProgram, ProgramInterface.ProgramOutput );   
            Console.WriteLine( "====================================" );

            // Now, use the vertex shader from the first program and the fragment shader from the 
            // second program.
            GL.UseProgramStages( _programPipeline, ProgramStageMask.VertexShaderBit, vertexProgram );
            GL.UseProgramStages( _programPipeline, ProgramStageMask.FragmentShaderBit, fragmentProgram );

            GL.BindProgramPipeline( _programPipeline );

            // We can delete the program objects now as the pipeline has them now
            GL.DeleteProgram( vertexProgram );
            GL.DeleteProgram( fragmentProgram );

            return true;
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            this._InitProgram();

            GL.GenVertexArrays( 1, out _vao );
            GL.BindVertexArray( _vao );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteProgramPipeline( _programPipeline );
            GL.DeleteVertexArrays( 1, ref _vao );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorBlack );
            GL.DrawArrays( PrimitiveType.Triangles, 0, 3 );
            SwapBuffers();
        }
    }
}
