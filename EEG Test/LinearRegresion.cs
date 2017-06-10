using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEG_Test
{
    public class LinearRegression
    {
        //Получение прогноза
        public static Double Forecast(Double[] x, Double[] w)
        {
            Double Result;
            Result = w[0];
            for (int i = 0; i < x.Length; i++)
            {
                Result += x[i] * w[i + 1];
            }
            return Result;
        }

        public static Double RSquared(Double[][] data, Double[] coef)
        {
            // 'coefficient of determination'
            int rows = data.Length;
            int cols = data[0].Length;

            // 1. compute mean of y
            Double ySum = 0.0;
            for (int i = 0; i < rows; ++i)
                ySum += data[i][cols - 1]; // last column
            Double yMean = ySum / rows;

            // 2. sum of squared residuals & tot sum squares
            Double ssr = 0.0;
            Double sst = 0.0;
            Double y; // actual y value
            Double predictedY; // using the coef[] 
            for (int i = 0; i < rows; ++i)
            {
                y = data[i][cols - 1]; // get actual y

                predictedY = coef[0]; // start w/ intercept constant
                for (int j = 0; j < cols - 1; ++j) // j is col of data
                    predictedY += coef[j + 1] * data[i][j]; // careful

                ssr += (y - predictedY) * (y - predictedY);
                sst += (y - yMean) * (y - yMean);
            }

            if (sst == 0.0)
                throw new Exception("All y values equal");
            else
                return 1.0 - (ssr / sst);
        }

        //Создаёт матрицу плана 
        public static Double[][] Design(Double[][] data)
        {
            int rows = data.Length;
            int cols = data[0].Length;
            Double[][] result = MatrixCreate(rows, cols + 1);
            for (int i = 0; i < rows; ++i)
                result[i][0] = 1.0;

            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < cols; ++j)
                    result[i][j + 1] = data[i][j];

            return result;
        }
        //Разрешение регрессии методом наименьших квадратов
        public static Double[] Solve(Double[][] design)
        {
            int rows = design.Length;
            int cols = design[0].Length;
            Double[][] X = MatrixCreate(rows, cols - 1);
            Double[][] Y = MatrixCreate(rows, 1);

            int j;
            for (int i = 0; i < rows; ++i)
            {
                for (j = 0; j < cols - 1; ++j)
                {
                    X[i][j] = design[i][j];
                }
                Y[i][0] = design[i][j];
            }

            // B = inv(Xt * X) * Xt * y
            Double[][] Xt = MatrixTranspose(X);
            Double[][] XtX = MatrixProduct(Xt, X);
            Double[][] inv = MatrixInverse(XtX);
            Double[][] invXt = MatrixProduct(inv, Xt);

            Double[][] mResult = MatrixProduct(invXt, Y);
            Double[] result = MatrixToVector(mResult);
            return result;
        }

        public static Double[][] MatrixCreate(int rows, int cols)
        {
            Double[][] result = new Double[rows][];
            for (int i = 0; i < rows; ++i)
                result[i] = new Double[cols];
            return result;
        }

        public static Double[] MatrixToVector(Double[][] matrix)
        {
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            if (cols != 1)
                throw new Exception("Bad matrix");
            Double[] result = new Double[rows];
            for (int i = 0; i < rows; ++i)
                result[i] = matrix[i][0];
            return result;
        }

        public static Double[][] MatrixIdentity(int n)
        {
            // return an n x n Identity matrix
            Double[][] result = MatrixCreate(n, n);
            for (int i = 0; i < n; ++i)
                result[i][i] = 1.0;

            return result;
        }

        public static string MatrixAsString(Double[][] matrix, int dec)
        {
            string s = "";
            for (int i = 0; i < matrix.Length; ++i)
            {
                for (int j = 0; j < matrix[i].Length; ++j)
                    s += matrix[i][j].ToString("F" + dec).PadLeft(8) + " ";
                s += Environment.NewLine;
            }
            return s;
        }

        public static bool MatrixAreEqual(Double[][] matrixA,
          Double[][] matrixB, Double epsilon)
        {
            int aRows = matrixA.Length; int aCols = matrixA[0].Length;
            int bRows = matrixB.Length; int bCols = matrixB[0].Length;
            if (aRows != bRows || aCols != bCols)
                throw new Exception("Non-conformable matrices in MatrixAreEqual");

            for (int i = 0; i < aRows; ++i)
                for (int j = 0; j < aCols; ++j)
                    if (Math.Abs(matrixA[i][j] - matrixB[i][j]) > epsilon)
                        return false;
            return true;
        }

        // -------------------------------------------------------------

        public static Double[][] MatrixProduct(Double[][] matrixA, Double[][] matrixB)
        {
            int aRows = matrixA.Length; int aCols = matrixA[0].Length;
            int bRows = matrixB.Length; int bCols = matrixB[0].Length;
            if (aCols != bRows)
                throw new Exception("Non-conformable matrices in MatrixProduct");

            Double[][] result = MatrixCreate(aRows, bCols);

            for (int i = 0; i < aRows; ++i) // each row of A
                for (int j = 0; j < bCols; ++j) // each col of B
                    for (int k = 0; k < aCols; ++k) // could use k < bRows
                        result[i][j] += matrixA[i][k] * matrixB[k][j];

            return result;
        }

        public static Double[] MatrixVectorProduct(Double[][] matrix, Double[] vector)
        {
            int mRows = matrix.Length; int mCols = matrix[0].Length;
            int vRows = vector.Length;
            if (mCols != vRows)
                throw new Exception("Non-conformable matrix and vector in MatrixVectorProduct");
            Double[] result = new Double[mRows];
            for (int i = 0; i < mRows; ++i)
                for (int j = 0; j < mCols; ++j)
                    result[i] += matrix[i][j] * vector[j];
            return result;
        }

        public static Double[][] MatrixDecompose(Double[][] matrix, out int[] perm,
          out int toggle)
        {
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            if (rows != cols)
                throw new Exception("Non-square mattrix");

            int n = rows;

            Double[][] result = MatrixDuplicate(matrix);

            perm = new int[n];
            for (int i = 0; i < n; ++i) { perm[i] = i; }

            toggle = 1;

            for (int j = 0; j < n - 1; ++j)
            {
                Double colMax = Math.Abs(result[j][j]);
                int pRow = j;

                for (int i = j + 1; i < n; ++i)
                {
                    if (Math.Abs(result[i][j]) > colMax)
                    {
                        colMax = Math.Abs(result[i][j]);
                        pRow = i;
                    }
                }

                if (pRow != j)
                {
                    Double[] rowPtr = result[pRow];
                    result[pRow] = result[j];
                    result[j] = rowPtr;

                    int tmp = perm[pRow]; // and swap perm info
                    perm[pRow] = perm[j];
                    perm[j] = tmp;

                    toggle = -toggle;
                }

                if (result[j][j] == 0.0)
                {
                    int goodRow = -1;
                    for (int row = j + 1; row < n; ++row)
                    {
                        if (result[row][j] != 0.0)
                            goodRow = row;
                    }

                    if (goodRow == -1)
                        throw new Exception("Cannot use Doolittle's method");

                    Double[] rowPtr = result[goodRow];
                    result[goodRow] = result[j];
                    result[j] = rowPtr;

                    int tmp = perm[goodRow];
                    perm[goodRow] = perm[j];
                    perm[j] = tmp;

                    toggle = -toggle;
                }

                for (int i = j + 1; i < n; ++i)
                {
                    result[i][j] /= result[j][j];
                    for (int k = j + 1; k < n; ++k)
                    {
                        result[i][k] -= result[i][j] * result[j][k];
                    }
                }

            }
            return result;
        }

        public static Double[][] MatrixInverse(Double[][] matrix)
        {
            int n = matrix.Length;
            Double[][] result = MatrixDuplicate(matrix);

            int [] perm;
            int toggle;
            Double[][] lum = MatrixDecompose(matrix, out perm, out toggle);
            if (lum == null)
                throw new Exception("Unable to compute inverse");

            Double[] b = new Double[n];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    if (i == perm[j])
                        b[j] = 1.0;
                    else
                        b[j] = 0.0;
                }

                Double[] x = HelperSolve(lum, b);

                for (int j = 0; j < n; ++j)
                    result[j][i] = x[j];
            }
            return result;
        }

        public static Double[][] MatrixTranspose(Double[][] matrix)
        {
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            Double[][] result = MatrixCreate(cols, rows); // note indexing
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    result[j][i] = matrix[i][j];
                }
            }
            return result;
        }

        public static Double MatrixDeterminant(Double[][] matrix)
        {
            int [] perm;
            int toggle;
            Double[][] lum = MatrixDecompose(matrix, out perm, out toggle);
            if (lum == null)
                throw new Exception("Unable to compute MatrixDeterminant");
            Double result = toggle;
            for (int i = 0; i < lum.Length; ++i)
                result *= lum[i][i];
            return result;
        }

        public static Double[] HelperSolve(Double[][] luMatrix, Double[] b)
        {
            int n = luMatrix.Length;
            Double[] x = new Double[n];
            b.CopyTo(x, 0);

            for (int i = 1; i < n; ++i)
            {
                Double sum = x[i];
                for (int j = 0; j < i; ++j)
                    sum -= luMatrix[i][j] * x[j];
                x[i] = sum;
            }

            x[n - 1] /= luMatrix[n - 1][n - 1];
            for (int i = n - 2; i >= 0; --i)
            {
                Double sum = x[i];
                for (int j = i + 1; j < n; ++j)
                    sum -= luMatrix[i][j] * x[j];
                x[i] = sum / luMatrix[i][i];
            }

            return x;
        }


        public static Double[][] MatrixDuplicate(Double[][] matrix)
        {
            Double[][] result = MatrixCreate(matrix.Length, matrix[0].Length);
            for (int i = 0; i < matrix.Length; ++i)
                for (int j = 0; j < matrix[i].Length; ++j)
                    result[i][j] = matrix[i][j];
            return result;
        }

        public static Double[][] ExtractLower(Double[][] matrix)
        {
            int rows = matrix.Length; int cols = matrix[0].Length;
            Double[][] result = MatrixCreate(rows, cols);
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    if (i == j)
                        result[i][j] = 1.0;
                    else if (i > j)
                        result[i][j] = matrix[i][j];
                }
            }
            return result;
        }

        public static Double[][] ExtractUpper(Double[][] matrix)
        {
            int rows = matrix.Length; int cols = matrix[0].Length;
            Double[][] result = MatrixCreate(rows, cols);
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    if (i <= j)
                        result[i][j] = matrix[i][j];
                }
            }
            return result;
        }

        public static Double[][] PermArrayToMatrix(int[] perm)
        {
            int n = perm.Length;
            Double[][] result = MatrixCreate(n, n);
            for (int i = 0; i < n; ++i)
                result[i][perm[i]] = 1.0;
            return result;
        }

        public static Double[][] UnPermute(Double[][] luProduct, int[] perm)
        {
            Double[][] result = MatrixDuplicate(luProduct);

            int[] unperm = new int[perm.Length];
            for (int i = 0; i < perm.Length; ++i)
                unperm[perm[i]] = i;

            for (int r = 0; r < luProduct.Length; ++r)
                result[r] = luProduct[unperm[r]];

            return result;
        }
    }
}
