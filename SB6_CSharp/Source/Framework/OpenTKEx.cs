//=================================================================================================
//
//
//=================================================================================================
using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics;

namespace SB6_CSharp.Framework
{
    //=============================================================================================
    /// <summary>
    /// 
    /// </summary>
    public static class OpenTKEx
    {
        /// <summary>
        /// OpenTK.Graphics.Color4 extention method to convert Color4 instance to a float array.
        /// </summary>
        public static float[] ToArrayExt( this Color4 color )
        {
            return new float[] { color.R, color.G, color.B, color.A };
        }

        /// <summary>
        /// Represents a 4D vector using four integer values.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Vector4i
        {
            public int X;
            public int Y;
            public int Z;
            public int W;

            public Vector4i( int value )
            {
                X = value;
                Y = value;
                Z = value;
                W = value;
            }
            
            public int this[int index]
            {
                get
                {
                    if( index == 0 )      { return X; }
                    else if( index == 1 ) { return Y; }
                    else if( index == 2 ) { return Z; }
                    else if( index == 3 ) { return W; }
                    throw new IndexOutOfRangeException( "You tried to access this vector at index: " + index );
                }
                set
                {
                    if( index == 0 )      { X = value; }
                    else if( index == 1 ) { Y = value; }
                    else if( index == 2 ) { Z = value; }
                    else if( index == 3 ) { W = value; }
                    else throw new IndexOutOfRangeException( "You tried to access this vector at index: " + index );
                }
            }
        };

    }
}
