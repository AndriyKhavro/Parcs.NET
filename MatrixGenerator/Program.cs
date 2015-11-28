using System;
using NewMatrixModule;

namespace MatrixGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter matrix height:");
            int h = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter matrix width:");
            int w = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter the name of the file");
            string file = Console.ReadLine();
            new Matrix(h, w, true).WriteToFile(file);
        }
    }
}
