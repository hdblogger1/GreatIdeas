//=================================================================================================
// The code herein has been adapted from the book "OpenGL SuperBible - Sixth Edition" and its
// accompanying C++ example source code. Please see 'Copyright_SB6.txt' for copyright information.
//=================================================================================================
using System;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
    class Listing_05L45_05L46 : GameWindow
    {
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// The Statics container class holding class-wide static globals
        /// </summary>
        static class Statics
        {
            public static readonly float[] colorBlack = { 0.0f, 0.0f, 0.0f, 1.0f };
        }

        private struct ShaderPrograms
        {
            public int Clear;
            public int Append;
            public int Resolve;
        }
        private ShaderPrograms _shaderPrograms;

        private struct Buffers
        {
            public int Uniforms;
            public int Fragment;
            public int AtomicCounter;
        }
        private Buffers _buffers;

        private struct Textures
        {
            public int HeadPointerImage;
        }
        private Textures _textures;

        private struct Models
        {
            public Framework.SBM6Model Dragon;
        }
        private Models _models;

        private struct UniformNames
        {
            public int mvp;
        }
        private UniformNames _uniformNames;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct UniformsBlock_s
        {
            Matrix4 mvMatrix;
            Matrix4 viewMatrix;
            Matrix4 projMatrix;
        }

        private int _vao;

        //-----------------------------------------------------------------------------------------
        public Listing_05L45_05L46() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Fragment List", 
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }
        
        //-----------------------------------------------------------------------------------------
        private bool _InitProgram()
        {
            int[] shaders = new int[2];
                
            shaders[0] = Framework.Shader.Load( Program.BasePath + @"Source\Listing_05L45_05L46\clear.vs.glsl", 
                                                ShaderType.VertexShader );
            shaders[1] = Framework.Shader.Load( Program.BasePath + @"Source\Listing_05L45_05L46\clear.fs.glsl", 
                                                ShaderType.FragmentShader );
            _shaderPrograms.Clear = Framework.Shader.Link( shaders, 2, true );


            shaders[0] = Framework.Shader.Load( Program.BasePath + @"Source\Listing_05L45_05L46\append.vs.glsl", 
                                                ShaderType.VertexShader );
            shaders[1] = Framework.Shader.Load( Program.BasePath + @"Source\Listing_05L45_05L46\append.fs.glsl", 
                                                ShaderType.FragmentShader );
            _shaderPrograms.Append = Framework.Shader.Link( shaders, 2, true );


            shaders[0] = Framework.Shader.Load( Program.BasePath + @"Source\Listing_05L45_05L46\resolve.vs.glsl", 
                                                ShaderType.VertexShader );
            shaders[1] = Framework.Shader.Load( Program.BasePath + @"Source\Listing_05L45_05L46\resolve.fs.glsl", 
                                                ShaderType.FragmentShader );
            _shaderPrograms.Resolve = Framework.Shader.Link( shaders, 2, true );

            _uniformNames.mvp = GL.GetUniformLocation( _shaderPrograms.Append, "mvp" );

            GL.GenVertexArrays( 1, out _vao );
            GL.BindVertexArray( _vao );

            return true;
        }
 
        //-----------------------------------------------------------------------------------------
        private bool _InitBuffers()
        {            
            GL.GenBuffers( 1, out _buffers.Uniforms );
            GL.BindBuffer( BufferTarget.UniformBuffer, _buffers.Uniforms );
            GL.BufferData( BufferTarget.UniformBuffer, (IntPtr)Framework.TypeUtils.SizeOf<UniformsBlock_s>(), 
                           IntPtr.Zero, BufferUsageHint.DynamicDraw );

            GL.GenBuffers( 1, out _buffers.Fragment );
            GL.BindBuffer( BufferTarget.ShaderStorageBuffer, _buffers.Fragment );
            GL.BufferData( BufferTarget.ShaderStorageBuffer, (IntPtr)(1024 * 1024 * 16), 
                           IntPtr.Zero, BufferUsageHint.DynamicCopy );

            GL.GenBuffers( 1, out _buffers.AtomicCounter );
            GL.BindBuffer( BufferTarget.AtomicCounterBuffer, _buffers.AtomicCounter );
            GL.BufferData( BufferTarget.AtomicCounterBuffer, (IntPtr)sizeof(uint), 
                           IntPtr.Zero, BufferUsageHint.DynamicCopy );
            return true;
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitTextures()
        {
            GL.GenTextures( 1, out _textures.HeadPointerImage );
            GL.BindTexture( TextureTarget.Texture2D, _textures.HeadPointerImage );
            GL.TexStorage2D( TextureTarget2d.Texture2D, 1, SizedInternalFormat.R32ui, 1024, 1024 );

            return true;
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitModels()
        {
            _models.Dragon = new Framework.SBM6Model();
            _models.Dragon.Load( Program.BasePath + @"Media\Objects\dragon.sbm" );

            return true;
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            _InitTextures();
            _InitBuffers();
            _InitModels();
            _InitProgram();
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteProgram( _shaderPrograms.Append );
            GL.DeleteProgram( _shaderPrograms.Clear );
            GL.DeleteProgram( _shaderPrograms.Resolve );
            GL.DeleteVertexArray( _vao );
            
            GL.DeleteBuffer( _buffers.AtomicCounter );
            GL.DeleteBuffer( _buffers.Fragment );
            GL.DeleteBuffer( _buffers.Uniforms );
            
            GL.DeleteTexture( _textures.HeadPointerImage );

            _models.Dragon.Delete();
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            float currentTime = (float)Program.ElapsedTimeSeconds;

            GL.Viewport( 0, 0, Width, Height );
            GL.MemoryBarrier( MemoryBarrierFlags.ShaderImageAccessBarrierBit
                              | MemoryBarrierFlags.AtomicCounterBarrierBit
                              | MemoryBarrierFlags.ShaderStorageBarrierBit );

            GL.UseProgram( _shaderPrograms.Clear );
            GL.BindVertexArray( _vao );
            GL.DrawArrays( PrimitiveType.TriangleStrip, 0, 4 );

            GL.UseProgram( _shaderPrograms.Append );

            Matrix4 modelMatrix = Matrix4.CreateScale( 7.0f );
            Vector3 viewPosition = new Vector3( (float)Math.Cos( currentTime * 0.35f ) * 120.0f,
                                                (float)Math.Cos( currentTime * 0.4f ) * 30.0f,
                                                (float)Math.Sin( currentTime * 0.35f ) * 120.0f );
            Matrix4 viewMatrix = Matrix4.LookAt( viewPosition,
                                                 new Vector3( 0.0f, 30.0f, 0.0f ),
                                                 new Vector3( 0.0f, 1.0f, 0.0f ) );  
            Matrix4 projMatrix = Matrix4.CreatePerspectiveFieldOfView( MathHelper.DegreesToRadians( 50.0f ),
                                                                       (float)Width / (float)Height,
                                                                       0.1f, 1000.0f );
            Matrix4 mvpMatrix = modelMatrix * viewMatrix * projMatrix; 

            GL.UniformMatrix4( _uniformNames.mvp, false, ref mvpMatrix );

            uint zero = 0;
            GL.BindBufferBase( BufferRangeTarget.AtomicCounterBuffer, 0, _buffers.AtomicCounter );
            GL.BufferSubData( BufferTarget.AtomicCounterBuffer, IntPtr.Zero, (IntPtr)sizeof(uint), ref zero );

            GL.BindBufferBase( BufferRangeTarget.ShaderStorageBuffer, 0, _buffers.Fragment );

            GL.BindImageTexture( 0, _textures.HeadPointerImage, 0, false, 0, 
                                 TextureAccess.ReadWrite, SizedInternalFormat.R32ui );

            GL.MemoryBarrier( MemoryBarrierFlags.ShaderImageAccessBarrierBit
                              | MemoryBarrierFlags.AtomicCounterBarrierBit
                              | MemoryBarrierFlags.ShaderStorageBarrierBit );
            
            _models.Dragon.Render();

            GL.MemoryBarrier( MemoryBarrierFlags.ShaderImageAccessBarrierBit
                              | MemoryBarrierFlags.AtomicCounterBarrierBit
                              | MemoryBarrierFlags.ShaderStorageBarrierBit );

            GL.UseProgram( _shaderPrograms.Resolve );

            GL.BindVertexArray( _vao );
            GL.MemoryBarrier( MemoryBarrierFlags.ShaderImageAccessBarrierBit
                              | MemoryBarrierFlags.AtomicCounterBarrierBit
                              | MemoryBarrierFlags.ShaderStorageBarrierBit );

            GL.DrawArrays( PrimitiveType.TriangleStrip, 0, 4 );

            SwapBuffers();
        }
    }
}
