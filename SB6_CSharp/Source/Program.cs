using System;
using System.Drawing;
using OpenTK;

namespace SB6_CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            //using(var example = new Example_02L01() )
            //using(var example = new Example_02L02() )
            //using(var example = new Example_02L03_02L07() )
            using(var example = new Example_02L08_02L09() )
            {
                //Run the example
                example.Run();
            }
        }
    }
}
