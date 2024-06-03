namespace CSharp2sem
{
    using System.Diagnostics;
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class Matrix
    {
        private long _rows;
        private long _columns;
        private ulong[,] _data;

        public Matrix(long rows, long columns)
        {
            _rows = rows;
            _columns = columns;
            _data = new ulong[rows, columns];
        }

        public long getCol() => _columns;
        public long getRow() => _rows;

        public ulong this[long row, long col]
        {
            get => _data[row, col];
            set => _data[row, col] = value;
        }
    }

    public static class MatrixIO
    {
        public static async Task CreateNewMatrixFile(string path, long rows, long columns)
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                Random rnd = new Random();

                string sizeLine = $"{rows} {columns}\n";
                byte[] sizeLineBytes = Encoding.UTF8.GetBytes(sizeLine);
                await stream.WriteAsync(sizeLineBytes, 0, sizeLineBytes.Length);

                for (long i = 0; i < rows; i++)
                {
                    StringBuilder lineBuilder = new StringBuilder();
                    for (long j = 0; j < columns; j++)
                    {
                        lineBuilder.Append(rnd.Next(0, 1000));
                        if (j < columns - 1)
                        {
                            lineBuilder.Append(" ");
                        }
                    }
                    lineBuilder.Append("\n");

                    byte[] lineBytes = Encoding.UTF8.GetBytes(lineBuilder.ToString());
                    await stream.WriteAsync(lineBytes, 0, lineBytes.Length);
                }
            }
        }

        public static async Task<Matrix> ReadMatrixAsync(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Файл с именем {path} не был найден.");
            }

            var lines = await File.ReadAllLinesAsync(path);

            var dimensions = lines[0].Split(' ');
            long rows = long.Parse(dimensions[0]);
            long columns = long.Parse(dimensions[1]);

            if (rows == 0 || columns == 0)
            {
                throw new InvalidOperationException($"Матрицы с размерностью {rows}x{columns} не существует, так как один из параметров равен 0.");
            }

            var matrix = new Matrix(rows, columns);

            try
            {
                Parallel.For(1, rows + 1, i =>
                {
                    var values = lines[i].Split(' ');

                    for (long j = 0; j < columns; j++)
                    {
                        matrix[i - 1, j] = ulong.Parse(values[j]);
                    }
                });
            }
            catch (IndexOutOfRangeException)
            {
                throw new IndexOutOfRangeException($"Выход за пределы массива. Возможно, ошибка в размерности матрицы {rows}x{columns}.");
            }
            catch (FormatException)
            {
                throw new FormatException($"Размерность матрицы не совпадает с количеством столбцов и строк. Возможно, ошибка в индексировании.");
            }

            return matrix;
        }

        public static async Task WriteMatrixAsync(Matrix matrix, string path)
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                long size_row = matrix.getRow();
                long size_col = matrix.getCol();

                string sizeLine = $"{size_row} {size_col}\n";
                byte[] sizeLineBytes = Encoding.UTF8.GetBytes(sizeLine);
                await stream.WriteAsync(sizeLineBytes, 0, sizeLineBytes.Length);

                for (long i = 0; i < size_row; i++)
                {
                    StringBuilder lineBuilder = new StringBuilder();
                    for (long j = 0; j < size_col; j++)
                    {
                        lineBuilder.Append(matrix[i, j].ToString());
                        if (j < size_col - 1)
                        {
                            lineBuilder.Append(" ");
                        }
                    }
                    lineBuilder.Append("\n");

                    byte[] lineBytes = Encoding.UTF8.GetBytes(lineBuilder.ToString());
                    await stream.WriteAsync(lineBytes, 0, lineBytes.Length);
                }
            }
        }
    }

    public static class MatrixMultiplier
    {
        public static Matrix Multiply(Matrix a, Matrix b)
        {
            long size_row_a = a.getRow();
            long size_col_a = a.getCol();
            long size_col_b = b.getCol();

            if (size_col_a != b.getRow())
                throw new InvalidOperationException("Невозможно перемножить матрицы, так как количество столбцов матрицы A и количество строк матрицы B не совпадают.");

            var result = new Matrix(size_row_a, size_col_b);

            Parallel.For(0, size_row_a, i =>
            {
                for (long j = 0; j < size_col_b; j++)
                {
                    ulong sum = 0;
                    for (long k = 0; k < size_col_a; k++)
                    {
                        sum += a[i, k] * b[k, j];
                    }
                    result[i, j] = sum;
                }
            });

            return result;
        }
    }

    class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                await MatrixIO.CreateNewMatrixFile("matrixA.txt", 1000, 1000);
                await MatrixIO.CreateNewMatrixFile("matrixB.txt", 1000, 1000);


                var matrixA = await MatrixIO.ReadMatrixAsync("matrixA.txt");
                var matrixB = await MatrixIO.ReadMatrixAsync("matrixB.txt");

                var stopwatch = Stopwatch.StartNew();

                var task1 = Task.Run(() => MatrixMultiplier.Multiply(matrixA, matrixB));

                var resultMatrix = await task1;

                stopwatch.Stop();

                await MatrixIO.WriteMatrixAsync(resultMatrix, "result.txt");

                Console.WriteLine($"Matrix multiplication completed in {stopwatch.ElapsedMilliseconds} ms");

                return 0;

            }
            catch (IndexOutOfRangeException ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex.Message);
                return 2;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                return 3;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                return 5;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 6;
            }
        }
    }
}