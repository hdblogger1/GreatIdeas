﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using System.Runtime.InteropServices;

namespace SB6_CSharp.Framework
{
    public static class SB6Debug
    {
        public static byte[] GetBuffer( BufferTarget bufferTarget, int numOfBytes )
        {
            byte[] bufferData = new byte[numOfBytes];

            IntPtr ptr = GL.MapBuffer( bufferTarget, BufferAccess.ReadOnly );
            Marshal.Copy( ptr, (byte[])bufferData, 0, numOfBytes );
            GL.UnmapBuffer( bufferTarget );

            return bufferData;
        }

        public static void ToOutput<T>( T data, string filename = "output.bin" )
        {
            byte[] buffer;
            TypeUtils.ToBytes<T>( data, out buffer );
            ByteArrayToOutput( buffer, filename );
        }

        public static void ToOutput<T>( T[] dataArray, string filename = "output.bin" )
        {
            byte[] buffer;
            TypeUtils.ToBytes<T>( dataArray, out buffer );
            ByteArrayToOutput( buffer, filename );
        }

        public static void ByteArrayToOutput( byte[] bytes, string filename = "output.bin" )
        {
            System.IO.FileStream fs = new System.IO.FileStream( filename, System.IO.FileMode.Create );
            System.IO.BinaryWriter file = new System.IO.BinaryWriter( fs );
            foreach( byte b in bytes )
            {
                file.Write( b );
            }
            file.Close();
            fs.Close();
        }
        
        public static byte[] ByteArrayFromOutput( string filename = "output.bin" )
        {
            System.IO.FileStream fs = new System.IO.FileStream( filename, System.IO.FileMode.Open );
            System.IO.BinaryReader file = new System.IO.BinaryReader( fs );

            byte[] dataBytes = file.ReadBytes( (int)file.BaseStream.Length );

            file.Close();
            fs.Close();

            return dataBytes;
        }
    }
}
