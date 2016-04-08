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
    class Listing_06L05_06L06 : GameWindow
    {
        private int _shaderProgram;
        private int  _vao;

        private struct Subroutines
        {
            public int myFunction1;
            public int myFunction2;
        }
        private Subroutines _subroutines;

        private struct Uniforms
        {
            public int subroutine1;
        }
        private Uniforms _uniforms;

        //-----------------------------------------------------------------------------------------
        public Listing_06L05_06L06() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Shader Subroutines", 
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            int[] shaders = new int[3];
                
            shaders[0] = Framework.Shader.Load( Program.BasePath + @"Source\Listing_06L05_06L06\subroutines.vs.glsl", 
                                                ShaderType.VertexShader );
            shaders[1] = Framework.Shader.Load( Program.BasePath + @"Source\Listing_06L05_06L06\subroutines.fs.glsl", 
                                                ShaderType.FragmentShader );
            
            _shaderProgram = Framework.Shader.Link( shaders, 2 );

            _subroutines.myFunction1 = GL.GetSubroutineIndex( _shaderProgram, ShaderType.FragmentShader, "myFunction1" );
            _subroutines.myFunction2 = GL.GetSubroutineIndex( _shaderProgram, ShaderType.FragmentShader, "myFunction2" );
            
            _uniforms.subroutine1 = GL.GetSubroutineUniformLocation( _shaderProgram, ShaderType.FragmentShader, 
                                                                     "mySubroutineUniform" );

            GL.GenVertexArrays( 1, out _vao );
            GL.BindVertexArray( _vao );
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
            int i = (int)Program.ElapsedTimeSeconds;
            int subroutine = (i % 2 == 0) ? _subroutines.myFunction1 : _subroutines.myFunction2;

            GL.UseProgram( _shaderProgram );

            GL.UniformSubroutines( ShaderType.FragmentShader, 1, ref subroutine ); 
            GL.DrawArrays( PrimitiveType.TriangleStrip, 0, 4 );

            SwapBuffers();
        }

    }
}
