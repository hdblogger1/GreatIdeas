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

namespace SB6_CSharp
{
    //=============================================================================================
    /// <summary>
    /// Our OpenTK GameWindow derived application class which takes care of creating a window, 
    /// handling input, and displaying the rendered results to the user.
    /// </summary>
    class Listing_05L33_05L35 : GameWindow
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
        public Listing_05L33_05L35() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Simple Texturing", 
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
                    // Declare a hard-coded array of positions
                    const vec4 vertices[3] = vec4[3](vec4( 0.75, -0.75, 0.5, 1.0),
                                                     vec4(-0.75, -0.75, 0.5, 1.0),
                                                     vec4( 0.75,  0.75, 0.5, 1.0));
                    // Index into our array using gl_VertexID
                    gl_Position = vertices[gl_VertexID];
                }
                ";
                
            // Source code for fragment shader
            string fragmentShaderSource = @"
                #version 430 core

                uniform sampler2D s;

                out vec4 color;

                void main(void)
                {
                    color = texture(s, gl_FragCoord.xy / textureSize(s, 0));
                    //color = texelFetch(s, ivec2(gl_FragCoord.xy), 0);
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

            // Now bind it to the context using the GL_TEXTURE_2D binding point
            GL.BindTexture( TextureTarget.Texture2D, _textureName );

            // Specify the amount of storage we want to use for the texture
            GL.TexStorage2D( TextureTarget2d.Texture2D,   // 2D texture
                             8,                           // 8 mipmap levels
                             SizedInternalFormat.Rgba32f, // 32-bit floating-point RGBA data
                             256, 256);                   // 256 x 256 texels

            // Define some data to upload into the texture
            float[] data = new float[256 * 256 * 4];

            // _GenerateTexture() is a function that fills memory with image data
            this._GenerateTexture( 256, 256, ref data );

            // Assume the texture is already bound to the GL_TEXTURE_2D target
            GL.TexSubImage2D( TextureTarget.Texture2D,  // 2D texture
                              0,                        // Level 0
                              0, 0,                     // Offset 0, 0
                              256, 256,                 // 256 x 256 texels, replace entire image
                              PixelFormat.Rgba,         // Four channel data
                              PixelType.Float,          // Floating point data
                              data);                    // Pointer to data
        }

        //-----------------------------------------------------------------------------------------
        private void _GenerateTexture( int width, int height, ref float[] data )
        {
            int x, y;

            for( y = 0; y < height; y++ )
            {
                for( x = 0; x < width; x++ )
                {
                    data[(y * width + x) * 4 + 0] = (float)((x & y) & 0xFF) / 255.0f;
                    data[(y * width + x) * 4 + 1] = (float)((x | y) & 0xFF) / 255.0f;
                    data[(y * width + x) * 4 + 2] = (float)((x ^ y) & 0xFF) / 255.0f;
                    data[(y * width + x) * 4 + 3] = 1.0f;
                }
            }
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

            //Draw one triangle
            GL.DrawArrays( PrimitiveType.Triangles, 0, 3 );
            
            SwapBuffers();
        }
    }
}
