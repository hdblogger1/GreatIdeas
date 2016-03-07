using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SB6_CSharp.Framework
{
    //=============================================================================================
    public class Object
    {
        //=========================================================================================
        private struct ObjectHeader_s
        {
            public uint  Magic;
            public uint  Size;
            public uint  NumChunks;
            public uint  Flags;
        }

        //=========================================================================================
        private enum ChunkType_e:uint
        {
            IndexData     = 0x58444E49, // ascii XDNI ==> "INDX"
            VertexData    = 0x58545258, // ascii XTRV ==> "VRTX"
            VertexAttribs = 0x42525441, // ascii BRTA ==> "ATRB"
            SubObjectList = 0x54534c4F, // ascii TSLO ==> "OLST"
            Comment       = 0x544E4D43, // ascii TNMC ==> "CMNT"
        }

        //=========================================================================================
        private struct ChunkHeader_s
        {
            public uint Type;
            public uint Size;
        }

        //=========================================================================================
        private struct ChunkIndexData_s
        {
            public ChunkHeader_s Header;
            public uint          Type;
            public uint          Count;
            public uint          DataOffset;
        }

        //=========================================================================================
        private struct ChunkVertexData_s
        {
            public ChunkHeader_s Header;
            uint                 Size;
            uint                 Offset;
            uint                 TotalVertices;
        }

        //=========================================================================================
        private struct ChunkComment_s
        {
            public ChunkHeader_s Header;
            byte[]               comment;
        }

        //=========================================================================================
        private struct SubObjectDecl_s
        {
            public uint First;
            public uint Count;
        }

        //=========================================================================================
        private struct ChunkSubObjectList_s
        {
            public ChunkHeader_s Header;
            public uint          Count;
            SubObjectDecl_s[]    SubObject;
        }

        //=========================================================================================
        private enum VertexAttribFlag_e:uint
        {
            Normalized  = 1,
            Integer     = 2,
        }

        //=========================================================================================
        private struct VertexAttribDecl_s
        {
            public byte[] Name; // 64 character name block
            public uint   Size;
            public uint   Type;
            public uint   Stride;
            public uint   Flags;
            public uint   DataOffset;
        }

        //=========================================================================================
        private struct VertexAttribChunk_s
        {
            public ChunkHeader_s Header;
            public uint          AttribCount;
            VertexAttribDecl_s[] AttribData;
        }

        //-----------------------------------------------------------------------------------------
        private static int _maxSubObjects = 256;

        private uint _vertexBuffer = 0;
        private uint _indexBuffer  = 0;
        private uint _numIndices   = 0;
        private uint _indexType    = 0;

        private SubObjectDecl_s[] _subObject = new SubObjectDecl_s[_maxSubObjects];

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

        }
    }
}
