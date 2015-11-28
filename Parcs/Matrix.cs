﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Parcs
{
    [Serializable]
    public class Matrix
    {
        private int[,] _data;
        public int Height { get; private set; }
        public int Width { get; private set; }
        private const int _maxRandomValue = 100;

        public Matrix(int heigth, int width, bool randomFill = false)
        {
            Height = heigth;
            Width = width;
            _data = new int[Height, Width];
            if (randomFill)
            {
                RandomFill();
            }
        }



        public Matrix SubMatrix(int top, int left, int height, int width)
        {
            Matrix subMatrix = null;
            if ((top >= 0) && (left >= 0) && (top + height <= Height) && (left + width <= Width))
            {
                subMatrix = new Matrix(height, width);
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        subMatrix[i, j] = _data[top + i, left + j];
                    }
                }
            }

            return subMatrix;
        }

        public int this[int x, int y]
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
                    for (int j = 0; j < matrix.Width; j++)
                    {
                        resultMatrix[i, j] = 0;
                        for (int pos = 0; pos < Width; pos++)
                        {
                            resultMatrix[i, j] += this[i, pos] * matrix[pos, j];
                        }
                    }
                }

            }

            return resultMatrix;
        }

        public void Assign(Matrix matrix)
        {
            Height = matrix.Height;
            Width = matrix.Width;
            _data = new int[Height, Width];
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
                    _data[i, j] = rand.Next(_maxRandomValue);
                }
            }
        }

        public static Matrix LoadFromFile(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                return LoadFromStream(stream);
            }
        }

        private static Matrix LoadFromStream(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                var m = reader.ReadInt32();
                var n = reader.ReadInt32();

                var matrix = new Matrix(m, n);

                for (var i = 0; i < m; i++)
                {
                    for (var j = 0; j < n; j++)
                    {
                        matrix[i, j] = reader.ReadInt32();
                    }
                }

                return matrix;
            }
        }

        public void WriteToFile(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                var writer = new BinaryWriter(stream);

                writer.Write(Width);
                writer.Write(Height);

                for (var i = 0; i < Width; i++)
                {
                    for (var j = 0; j < Height; j++)
                    {
                        writer.Write(this[i, j]);
                    }
                }
            }
        
        }



        
    }
}
