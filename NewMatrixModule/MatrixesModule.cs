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
        private const string fileName = "resMatrix.mtr";

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
            Matrix a, b;

            try
            {
                a = Matrix.LoadFromFile(file1);
                b = Matrix.LoadFromFile(file2);
            }

            catch (FileNotFoundException)
            {
                Console.WriteLine("File with a given fileName not found, stopping the application...");
                return;
            }

            int[] possibleValues = { 1, 2, 4, 8, 16, 32 };
            
            int pointsNum;
            do
            {
                Console.WriteLine("Enter the number of points (possible options: {0}): ", string.Join(" ", possibleValues));
            }
            while (!int.TryParse(Console.ReadLine(), out pointsNum) || !possibleValues.Contains(pointsNum));

            var points = new IPoint[pointsNum];
            var channels = new IChannel[pointsNum];
            for (int i = 0; i < pointsNum; ++i)
            {
                points[i] = info.CreatePoint();
                channels[i] = points[i].CreateChannel();
                points[i].ExecuteClass("NewMatrixModule.MultMatrix");
            }

            var resMatrix = new Matrix(a.Height, b.Width);
            DateTime time = DateTime.Now;
            Console.WriteLine("Waiting for a result...");

            if (pointsNum == 1)
            {
                channels[0].WriteObject(a);
                channels[0].WriteObject(b);
                resMatrix = (Matrix)channels[0].ReadObject(typeof(Matrix));
            }

            else if (pointsNum == 2)
            {
                var matrixPairs = Divide2(a, b).ToArray();
                channels[0].WriteObject(matrixPairs[0].Item1);
                channels[0].WriteObject(matrixPairs[0].Item2);
                channels[1].WriteObject(matrixPairs[1].Item1);
                channels[1].WriteObject(matrixPairs[1].Item2);

                LogSendingTime(time);

                Join2(resMatrix, channels.Select(c => new Lazy<Matrix>(c.ReadObject<Matrix>)).ToArray());
            }

            else if (pointsNum == 4)
            {
                var matrixPairs = Divide4(a, b).ToArray();
                for (int i = 0; i < matrixPairs.Length; i++)
                {
                    channels[i].WriteObject(matrixPairs[i].Item1);
                    channels[i].WriteObject(matrixPairs[i].Item2);
                }

                LogSendingTime(time);

                Join4(resMatrix, channels.Select(c => new Lazy<Matrix>(c.ReadObject<Matrix>)).ToArray());
            }

            else if (pointsNum == 8)
            {
                var matrixPairs = Divide8(a, b).ToArray();
                for (int i = 0; i < matrixPairs.Length; i++)
                {
                    channels[i].WriteObject(matrixPairs[i].Item1);
                    channels[i].WriteObject(matrixPairs[i].Item2);
                }

                LogSendingTime(time);

                Join8(resMatrix, channels.Select(c => new Lazy<Matrix>(c.ReadObject<Matrix>)).ToArray());
            }

            else if (pointsNum == 16) //8 * 2
            {
                var matrixPairs8 = Divide8(a, b).ToArray();
                for (int i = 0; i < 8; i++)
                {
                    var m2 = Divide2(matrixPairs8[i].Item1, matrixPairs8[i].Item2).ToArray();
                    channels[i*2].WriteObject(m2[0].Item1);
                    channels[i*2].WriteObject(m2[0].Item2);
                    channels[i*2 + 1].WriteObject(m2[1].Item1);
                    channels[i*2 + 1].WriteObject(m2[1].Item2);
                }

                LogSendingTime(time);

                var resMatrix8 =
                    Enumerable.Range(0, 8)
                        .Select(i => new Matrix(matrixPairs8[i].Item1.Height, matrixPairs8[i].Item2.Width))
                        .ToArray();
                for (int i = 0; i < 8; i++)
                {
                    Join2(resMatrix8[i],
                        new[] {channels[2*i], channels[2*i + 1]}.Select(c => new Lazy<Matrix>(c.ReadObject<Matrix>))
                            .ToArray());
                }

                Join8(resMatrix, resMatrix8.Select(m => new Lazy<Matrix>(() => m)).ToArray()); //not nice, probably create overload
            }

            else if (pointsNum == 32) //8 * 4
            {
                var matrixPairs8 = Divide8(a, b).ToArray();
                for (int i = 0; i < 8; i++)
                {
                    var m4 = Divide4(matrixPairs8[i].Item1, matrixPairs8[i].Item2).ToArray();

                    for (int j = 0; j < 4; j++)
                    {
                        channels[i * 4 + j].WriteObject(m4[j].Item1);
                        channels[i * 4 + j].WriteObject(m4[j].Item2);
                    }
                }

                LogSendingTime(time);

                var resMatrix8 =
                    Enumerable.Range(0, 8)
                        .Select(i => new Matrix(matrixPairs8[i].Item1.Height, matrixPairs8[i].Item2.Width))
                        .ToArray();
                for (int i = 0; i < 8; i++)
                {
                    Join4(resMatrix8[i], Enumerable.Range(0, 4).Select(j => channels[4*i+j])
                        .Select(c => new Lazy<Matrix>(c.ReadObject<Matrix>))
                            .ToArray());
                }

                Join8(resMatrix, resMatrix8.Select(m => new Lazy<Matrix>(() => m)).ToArray()); //not nice, probably create overload
            }

            LogResultFoundTime(time);
            SaveMatrix(resMatrix);
        }

        private static void LogResultFoundTime(DateTime time)
        {
            Console.WriteLine(
                "Result found: time = {0}, saving the result to the file {1}",
                Math.Round((DateTime.Now - time).TotalSeconds, 3),
                fileName);
        }

        private static void LogSendingTime(DateTime time)
        {
            Console.WriteLine("Sending finished: time = {0}", Math.Round((DateTime.Now - time).TotalSeconds, 3));
        }

        private static IEnumerable<Tuple<Matrix, Matrix>> Divide2(Matrix a, Matrix b)
        {
            yield return Tuple.Create(a.SubMatrix(0, 0, b.Height / 2, b.Width), b);
            yield return Tuple.Create(a.SubMatrix(0, 0, a.Height / 2 + a.Height % 2, a.Width), b);
        }

        private static IEnumerable<Tuple<Matrix, Matrix>> Divide4(Matrix a, Matrix b)
        {
            yield return Tuple.Create(a.SubMatrix(0, 0, a.Height / 2, a.Width), b.SubMatrix(0, 0, b.Height, b.Width / 2));
            yield return Tuple.Create(a.SubMatrix(0, 0, a.Height / 2, a.Width), b.SubMatrix(0, b.Width / 2, b.Height, b.Width / 2 + b.Width % 2));
            yield return Tuple.Create(a.SubMatrix(a.Height / 2, 0, a.Height / 2 + a.Height % 2, b.Width), b.SubMatrix(0, 0, b.Height, b.Width / 2));
            yield return Tuple.Create(a.SubMatrix(a.Height / 2, 0, a.Height / 2 + a.Height % 2, b.Width), b.SubMatrix(0, b.Width / 2, b.Height, b.Width / 2 + b.Width % 2));
        }

        private static IEnumerable<Tuple<Matrix, Matrix>> Divide8(Matrix a, Matrix b)
        {
            yield return
                Tuple.Create(a.SubMatrix(0, 0, a.Height/2, a.Width/2), b.SubMatrix(0, 0, b.Height/2, b.Width/2));
            yield return
                Tuple.Create(a.SubMatrix(0, a.Width/2, a.Height/2, a.Width/2 + a.Width%2),
                    b.SubMatrix(b.Height/2, 0, b.Height/2 + b.Height%2, b.Width/2));
            yield return
                Tuple.Create(a.SubMatrix(0, 0, a.Height/2, a.Width/2),
                    b.SubMatrix(0, b.Width/2, b.Height/2, b.Width/2 + b.Width%2));
            yield return Tuple.Create(a.SubMatrix(0, a.Width/2, a.Height/2, a.Width/2 + a.Width%2),
                b.SubMatrix(b.Height/2, b.Width/2, b.Height/2 + b.Height%2, b.Width/2 + b.Width%2));
            yield return
                Tuple.Create(a.SubMatrix(a.Height/2, 0, a.Height/2 + a.Height%2, a.Width/2),
                    b.SubMatrix(0, 0, b.Height/2, b.Width/2));
            yield return Tuple.Create(a.SubMatrix(a.Height/2, a.Width/2, a.Height/2 + a.Height%2,
                a.Width/2 + a.Width%2), b.SubMatrix(b.Height/2, 0, b.Height/2 + b.Height%2, b.Width/2));
            yield return
                Tuple.Create(a.SubMatrix(a.Height/2, 0, a.Height/2 + a.Height%2, a.Width/2),
                    b.SubMatrix(0, b.Width/2, b.Height/2, b.Width/2 + b.Width%2));
            yield return Tuple.Create(a.SubMatrix(a.Height/2, a.Width/2, a.Height/2 + a.Height%2,
                a.Width/2 + a.Width%2), b.SubMatrix(b.Height/2, b.Width/2, b.Height/2 + b.Height%2,
                    b.Width/2 + b.Width%2));
        }

        private static Matrix Join2(Matrix resMatrix, IList<Lazy<Matrix>> matrixes)
        {
            resMatrix.FillSubMatrix(matrixes[0].Value, 0, 0);
            resMatrix.FillSubMatrix(matrixes[1].Value, (resMatrix.Height / 2), 0);
            return resMatrix;
        }

        private static Matrix Join4(Matrix resMatrix, IList<Lazy<Matrix>> matrixes)
        {
            resMatrix.FillSubMatrix(matrixes[0].Value, 0, 0);
            resMatrix.FillSubMatrix(matrixes[1].Value, 0, resMatrix.Width / 2);
            resMatrix.FillSubMatrix(matrixes[2].Value, resMatrix.Height / 2, 0);
            resMatrix.FillSubMatrix(matrixes[3].Value, resMatrix.Height / 2, resMatrix.Width / 2);
            return resMatrix;
        }

        private static Matrix Join8(Matrix resMatrix, IList<Lazy<Matrix>> matrixes)
        {
            var parts = new Matrix[2, 2];

            parts[0, 0] = matrixes[0].Value;
            parts[0, 0].Add(matrixes[1].Value);
            resMatrix.FillSubMatrix(parts[0, 0], 0, 0);
            parts[0, 1] = matrixes[2].Value;
            parts[0, 1].Add(matrixes[3].Value);
            resMatrix.FillSubMatrix(parts[0, 1], 0, resMatrix.Width / 2);
            parts[1, 0] = matrixes[4].Value;
            parts[1, 0].Add(matrixes[5].Value);
            resMatrix.FillSubMatrix(parts[1, 0], resMatrix.Height / 2, 0);
            parts[1, 1] = matrixes[6].Value;
            parts[1, 1].Add(matrixes[7].Value);
            resMatrix.FillSubMatrix(parts[1, 1], resMatrix.Height / 2, resMatrix.Width / 2);
            return resMatrix;
        }

        private void SaveMatrix(Matrix matrix)
        {
        }
    }
}
