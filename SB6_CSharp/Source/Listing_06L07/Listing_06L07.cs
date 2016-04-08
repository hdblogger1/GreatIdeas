//=================================================================================================
// The code herein has been adapted from the book "OpenGL SuperBible - Sixth Edition" and its
// accompanying C++ example source code. Please see 'Copyright_SB6.txt' for copyright information.
//=================================================================================================
using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

using SB6_CSharp.Framework;

namespace SB6_CSharp
{
    //=============================================================================================
    /// <summary>
    /// Our OpenTK GameWindow derived application class which takes care of creating a window, 
    /// handling input, and displaying the rendered results to the user.
    /// </summary>
    class Listing_06L07 : GameWindow
    {
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// The Statics container class holding class-wide static globals
        /// </summary>
        static class Statics
        {
            public static readonly float[] colorRed = { 1.0f, 0.0f, 0.0f, 1.0f };

            public static readonly string vertexShaderSource = @"
                #version 430 core
                void main(void)
                {
                    gl_Position = vec4(0.0, 0.0, 0.5, 1.0);
                }
                ";
                           
            public static readonly string fragmentShaderSource = @"
                #version 430 core
                out vec4 color;
                void main(void)
                {
                    color = vec4(0.0, 0.8, 1.0, 1.0);
                }
                ";
        }

        // OpenGL object names attached to resources that require 'deleting'. (see OnUnload())
        private int _shaderProgramName;
        private int _vao;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ProgramBlobHeader
        {
            public BinaryFormat format;
            public int          length;
        }
        private ProgramBlobHeader _programBlobHeader;

        //-----------------------------------------------------------------------------------------
        public Listing_06L07() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Saving and Loading Programs",
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }

        //-----------------------------------------------------------------------------------------
        private bool _writeProgramBlob( int shaderProgramName, string fileName )
        {
            // Get the expected size of the program binary
            int binarySize;
            GL.GetProgram( shaderProgramName, (GetProgramParameterName)0x8741, out binarySize );

            // Allocate some memory to store the program binary
            byte[] programData = new byte[binarySize];

            // Retrieve the binary from the program object
            GL.GetProgramBinary<byte>( shaderProgramName, binarySize, 
                                       out _programBlobHeader.length, 
                                       out _programBlobHeader.format,
                                       programData );

            // Save program binary to disk
            FileStream fs = new FileStream( fileName, FileMode.Create );
            BinaryWriter bw = new BinaryWriter( fs );

            byte[] header;
            TypeUtils.ToBytes<ProgramBlobHeader>( _programBlobHeader, out header );
            bw.Write( header );
            bw.Write( programData );
            bw.Close();
            fs.Close();
            
            return true;
        }

        //-----------------------------------------------------------------------------------------
        private bool _readProgramBlob( int shaderProgramName, string fileName  )
        {            
            // Get program binary from disk
            FileStream fs = new FileStream( fileName, FileMode.Open );
            BinaryReader br = new BinaryReader( fs );

            byte[] fileBytes = br.ReadBytes( (int)br.BaseStream.Length );

            br.Close();
            fs.Close();

            // Get header
            ProgramBlobHeader header;
            int dataOffset = TypeUtils.FromBytes<ProgramBlobHeader>( fileBytes, 0, out header );

            // Reload program object from program data
            GL.ProgramBinary( shaderProgramName, header.format, ref fileBytes[dataOffset], header.length );
            
            return true;
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            //check if program binary exists on disk. if it does load then delete it. If it dosn't
            //create it. This should cause every other run to use the store program binary created
            //from the previous run
            string binaryFileName = Program.BasePath + @"Source\Listing_06L07\programBinary.bin";

            // Create a program object
            _shaderProgramName = GL.CreateProgram();

            if( File.Exists( binaryFileName ) )
            {
                _readProgramBlob( _shaderProgramName, binaryFileName );

                // Delete our saved program binary so next run can recreate it.
                File.Delete( binaryFileName );
            }
            else
            {
                // Create and compile vertex and fragment shaders
                int vertexShaderName   = Framework.Shader.Compile( ShaderType.VertexShader, Statics.vertexShaderSource );
                int fragmentShaderName = Framework.Shader.Compile( ShaderType.FragmentShader, Statics.fragmentShaderSource );

                // Attach shaders to program object
                GL.AttachShader( _shaderProgramName, vertexShaderName );
                GL.AttachShader( _shaderProgramName, fragmentShaderName );

                // Set the binary retrievable hint and link the program
                GL.ProgramParameter( _shaderProgramName, ProgramParameterName.ProgramBinaryRetrievableHint, 1 );
                GL.LinkProgram( _shaderProgramName );
           
                // Delete the shaders as the program has them now
                GL.DeleteShader( vertexShaderName );
                GL.DeleteShader( fragmentShaderName );

                _writeProgramBlob( _shaderProgramName, binaryFileName );
            }

            GL.GenVertexArrays( 1, out _vao );
            GL.BindVertexArray( _vao );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteProgram( _shaderProgramName );
            GL.DeleteVertexArray( _vao );
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorRed );
 
            // Use the program object we created earlier for rendering
            GL.UseProgram( _shaderProgramName );

            // The following sets the point size to at least 64 pixels
            GL.PointSize( 40.0f );

            // Draw one point
            GL.DrawArrays( PrimitiveType.Points, 0, 1 );

            SwapBuffers();
        }
    }
}
