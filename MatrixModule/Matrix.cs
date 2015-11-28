using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatrixModule
{
    [Serializable]
    public class Matrix
    {
        private double[,] _data;
        public int Height { get; private set; }
        public int Width { get; private set; }

        public Matrix(int heigth, int width)
        {
            Height = heigth;
            Width = width;
            _data = new double[Height, Width];
            RandomFill();
        }

        public Matrix SubMatrix(int top, int left, int height, int width)
        {
            Matrix subMatrix = null;
            if ((top >= 0) && (left >= 0) && (top + height <= Height) && (left + width <= Width))
            {
                subMatrix = new Matrix(height, width);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        subMatrix[y, x] = _data[top + y, left + x];
                    }
                }
            }

            return subMatrix;
        }

        public double this[int x, int y]
        {
            get
            {
                return _data[x, y];
            }

            set
            {
                _data[x, y] = value;
            }
        }


        
        public Matrix Add(Matrix matrix)
        {
            if (matrix.Width != this.Width || matrix.Height != this.Height)
            {
                Console.WriteLine("Different dimentions");
                return null;
            }

            for (int i = 0; i < Height; ++i)
            {
                for (int j = 0; j < Width; ++j)
                {
                    this[i, j] += matrix[i, j];
                }
            }

            return this;
        }

        public Matrix MultiplyBy(Matrix matrix)
        {
            Matrix resultMatrix = null;
            if (this.Width != matrix.Height)
            {
                Console.WriteLine("Cannot multiply matrixes with such dimentions");
            }
            
            else
            {
                resultMatrix = new Matrix(Height, matrix.Width);
                for (int i = 0; i < Height; i++)
                {
                    //The next line is for debugging purposes only
                    //System.out.println(this.toString());
                    for (int j = 0; j < matrix.Width; j++)
                    {
                        resultMatrix[i, j] = 0;
                        for (int pos = 0; pos < Width; pos++)
                        {
                            resultMatrix[i, j] = resultMatrix[i, j] + this[i, pos] * matrix[pos, j];
                        }
                    }
                }
                
                Assign(resultMatrix);
            }

            return resultMatrix;
        }

        public void Assign(Matrix matrix)
        {
            Height = matrix.Height;
            Width = matrix.Width;
            _data = new double[Height, Width];
            for (int i = 0; i < Height; ++i)
            {
                for (int j = 0; j < Width; ++j)
                {
                    this[i, j] = matrix[i, j];
                }
            }
        }

        public void FillSubMatrix(Matrix source, int top, int left)
        {
            if (top + source.Height <= this.Height && left + source.Width <= this.Width)
            {
                for (int i = 0; i < source.Height; i++)
                {
                    for (int j = 0; j < source.Width; j++)
                    {
                        _data[top + i, left + j] = source[i, j];
                    }
                }
            }
        }

        public void RandomFill()
        {
            var rand = new Random();
            for (int i = 0; i < Height; ++i)
            {
                for (int j = 0; j < Width; ++j)
                {
                    _data[i, j] = rand.Next() + rand.NextDouble();
                }
            }
        }

    }
}
