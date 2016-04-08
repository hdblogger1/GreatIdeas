using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace SB6_CSharp.Framework
{
    public class TypeUtils
    {
        //-----------------------------------------------------------------------------------------
        public static int SizeOf<T>()
        { 
            return Marshal.SizeOf( typeof(T) ); 
        }
        
        //-----------------------------------------------------------------------------------------
        public static int FromBytes<T>( byte[] bytes, int offset, out T data )
        {
            int sizeConverted = 0;
            data = default(T);

            // Pin the managed memory, copy out the data and then unpin it
            GCHandle handle = GCHandle.Alloc( bytes, GCHandleType.Pinned );
            try 
            {
                IntPtr ptr = handle.AddrOfPinnedObject();
                data = (T)Marshal.PtrToStructure( IntPtr.Add( ptr, offset ), typeof(T) );
                sizeConverted = SizeOf<T>();
            } 
            finally 
            {
                handle.Free();
            }

            return sizeConverted;
        }

        //-----------------------------------------------------------------------------------------
        public static int FromBytes<T>( byte[] bytes, int offset, out T[] data )
        {
            int sizeConverted = 0;
            data = default(T[]);            
            int bytesRemaining = bytes.Length;
            
            // Pin the managed memory, copy out the data and then unpin it
            GCHandle handle = GCHandle.Alloc( bytes, GCHandleType.Pinned );
            try 
            {
                List<T> elements   = new List<T>();
                int elementSize    = SizeOf<T>();
                IntPtr ptr         = handle.AddrOfPinnedObject();

                while( bytesRemaining >= elementSize )
                {
                    T element = (T)Marshal.PtrToStructure( IntPtr.Add( ptr, offset + sizeConverted ), typeof(T) );
                    elements.Add( element );
                    sizeConverted += elementSize;
                    bytesRemaining -= elementSize;
                }

                data = elements.ToArray();
            } 
            finally 
            {
                handle.Free();
            }

            return sizeConverted;
        }

        //-----------------------------------------------------------------------------------------
        public static int ToBytes<T>( T data, out byte[] bytes )
        {
            int sizeConverted = 0;
            bytes = new byte[SizeOf<T>()];

            // Pin the managed memory, copy out the data and then unpin it
            GCHandle handle = GCHandle.Alloc( bytes, GCHandleType.Pinned );
            try 
            {
                IntPtr bytesPtr = handle.AddrOfPinnedObject();
                Marshal.StructureToPtr( data, bytesPtr, false );
                sizeConverted = SizeOf<T>();
            } 
            finally 
            {
                handle.Free();
            }

            return sizeConverted; 
        }

        //-----------------------------------------------------------------------------------------
        public static int ToBytes<T>( T[] data, out byte[] bytes )
        {
            int sizeConverted = 0;
            int elementSize = SizeOf<T>();
            bytes = new byte[elementSize * data.Length];

            // Pin the managed memory, copy out the data and then unpin it
            GCHandle handle = GCHandle.Alloc( bytes, GCHandleType.Pinned );
            try 
            {
                IntPtr bytesPtr = handle.AddrOfPinnedObject();

                foreach( T element in data )
                {
                    Marshal.StructureToPtr( element, bytesPtr + sizeConverted, false );
                    sizeConverted += elementSize;
                }
            } 
            finally 
            {
                handle.Free();
            }

            return sizeConverted; 
        }

        //-----------------------------------------------------------------------------------------
        public static int FromBinaryReader<T>( BinaryReader reader, out T obj )
        {
            byte[] bytes = reader.ReadBytes( SizeOf<T>() );
            return FromBytes<T>( bytes, 0, out obj );
        }
        
        //-----------------------------------------------------------------------------------------
        public static int ToBinaryWriter<T>( BinaryWriter writer, T obj )
        {
            byte[] bytes;
            int sizeConverted = ToBytes<T>( obj, out bytes );
            if( sizeConverted > 0 ) { writer.Write( bytes ); }
            return sizeConverted;
        }

        //-----------------------------------------------------------------------------------------
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Byteable<T>
        { 
            public static int FromBytes( byte[] bytes, int offset, out T obj ) 
                { return TypeUtils.FromBytes<T>( bytes, offset, out obj ); }
                       
            public static int ToBytes( T obj, out byte[] bytes ) 
                { return TypeUtils.ToBytes<T>( obj, out bytes ); }        
        }

    }
}
