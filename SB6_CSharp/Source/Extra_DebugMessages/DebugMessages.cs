using System;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;

using System.Collections.Generic;

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
    class DebugMessages : GameWindow
    {
        static class Statics
        {
            public static readonly float[] colorBlack = { 0.0f, 0.0f, 0.0f, 1.0f };
           
            public static readonly string vertexShaderSource = @"
                #version 430 core
                void main( void )
                {
                    const vec4 vertices[] = vec4[]( vec4(  0.25, -0.25, 0.5, 1.0 ),
                                                    vec4( -0.25, -0.25, 0.5, 1.0 ),
                                                    vec4(  0.25,  0.25, 0.5, 1.0 ));
                    gl_Position = vertices[gl_VertexID];
                }
                ";
                            
            public static readonly string fragmentShaderSource = @"
                #version 430 core
                out vec4 color;
                void main( void )
                {
                    color = vec4( 0.0, 0.8, 1.0, 1.0 );
                }
                ";
        }

        private uint _shaderProgram;
        private uint _vao;

	    // CALLBACK DELEGATE
	    DebugProc _debugCallbackInstance = DebugCallback;
    
        //-----------------------------------------------------------------------------------------
        public DebugMessages() 
            : base( 800, 600, GraphicsMode.Default, "Test Debug Messages",  
                    GameWindowFlags.Default, DisplayDevice.Default, 4, 3, 
                    GraphicsContextFlags.Debug | GraphicsContextFlags.ForwardCompatible )
        {
        }

        //-----------------------------------------------------------------------------------------
        static void DebugCallback( DebugSource source, DebugType type, int id, DebugSeverity severity, int length, 
                                            IntPtr message, IntPtr userParam )
	    {
            string msg = System.Runtime.InteropServices.Marshal.PtrToStringAnsi( message );
            Console.WriteLine( "{0}[GL] {1}; {2}; {3}; {4}; {5}", Program.ElapsedTimeSeconds, source, type, id, severity, msg );
	    }

        //-----------------------------------------------------------------------------------------
        private bool _InitProgram()
        {
            uint vertexShader, fragmentShader;

            // Create and compile vertex shader
            vertexShader = (uint)GL.CreateShader( ShaderType.VertexShader );
            GL.ShaderSource( (int)vertexShader, Statics.vertexShaderSource );
            GL.CompileShader( vertexShader );

            // Create and compile fragment shader
            fragmentShader = (uint)GL.CreateShader( ShaderType.FragmentShader );
            GL.ShaderSource( (int)fragmentShader, Statics.fragmentShaderSource );
            GL.CompileShader( fragmentShader );

            // Create program, attach shaders to it, and link it
            _shaderProgram = (uint)GL.CreateProgram();
            GL.AttachShader( _shaderProgram, vertexShader );
            GL.AttachShader( _shaderProgram, fragmentShader );
            GL.LinkProgram( _shaderProgram );

            GL.UseProgram( _shaderProgram );

            // Delete the shaders as the program has them now
            GL.DeleteShader( vertexShader );
            GL.DeleteShader( fragmentShader );

            GL.GenVertexArrays( 1, out _vao );
            GL.BindVertexArray( _vao );

            return true;
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            this.Context.ErrorChecking = false;

//            GLDirect.PrintAllExtentions();

            GL.Enable( EnableCap.DebugOutput );
            GL.Enable( EnableCap.DebugOutputSynchronous );

            int iFlagValue;
            GL.GetInteger( GetPName.ContextFlags, out iFlagValue );
            if( iFlagValue != 0 )
            {
                Console.WriteLine( String.Format( "Debug context present!!!, flags = {0}", iFlagValue ) );
            }
            
            if( GL.IsEnabled( EnableCap.DebugOutput ) )
            {
                Console.WriteLine( "DebugOutput is enabled!!!" );
            }
            
            if( GL.IsEnabled( EnableCap.DebugOutputSynchronous ) )  // InvalidEnum exception thrown
            {
                Console.WriteLine( "DebugOutputSynchronous is enabled!!!" );
            }

            GL.DebugMessageCallback( _debugCallbackInstance, IntPtr.Zero );
            
//            int iReportEverything = 0;
//            //int[] reportEverything = new int[] {};
//            GL.DebugMessageControl( DebugSourceControl.DontCare, DebugTypeControl.DontCare, 
//                                    DebugSeverityControl.DontCare, 0, ref iReportEverything, true );

            this.Context.ErrorChecking = true;

            _InitProgram(); 
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteProgram( _shaderProgram );
            GL.DeleteVertexArrays( 1, ref _vao );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorBlack );
            GL.DrawArrays( PrimitiveType.Triangles, 0, 3 );

            string msg = "Debug message from OnRenderFrame()!!!";
            GL.DebugMessageInsert( DebugSourceExternal.DebugSourceApplication, DebugType.DebugTypeOther, 1234, 
                                   DebugSeverity.DebugSeverityHigh, msg.Length, msg );

//            this.Context.ErrorChecking = false;
//            OpenTK.Graphics.OpenGL.GL.Translate( 0.0f, 0.0f, 0.0f );
//            this.Context.ErrorChecking = true;

            SwapBuffers();
        }
    }
}
