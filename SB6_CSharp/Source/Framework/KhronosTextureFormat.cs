using System;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SB6_CSharp.Framework
{
    using IOException = System.IO.IOException;
    using IOFile = System.IO.File;
    using IOFileMode = System.IO.FileMode;
    using IOReader = System.IO.BinaryReader;

    static public class KTX
    {
        private class ParseKtxFileException : Exception
        {
            public ParseKtxFileException() { }
            public ParseKtxFileException( string msg ) : base( msg ) { }
            public ParseKtxFileException( string msg, Exception inner ) : base( msg, inner ) { }
        }

        private struct KtxHeader_s
        {
            public byte[] identifier;
            public uint   endianness;
            public uint   gltype;
            public uint   gltypesize;
            public uint   glformat;
            public uint   glinternalformat;
            public uint   glbaseinternalformat;
            public int    pixelwidth;
            public int    pixelheight;
            public int    pixeldepth;
            public uint   arrayelements;
            public uint   faces;
            public uint   miplevels;
            public uint   keypairbytes;        
        }

        private struct KeyValuePair_s
        {
            public uint   size;
            public byte[] rawbytes;
        }

        static private byte[] _identifier
            = { 0xAB, 0x4B, 0x54, 0x58, 0x20, 0x31, 0x31, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A };

        //-----------------------------------------------------------------------------------------
        static private uint _Swap32( uint u32 )
        {
            return ((u32 & 0xFF000000u) >> 24) 
                    | ((u32 & 0x00FF0000u) >> 8)
                    | ((u32 & 0x0000FF00u) << 8)
                    | ((u32 & 0x000000FFu) << 24);
        }
            
        //-----------------------------------------------------------------------------------------
        static private ushort _Swap16( ushort u16 )
        {               
            return (ushort)(((u16 & 0xFF00) >> 8) | ((u16 & 0x00FF) << 8));
        }

        //-----------------------------------------------------------------------------------------
        static private uint _CalculateStride( ref KtxHeader_s hdr, int width, uint pad =4 )
        {
            uint channels = 0;
            switch( hdr.glbaseinternalformat )
            {
                case (int)OpenTK.Graphics.OpenGL.All.Red:  channels = 1;  break;
                case (int)OpenTK.Graphics.OpenGL.All.Rg:   channels = 2;  break;
                case (int)OpenTK.Graphics.OpenGL.All.Bgr:
                case (int)OpenTK.Graphics.OpenGL.All.Rgb:  channels = 3;  break;
                case (int)OpenTK.Graphics.OpenGL.All.Bgra: 
                case (int)OpenTK.Graphics.OpenGL.All.Rgba: channels = 4;  break;
            }
            uint stride = hdr.gltypesize * channels * (uint)width;
            stride = (stride + (pad - 1)) & ~(pad - 1);

            return stride;
        }

        //-----------------------------------------------------------------------------------------
        static private uint _CalculateFaceSize( ref KtxHeader_s hdr )
        {
            return _CalculateStride( ref hdr, hdr.pixelwidth ) * (uint)hdr.pixelheight;
        }

        //-----------------------------------------------------------------------------------------
        static public uint Load( string filename, uint tex )
        {
            IOReader file = null;
            KtxHeader_s hdr = new KtxHeader_s();

            try
            {
                file = new IOReader( IOFile.Open( filename, IOFileMode.Open ) );

                hdr.identifier           = file.ReadBytes(12);
                hdr.endianness           = file.ReadUInt32();
                hdr.gltype               = file.ReadUInt32();
                hdr.gltypesize           = file.ReadUInt32();
                hdr.glformat             = file.ReadUInt32();
                hdr.glinternalformat     = file.ReadUInt32();
                hdr.glbaseinternalformat = file.ReadUInt32();
                hdr.pixelwidth           = (int)file.ReadUInt32();
                hdr.pixelheight          = (int)file.ReadUInt32();
                hdr.pixeldepth           = (int)file.ReadUInt32();
                hdr.arrayelements        = file.ReadUInt32();
                hdr.faces                = file.ReadUInt32();
                hdr.miplevels            = file.ReadUInt32();
                hdr.keypairbytes         = file.ReadUInt32();
 
                if( !hdr.identifier.SequenceEqual( _identifier ) ) 
                { 
                    throw new ParseKtxFileException( "Invalid KTX file header." );
                }

                if( hdr.endianness == 0x04030201 ) { /* No swap needed */ }
                else if( hdr.endianness == 0x01020304 )
                {
                    hdr.endianness           = _Swap32( hdr.endianness );
                    hdr.gltype               = _Swap32( hdr.gltype );
                    hdr.gltypesize           = _Swap32( hdr.gltypesize );
                    hdr.glformat             = _Swap32( hdr.glformat );
                    hdr.glinternalformat     = _Swap32( hdr.glinternalformat );
                    hdr.glbaseinternalformat = _Swap32( hdr.glbaseinternalformat );
                    hdr.pixelwidth           = (int)_Swap32( (uint)hdr.pixelwidth );
                    hdr.pixelheight          = (int)_Swap32( (uint)hdr.pixelheight );
                    hdr.pixeldepth           = (int)_Swap32( (uint)hdr.pixeldepth );
                    hdr.arrayelements        = _Swap32( hdr.arrayelements );
                    hdr.faces                = _Swap32( hdr.faces );
                    hdr.miplevels            = _Swap32( hdr.miplevels );
                    hdr.keypairbytes         = _Swap32( hdr.keypairbytes ); 
                }
                else
                {
                    throw new ParseKtxFileException( "Invalid KTX header value: endianness." );
                }

                // Guess target (texture type)
                TextureTarget target = 0;

                if( hdr.pixelheight == 0 )
                {
                    if( hdr.arrayelements == 0 ) { target = TextureTarget.Texture1D; }
                    else                         { target = TextureTarget.Texture1DArray; }
                }
                else if( hdr.pixeldepth == 0 )
                {
                    if( hdr.arrayelements == 0 )
                    {
                        if( hdr.faces == 0 ) { target = TextureTarget.Texture2D; }
                        else                 { target = TextureTarget.TextureCubeMap; }
                    }
                    else
                    {
                        if( hdr.faces == 0 ) { target = TextureTarget.Texture2DArray; }
                        else                 { target = TextureTarget.TextureCubeMapArray; }
                    }
                }
                else
                {
                    target = TextureTarget.Texture3D;
                }

                // Check for insanity...
                if( target == 0 ||                                   // Couldn't figure out target
                    (hdr.pixelwidth == 0) ||                         // Texture has no width???
                    (hdr.pixelheight == 0 && hdr.pixeldepth != 0) )  // Texture has depth but no height???
                {
                    throw new ParseKtxFileException( "Invalid KTX file." );
                }

                if( tex == 0 ) { GL.GenTextures( 1, out tex ); }
                GL.BindTexture( target, tex );

                long dataBytes = file.BaseStream.Length - (file.BaseStream.Position + hdr.keypairbytes);

                //byte[] data = new byte[dataBytes];
                byte[] data = file.ReadBytes( (int)dataBytes );

                if( hdr.miplevels == 0 ) { hdr.miplevels = 1; }

                switch( target )
                {
                    case TextureTarget.Texture1D:
                        GL.TexStorage1D( TextureTarget1d.Texture1D, (int)hdr.miplevels,  
                                            (SizedInternalFormat)hdr.glinternalformat, hdr.pixelwidth );
                        GL.TexSubImage1D( TextureTarget.Texture1D, 0, 0, hdr.pixelwidth, 
                                            (PixelFormat)hdr.glformat, (PixelType)hdr.glinternalformat, data );
                        break;

                    case TextureTarget.Texture2D:
                        GL.TexStorage2D( TextureTarget2d.Texture2D, (int)hdr.miplevels, 
                                            (SizedInternalFormat)hdr.glinternalformat, 
                                            hdr.pixelwidth, hdr.pixelheight );
                        {
                            int offset = 0;
                            int height = hdr.pixelheight;
                            int width = hdr.pixelwidth;

                            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1 );

                            for( int i = 0; i < hdr.miplevels; i++ )
                            {
                                GL.TexSubImage2D(TextureTarget.Texture2D, i, 0, 0, width, height,
                                                    (PixelFormat)hdr.glformat, (PixelType)hdr.gltype,
                                                    ref data[offset] );
                                offset += height * (int)_CalculateStride( ref hdr, width, 1 );
                                height >>= 1;
                                if( height == 0 ) { height = 1; }
                                width >>= 1;
                                if( width == 0 )  { width = 1; }
                            }
                        }
                        break;

                    case TextureTarget.Texture3D:
                        GL.TexStorage3D( TextureTarget3d.Texture3D, (int)hdr.miplevels, 
                                            (SizedInternalFormat) hdr.glinternalformat, hdr.pixelwidth,
                                            hdr.pixelheight, hdr.pixeldepth );
                        GL.TexSubImage3D( TextureTarget.Texture3D, 0, 0, 0, 0, hdr.pixelwidth, hdr.pixelheight, hdr.pixeldepth,
                                            (PixelFormat)hdr.glformat, (PixelType)hdr.gltype, data );
                        break;

                    case TextureTarget.Texture1DArray:
                        GL.TexStorage2D( TextureTarget2d.Texture1DArray, (int)hdr.miplevels, 
                                            (SizedInternalFormat) hdr.glinternalformat, hdr.pixelwidth,
                                            (int)hdr.arrayelements );
                        GL.TexSubImage2D( TextureTarget.Texture1DArray, 0, 0, 0, hdr.pixelwidth, (int)hdr.arrayelements,
                                            (PixelFormat)hdr.glformat, (PixelType)hdr.gltype, data );
                        break;

                    case TextureTarget.Texture2DArray:
                        GL.TexStorage3D( TextureTarget3d.Texture2DArray, (int)hdr.miplevels, 
                                            (SizedInternalFormat) hdr.glinternalformat, hdr.pixelwidth, hdr.pixelheight,
                                            (int)hdr.arrayelements );
                        GL.TexSubImage3D( TextureTarget.Texture2DArray, 0, 0, 0, 0, hdr.pixelwidth, hdr.pixelheight,
                                            (int)hdr.arrayelements,
                                            (PixelFormat)hdr.glformat, (PixelType)hdr.gltype, data );
                        break;

                    case TextureTarget.TextureCubeMap:
                        GL.TexStorage2D( TextureTarget2d.TextureCubeMap, (int)hdr.miplevels, 
                                            (SizedInternalFormat) hdr.glinternalformat, hdr.pixelwidth,
                                            (int)hdr.pixelheight );
                        {
                            uint faceSize = _CalculateFaceSize( ref hdr );
                            for( uint i = 0; i < hdr.faces; i++ )
                            {
                                GL.TexSubImage2D( (TextureTarget)((int)TextureTarget.TextureCubeMapPositiveX + i), 
                                                    0, 0, 0, hdr.pixelwidth, 
                                                    (int)hdr.pixelheight,
                                                    (PixelFormat)hdr.glformat, (PixelType)hdr.gltype, 
                                                    ref data[faceSize * i] );
                            }
                        }
                        break;

                    case TextureTarget.TextureCubeMapArray:
                        GL.TexStorage3D( (TextureTarget3d)TextureTarget.TextureCubeMapArray, 
                                            (int)hdr.miplevels, 
                                            (SizedInternalFormat) hdr.glinternalformat, hdr.pixelwidth, hdr.pixelheight,
                                            (int)hdr.arrayelements );
                        GL.TexSubImage3D( TextureTarget.TextureCubeMapArray, 
                                            0, 0, 0, 0, hdr.pixelwidth, hdr.pixelheight,
                                            (int)(hdr.faces * hdr.arrayelements),
                                            (PixelFormat)hdr.glformat, (PixelType)hdr.gltype, data );
                        break;

                    default:  // Should never happen
                        throw new ParseKtxFileException( "Unsupported KTX file texture target." );
                }

                if( hdr.miplevels == 1 ) 
                { 
                    GL.GenerateMipmap( (GenerateMipmapTarget)target ); 
                }
                
            }
            catch( IOException e )
            {
                Console.WriteLine( "KTX.File IO Exception: " + e.Message );
            }
            catch( ParseKtxFileException e )
            {
                Console.WriteLine( "KTX.File.Load Exception: " + e.Message );
            }

            if( file != null ) { file.Close(); }

            return tex;
        }

        //-----------------------------------------------------------------------------------------
        static public bool Save( string filename, TextureTarget target, uint tex )
        {
            KtxHeader_s hdr = new KtxHeader_s();
            Array.Copy( _identifier, hdr.identifier, _identifier.Length );
            hdr.endianness = 0x04030201;

            GL.BindTexture( target, tex );

            GL.GetTexLevelParameter( target, 0, GetTextureParameter.TextureWidth, out hdr.pixelwidth );
            GL.GetTexLevelParameter( target, 0, GetTextureParameter.TextureHeight, out hdr.pixelheight );
            GL.GetTexLevelParameter( target, 0, GetTextureParameter.TextureDepth, out hdr.pixeldepth );

            return true;
        }
    }
}
