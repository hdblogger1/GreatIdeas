using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace SB6_CSharp.Framework
{
    //=============================================================================================
    // Super-Bible Model file format
    public class SBM6Model
    {
        //=========================================================================================
        private class SBM6ModelException : Exception
        {
            public SBM6ModelException() { }
            public SBM6ModelException( string msg ) : base( msg ) { }
            public SBM6ModelException( string msg, Exception inner ) : base( msg, inner ) { }
        }

        //=========================================================================================
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private class SBM6Header : TypeUtils.Byteable<SBM6Header>
        {
            public uint       Magic;
            public uint       Size;
            public uint       NumChunks;
            public uint       Flags;
        }

        //=========================================================================================
        private enum ChunkType_e:uint
        {
            IndexData     = 0x58444E49, // ascii XDNI ==> "INDX"
            VertexData    = 0x58545256, // ascii XTRV ==> "VRTX"
            VertexAttribs = 0x42525441, // ascii BRTA ==> "ATRB"
            SubObjectList = 0x54534c4F, // ascii TSLO ==> "OLST"
            Comment       = 0x544E4D43, // ascii TNMC ==> "CMNT"
        }

        //=========================================================================================
        private enum VertexAttribFlag_e:uint
        {
            Normalized  = 1,
            Integer     = 2,
        }

        //=========================================================================================
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private class ChunkHeader : TypeUtils.Byteable<ChunkHeader>
        {
            public ChunkType_e Type;
            public uint        Size;
        }

        //=========================================================================================
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private class ChunkIndexData : TypeUtils.Byteable<ChunkIndexData>
        {
            public ChunkHeader Header;
            public uint        IndexType;
            public uint        IndexCount;
            public uint        DataOffset;
        }

        //=========================================================================================
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private class ChunkVertexData : TypeUtils.Byteable<ChunkVertexData>
        {
            public ChunkHeader Header;
            public uint        DataSize;
            public uint        DataOffset;
            public uint        TotalVertices;
        }

        //=========================================================================================
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private class VertexAttribDecl : TypeUtils.Byteable<VertexAttribDecl>
        {
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
            public byte[] Name;
            public uint   Size;
            public uint   Type;
            public uint   Stride;
            public uint   Flags;
            public uint   DataOffset;
        }

        //=========================================================================================
        private class ChunkVertexAttrib
        {
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            private class MarshalChunkVertexAttrib
            {
                public ChunkHeader     Header;
                public uint            AttribCount;
            }
            
            private MarshalChunkVertexAttrib _data;

            public ChunkHeader Header       { get { return _data.Header; }       set { _data.Header = value; } }
            public uint        AttribCount  { get { return _data.AttribCount; }  set { _data.AttribCount = value; } }          
            
            public VertexAttribDecl[] AttribData;

            //-------------------------------------------------------------------------------------
            public static int FromBytes( byte[] bytes, int offset, out ChunkVertexAttrib obj ) 
            { 
                obj = new ChunkVertexAttrib();
                int blockSize = TypeUtils.FromBytes<MarshalChunkVertexAttrib>( bytes, offset, out obj._data );

                obj.AttribData = new VertexAttribDecl[obj.AttribCount];
                for( int i = 0; i < obj.AttribCount; i++ )
                {
                    blockSize += TypeUtils.FromBytes<VertexAttribDecl>( bytes, offset + blockSize, out obj.AttribData[i] );
                }
                return blockSize;
            }

            //-------------------------------------------------------------------------------------
            public static int ToBytes( ChunkVertexAttrib obj, out byte[] bytes ) 
            { 
                bytes = new byte[obj.Header.Size];

                byte[] block;
                int size = TypeUtils.ToBytes<MarshalChunkVertexAttrib>( obj._data, out block );

                Array.Copy( block, bytes, size );

                int blockOffset = size;
                for( int i = 0; i < obj.AttribCount; i++ )
                {
                    size = TypeUtils.ToBytes<VertexAttribDecl>( obj.AttribData[i], out block );
                    Array.Copy( block, 0, bytes, blockOffset, size );
                    blockOffset += size;
                }
                return blockOffset;
            }
        }

        //=========================================================================================
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private class SubObjectDecl : TypeUtils.Byteable<SubObjectDecl>
        {
            public uint First;
            public uint Count;
        }

        //=========================================================================================
        private class ChunkSubObjectList
        {
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            private class MarshalChunkSubObjectList
            {
                public ChunkHeader     Header;
                public uint            Count;
            }
            private MarshalChunkSubObjectList _data;

            /// <summary>
            /// ////////////////////NOTE: These should return native OpenTK types if applicable
            /// </summary>
            public ChunkHeader Header { get { return _data.Header; } set { _data.Header = value; } }
            public uint        Count  { get { return _data.Count; }  set { _data.Count = value; } }

            public SubObjectDecl[] SubObject;

            //-------------------------------------------------------------------------------------
            public static int FromBytes( byte[] bytes, int offset, out ChunkSubObjectList obj ) 
            { 
                obj = new ChunkSubObjectList();
                int blockSize = TypeUtils.FromBytes<MarshalChunkSubObjectList>( bytes, offset, out obj._data );

                obj.SubObject = new SubObjectDecl[obj.Count];
                for( int i = 0; i < obj.Count; i++ )
                {
                    blockSize += TypeUtils.FromBytes<SubObjectDecl>( bytes, offset + blockSize, out obj.SubObject[i] );
                }
                return blockSize;
            }

            //-------------------------------------------------------------------------------------
            public static int ToBytes( ChunkSubObjectList obj, out byte[] bytes ) 
            { 
                bytes = new byte[obj.Header.Size];

                byte[] block;
                int size = TypeUtils.ToBytes<MarshalChunkSubObjectList>( obj._data, out block );

                Array.Copy( block, bytes, size );

                int blockOffset = size;
                for( int i = 0; i < obj.Count; i++ )
                {
                    size = TypeUtils.ToBytes<SubObjectDecl>( obj.SubObject[i], out block );
                    Array.Copy( block, 0, bytes, blockOffset, size );
                    blockOffset += size;
                }
                return blockOffset;
            }
        }

        //=========================================================================================
        private class ChunkComment
        {
            public ChunkHeader Header;
            public byte[]      Comment;

            //-------------------------------------------------------------------------------------
            public static int FromBytes( byte[] bytes, int offset, out ChunkComment obj )
            { 
                obj = new ChunkComment();
                int blockSize = TypeUtils.FromBytes<ChunkHeader>( bytes, offset, out obj.Header );

                int dataSize = (int)(obj.Header.Size - blockSize);
                obj.Comment = new byte[dataSize];
                Array.Copy( bytes, offset, obj.Comment, 0, dataSize );

                return blockSize + dataSize;
            }

            //-------------------------------------------------------------------------------------
            public static int ToBytes( ChunkComment obj, out byte[] bytes )
            { 
                bytes = new byte[obj.Header.Size];

                byte[] block;
                int blockSize = TypeUtils.ToBytes<ChunkHeader>( obj.Header, out block );
                Array.Copy( block, bytes, blockSize );

                Array.Copy( obj.Comment, 0, bytes, blockSize, (int)(obj.Header.Size - blockSize) );

                return (int)(obj.Header.Size);
            }
        }

        //-----------------------------------------------------------------------------------------
        private static uint _maxSubObjects = 256;

        private uint _vertexBuffer = 0;
        private uint _indexBuffer  = 0;
        private uint _numIndices   = 0;
        private uint _indexType    = 0;

        private SubObjectDecl[] _subObject;

        private uint _numSubObjects = 0;
        public uint NumSubObject { get { return _numSubObjects; } }

        private uint _vao = 0;
        public uint Vao { get { return _vao; } }

        //-----------------------------------------------------------------------------------------
        public void Render( uint instanceCount =1, uint baseInstance =0 )
        {
            RenderSubObject( 0, instanceCount, baseInstance );
        }

        //-----------------------------------------------------------------------------------------
        public void RenderSubObject( uint objectIndex,
                                     uint instanceCount =1,
                                     uint baseInstance =0 )
        {
            GL.BindVertexArray( _vao );
            
            if( _indexBuffer != 0 )
            {
                GL.DrawElementsInstancedBaseInstance( PrimitiveType.Triangles,
                                                      (int)_numIndices,
                                                      (DrawElementsType)_indexType,
                                                      IntPtr.Zero,
                                                      (int)instanceCount,
                                                      (int)baseInstance );
            }
            else
            {
                GL.DrawArraysInstancedBaseInstance( PrimitiveType.Triangles,
                                                    (int)_subObject[objectIndex].First,
                                                    (int)_subObject[objectIndex].Count,
                                                    (int)instanceCount,
                                                    baseInstance );
            }
        }

        //-----------------------------------------------------------------------------------------
        public void GetSubObjectInfo( uint index, out uint first, out uint count )
        {
            if( index >= _numSubObjects ) { first = 0; count = 0; }
            else 
            {  
                first = _subObject[index].First;
                count = _subObject[index].Count;
            }
        }

        //-----------------------------------------------------------------------------------------
        public void Delete() 
        { 
            GL.DeleteVertexArrays( 1, ref _vao );
            GL.DeleteBuffers( 1, ref _vertexBuffer );
            GL.DeleteBuffers( 1, ref _indexBuffer );

            _vao = _vertexBuffer = _indexBuffer = _numIndices = 0;
        }

        //-----------------------------------------------------------------------------------------
        public void Load( string filename )
        {
            BinaryReader file = null;

            try
            {
                file = new BinaryReader( File.Open( filename, FileMode.Open ) );

                byte[] data = file.ReadBytes( (int)file.BaseStream.Length );
                
                SBM6Header hdr;

                int dataOffset = SBM6Header.FromBytes( data, 0 , out hdr );

                ChunkVertexAttrib vertexAttribChunk = null;
                ChunkVertexData vertexDataChunk     = null;
                ChunkIndexData indexDataChunk       = null;
                ChunkSubObjectList subObjectChunk   = null;

                for( int i = 0; i < hdr.NumChunks; i++ )
                {
                    ChunkHeader chunkHeader = new ChunkHeader();
                    TypeUtils.FromBytes<ChunkHeader>( data, dataOffset, out chunkHeader );

                    switch( chunkHeader.Type )
                    {
                        case ChunkType_e.VertexAttribs:
                            dataOffset += ChunkVertexAttrib.FromBytes( data, dataOffset, out vertexAttribChunk );
                            break;
                        case ChunkType_e.VertexData:
                            dataOffset += ChunkVertexData.FromBytes( data, dataOffset, out vertexDataChunk );
                            break;
                        case ChunkType_e.IndexData:
                            dataOffset += ChunkIndexData.FromBytes( data, dataOffset, out indexDataChunk );
                            break;
                        case ChunkType_e.SubObjectList:
                            dataOffset += ChunkSubObjectList.FromBytes( data, dataOffset, out subObjectChunk );
                            break;
                        default:
                            throw new SBM6ModelException( "Unknown SBM6 chunk encountered." );
                    }
                }

                if( subObjectChunk != null )
                {
                    if( subObjectChunk.Count > _maxSubObjects ) { subObjectChunk.Count = _maxSubObjects; }
                    _subObject = new SubObjectDecl[subObjectChunk.Count];

                    for( int i = 0; i < subObjectChunk.Count; i++ )
                    {
                        
                        _subObject[i] = subObjectChunk.SubObject[i];
                    }
                    _numSubObjects = subObjectChunk.Count;
                }
                else
                {
                    _subObject = new SubObjectDecl[1];
                    _subObject[0] = new SubObjectDecl();
                    _subObject[0].First = 0;
                    _subObject[0].Count = vertexDataChunk.TotalVertices;
                    _numSubObjects = 1;
                }

                GL.GenBuffers( 1, out _vertexBuffer );
                GL.BindBuffer( BufferTarget.ArrayBuffer, _vertexBuffer );
                GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)vertexDataChunk.DataSize, 
                               ref data[vertexDataChunk.DataOffset], BufferUsageHint.StaticDraw );

                GL.GenVertexArrays( 1, out _vao );
                GL.BindVertexArray( _vao );

                for( int i = 0; i < vertexAttribChunk.AttribCount; i++ )
                {
                    VertexAttribDecl attribDecl = vertexAttribChunk.AttribData[i];
                    
                    bool normalized = true;
                    if( (attribDecl.Flags & (uint)VertexAttribFlag_e.Normalized) == 0 ) 
                    { 
                        normalized = false; 
                    }

                    GL.VertexAttribPointer( i, (int)attribDecl.Size, (VertexAttribPointerType)attribDecl.Type, 
                                            normalized, (int)attribDecl.Stride, (IntPtr)attribDecl.DataOffset );
                    GL.EnableVertexAttribArray( i );
                }

                if( indexDataChunk != null )
                {
                    GL.GenBuffers( 1, out _indexBuffer );
                    GL.BindBuffer( BufferTarget.ElementArrayBuffer, _indexBuffer );

                    int indexSize = (indexDataChunk.IndexType == (uint)VertexAttribType.UnsignedShort) ? sizeof(ushort) : sizeof(byte);
                    GL.BufferData( BufferTarget.ElementArrayBuffer,
                                   (IntPtr)(indexDataChunk.IndexCount * indexSize),
                                   (IntPtr)data[indexDataChunk.DataOffset], BufferUsageHint.StaticDraw );
                    _numIndices = indexDataChunk.IndexCount;
                    _indexType = indexDataChunk.IndexType;
                }
                else
                {
                    _numIndices = vertexDataChunk.TotalVertices;
                }

                GL.BindVertexArray(0);
                GL.BindBuffer( BufferTarget.ElementArrayBuffer, 0 );
            }
            catch( IOException e )
            {
                Console.WriteLine( "Framwork.SBM6 IO Exception: " + e.Message );
            }
            catch( SBM6ModelException e )
            {
                Console.WriteLine( "Framwork.SBM6 Exception: " + e.Message );
            }

            if( file != null ) { file.Close(); }

        }
    }
}
