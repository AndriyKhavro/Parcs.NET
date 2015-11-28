using Parcs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NewMatrixModule
{
    public class MatrixesModule : MainModule
    {
        const string fileName = "resMatrix.matr";
        public static void Main(string[] args)
        {
            (new MatrixesModule()).RunModule();
            Console.ReadKey();
        }

        public override void Run(ModuleInfo info)
        {

            Console.WriteLine("Enter the fileName of the first matrix:");
            string file1 = Console.ReadLine();
            Console.WriteLine("Enter the fileName of the second matrix:");
            string file2 = Console.ReadLine();
            MyMatrix A, B;

            try
            {
                A = MyMatrix.LoadFromFile(file1);
                B = MyMatrix.LoadFromFile(file2);
            }

            catch (FileNotFoundException)
            {
                Console.WriteLine("File with a given fileName not found, stopping the application...");
                return;
            }

            Console.WriteLine("Enter the number of points (possible options: 1 2 3 4 5 8): ");

            int pointsNum = int.Parse(Console.ReadLine());
            if (pointsNum > 5) pointsNum = 8;
            var points = new IPoint[pointsNum];
            var channels = new IChannel[pointsNum];
            for (int i = 0; i < pointsNum; ++i)
            {
                points[i] = info.CreatePoint();
                channels[i] = points[i].CreateChannel();
                points[i].ExecuteClass("NewMatrixModule.MultMatrix");
            }

            var resMatrix = new MyMatrix(A.Height, B.Width);
            DateTime time = DateTime.Now;
            Console.WriteLine("Waiting for a result...");

            if (pointsNum == 1)
            {
                channels[0].WriteObject(A);
                channels[0].WriteObject(B);
                resMatrix = (MyMatrix)channels[0].ReadObject(typeof(MyMatrix));
            }

            if (pointsNum == 2 || pointsNum == 3 || pointsNum == 5)
            {
                for (int i = 0; i < pointsNum; ++i)
                {
                    channels[i].WriteObject(A.SubMatrix(0, 0, A.Height / pointsNum + ((i == pointsNum - 1) ? A.Height % pointsNum : 0), A.Width));
                    channels[i].WriteObject(B);
                }

                Console.WriteLine("Sending finished: time = {0}", Math.Round((DateTime.Now - time).TotalSeconds, 3));
            
                for (int i = 0; i < pointsNum; ++i)
                {
                    var subMatrix = (MyMatrix)channels[i].ReadObject(typeof(MyMatrix));
                    resMatrix.FillSubMatrix(subMatrix, i * (A.Height / pointsNum), 0);
                }
            }

            if (pointsNum == 4)
            {
                channels[0].WriteObject(A.SubMatrix(0, 0, A.Height / 2, A.Width));
                channels[0].WriteObject(B.SubMatrix(0, 0, B.Height, B.Width / 2));
                channels[1].WriteObject(A.SubMatrix(0, 0, A.Height / 2, A.Width));
                channels[1].WriteObject(B.SubMatrix(0, B.Width / 2, B.Height, B.Width / 2 + B.Width % 2));
                channels[2].WriteObject(A.SubMatrix(A.Height / 2, 0, A.Height / 2 + A.Height % 2, B.Width));
                channels[2].WriteObject(B.SubMatrix(0, 0, B.Height, B.Width / 2));
                channels[3].WriteObject(A.SubMatrix(A.Height / 2, 0, A.Height / 2 + A.Height % 2, B.Width));
                channels[3].WriteObject(B.SubMatrix(0, B.Width / 2, B.Height, B.Width / 2 + B.Width % 2));

                Console.WriteLine("Sending finished: time = {0}", Math.Round((DateTime.Now - time).TotalSeconds, 3));
            
                resMatrix.FillSubMatrix((MyMatrix)channels[0].ReadObject(), 0, 0);
                resMatrix.FillSubMatrix((MyMatrix)channels[1].ReadObject(), 0, B.Width / 2);
                resMatrix.FillSubMatrix((MyMatrix)channels[2].ReadObject(), resMatrix.Height / 2, 0);
                resMatrix.FillSubMatrix((MyMatrix)channels[3].ReadObject(), resMatrix.Height / 2, resMatrix.Width / 2);

            }

            if (pointsNum == 8)
            {
                channels[0].WriteObject(A.SubMatrix(0, 0, A.Height / 2, A.Width / 2));
                channels[0].WriteObject(B.SubMatrix(0, 0, B.Height / 2, B.Width / 2));

                channels[1].WriteObject(A.SubMatrix(0, A.Width / 2, A.Height / 2, A.Width / 2 + A.Width % 2));
                channels[1].WriteObject(B.SubMatrix(B.Height / 2, 0, B.Height / 2 + B.Height % 2, B.Width / 2));

                channels[2].WriteObject(A.SubMatrix(0, 0, A.Height / 2, A.Width / 2));
                channels[2].WriteObject(B.SubMatrix(0, B.Width / 2, B.Height / 2, B.Width / 2 + B.Width % 2));

                channels[3].WriteObject(A.SubMatrix(0, A.Width / 2, A.Height / 2, A.Width / 2 + A.Width % 2));
                channels[3].WriteObject(B.SubMatrix(B.Height / 2, B.Width / 2, B.Height / 2 + B.Height % 2, B.Width / 2 + B.Width % 2));

                channels[4].WriteObject(A.SubMatrix(A.Height / 2, 0, A.Height / 2 + A.Height % 2, A.Width / 2));
                channels[4].WriteObject(B.SubMatrix(0, 0, B.Height / 2, B.Width / 2));

                channels[5].WriteObject(A.SubMatrix(A.Height / 2, A.Width / 2, A.Height / 2 + A.Height % 2, A.Width / 2 + A.Width % 2));
                channels[5].WriteObject(B.SubMatrix(B.Height / 2, 0, B.Height / 2 + B.Height % 2, B.Width / 2));

                channels[6].WriteObject(A.SubMatrix(A.Height / 2, 0, A.Height / 2 + A.Height % 2, A.Width / 2));
                channels[6].WriteObject(B.SubMatrix(0, B.Width / 2, B.Height / 2, B.Width / 2 + B.Width % 2));

                channels[7].WriteObject(A.SubMatrix(A.Height / 2, A.Width / 2, A.Height / 2 + A.Height % 2, A.Width / 2 + A.Width % 2));
                channels[7].WriteObject(B.SubMatrix(B.Height / 2, B.Width / 2, B.Height / 2 + B.Height % 2, B.Width / 2 + B.Width % 2));
                Console.WriteLine("Sending finished: time = {0}", Math.Round((DateTime.Now - time).TotalSeconds, 3));
            
                MyMatrix[,] Parts = new MyMatrix[2, 2];

                Parts[0, 0] = (MyMatrix)channels[0].ReadObject();
                Parts[0, 0].Add((MyMatrix)channels[1].ReadObject());
                resMatrix.FillSubMatrix(Parts[0, 0], 0, 0);
                Parts[0, 1] = (MyMatrix)channels[2].ReadObject();
                Parts[0, 1].Add((MyMatrix)channels[3].ReadObject());
                resMatrix.FillSubMatrix(Parts[0, 1], 0, resMatrix.Width / 2);
                Parts[1, 0] = (MyMatrix)channels[4].ReadObject();
                Parts[1, 0].Add((MyMatrix)channels[5].ReadObject());
                resMatrix.FillSubMatrix(Parts[1, 0], resMatrix.Height / 2, 0);
                Parts[1, 1] = (MyMatrix)channels[6].ReadObject();
                Parts[1, 1].Add((MyMatrix)channels[7].ReadObject());

                resMatrix.FillSubMatrix(Parts[1, 1], resMatrix.Height / 2, resMatrix.Width / 2);

            }


            Console.WriteLine("Result found: time = {0}, saving the result to the file {1}", Math.Round((DateTime.Now - time).TotalSeconds, 3), fileName);
        }
    }
}
