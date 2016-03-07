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
    class Example_05L38_05L39 : GameWindow
    {
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// The Statics container class holding class-wide static globals
        /// </summary>
        static class Statics
        {
            public static readonly float[] colorGreen = { 0.0f, 0.2f, 0.0f, 1.0f };
        }

        private int _shaderProgramName;
        private int _vertexArrayName;

        private int _textureName;

        //-----------------------------------------------------------------------------------------
        public Example_05L38_05L39() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Texture Coordinates", 
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

                void main(void)
                {
                    const vec4 vertices[] = vec4[](vec4(-1.0, -1.0, 0.5, 1.0),
                                                   vec4( 1.0, -1.0, 0.5, 1.0),
                                                   vec4(-1.0,  1.0, 0.5, 1.0),
                                                   vec4( 1.0,  1.0, 0.5, 1.0));
                    gl_Position = vertices[gl_VertexID];
                }
                ";
                
            // Source code for fragment shader
            string fragmentShaderSource = @"
                #version 430 core

                uniform sampler2D s;
                uniform float     exposure;

                out vec4 color;

                void main(void)
                {
                    color = texture(s, gl_FragCoord.xy / textureSize(s, 0)) * exposure;
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
        private bool _InitVao()
        {
            // Create VAO object to hold vertex shader inputs and attach it to our context. As 
            // OpenGL requires the VAO object (whether or not it's used) we do this here.
            GL.GenVertexArrays( 1, out _vertexArrayName );
            GL.BindVertexArray( _vertexArrayName );
            
            return true;
        }

        //-----------------------------------------------------------------------------------------
        private void _InitTextures()
        {
            // Generate a name for the texture
            GL.GenTextures( 1, out _textureName );

            // Load texture from file
            // Note: If debugging from Visual Studio, debug command line options must be set to:
            //   -b "..\..\..\\"
            Framework.KTX.Load( Program.BasePath + @"Media\Textures\Tree.ktx", (uint)_textureName );

            // Now bind it to the context using the GL_TEXTURE_2D binding point
            GL.BindTexture( TextureTarget.Texture2D, _textureName );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            this._InitProgram();
            this._InitTextures();
            this._InitVao();
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteProgram( _shaderProgramName );
            GL.DeleteVertexArrays( 1, ref _vertexArrayName );
            GL.DeleteTextures( 1, ref _textureName );
        }
        
        //-----------------------------------------------------------------------------------------
        //Our rendering function
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            //Clear the window with given color
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorGreen );
 
            //Use the program object we created earlier for rendering
            GL.UseProgram( _shaderProgramName );

            GL.Uniform1( 0, (float)(Math.Sin( Program.ElapsedTimeSeconds ) * 16.0 + 16.0) );

            //Draw one triangle
            GL.DrawArrays( PrimitiveType.TriangleStrip, 0, 4 );
            
            SwapBuffers();
        }
    }
}
