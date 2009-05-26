//-----------------------------------------------------------------------
// <copyright file="LinearAlgebra.Test.cs" company="Math.NET Project">
//    Copyright (c) 2002-2009, Christoph R�egg.
//    All Right Reserved.
// </copyright>
// <author>
//    Christoph R�egg, http://christoph.ruegg.name
// </author>
// <product>
//    Math.NET Iridium, part of the Math.NET Project.
//    http://mathnet.opensourcedotnet.info
// </product>
// <license type="opensource" name="LGPL" version="2 or later">
//    This program is free software; you can redistribute it and/or modify
//    it under the terms of the GNU Lesser General Public License as published 
//    by the Free Software Foundation; either version 2 of the License, or
//    any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public 
//    License along with this program; if not, write to the Free Software
//    Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
// </license>
//-----------------------------------------------------------------------

using System;
using NUnit.Framework;

namespace Iridium.Test.LinearAlgebraTests
{
    using MathNet.Numerics.LinearAlgebra;

    /// <summary>TestMatrix tests the functionality of the 
    /// Matrix class and associated decompositions.</summary>
    /// <remarks>
    /// <para>
    /// Detailed output is provided indicating the functionality being tested
    /// and whether the functionality is correctly implemented. Exception handling
    /// is also tested.
    /// </para>
    /// <para>
    /// The test is designed to run to completion and give a summary of any implementation errors
    /// encountered. The final output should be:
    /// </para>
    /// <BLOCKQUOTE><PRE><CODE>
    /// TestMatrix completed.
    /// Total errors reported: n1
    /// Total warning reported: n2
    /// </CODE></PRE></BLOCKQUOTE>
    /// <para>
    /// If the test does not run to completion, this indicates that there is a 
    /// substantial problem within the implementation that was not anticipated in the test design.  
    /// The stopping point should give an indication of where the problem exists.
    /// </para>
    /// </remarks>
    [TestFixture]
    public class LinearAlgebraTests
    {
        private static readonly Random random = new Random();

        [Test]
        public void MultiplyByDiagonal()
        {
            Matrix A = Matrix.Create(
                new double[3, 4] {
                    { 1, 2, 3, 4 },
                    { 3, 4, 5, 6 },
                    { 5, 6, 7, 8 }
                    });

            double[] diagonal = new double[3] { 0, 1, 2 };

            A.MultiplyLeftDiagonalInplace(new Vector(diagonal));

            Assert.That(A[0, 0], Is.EqualTo(0), "#A00");
            Assert.That(A[0, 1], Is.EqualTo(0), "#A01");
            Assert.That(A[1, 0], Is.EqualTo(3), "#A02");
            Assert.That(A[1, 1], Is.EqualTo(4), "#A03");
            Assert.That(A[2, 0], Is.EqualTo(10), "#A04");
            Assert.That(A[2, 1], Is.EqualTo(12), "#A05");
        }

        [Test]
        public void MultiplyByMatrix()
        {
            Matrix A = Matrix.Create(
                new double[3, 4] {
                    { 10, -61, -8, -29 },
                    { 95, 11, -49, -47 },
                    { 40, -81, 91, 68 }
                    });

            Matrix B = Matrix.Create(
                new double[4, 2] {
                    { 72, 37 },
                    { -23, 87 },
                    { 44, 29 },
                    { 98, -23 }
                    });

            Matrix C = Matrix.Create(
                new double[3, 2] {
                    { -1071, -4502 },
                    {  -175, 4132 },
                    { 15411, -4492 }
                    });

            Matrix P = A.Multiply(B);

            Assert.That(C.ColumnCount, Is.EqualTo(P.ColumnCount), "#A00 Invalid column count in linear product.");
            Assert.That(C.RowCount, Is.EqualTo(P.RowCount), "#A01 Invalid row count in linear product.");

            for(int i = 0; i < C.RowCount; i++)
            {
                for(int j = 0; j < C.ColumnCount; j++)
                {
                    Assert.That(C[i, j], Is.EqualTo(P[i, j]), "#A02 Unexpected product value.");
                }
            }
        }

        [Test]
        public void SolveRobust()
        {
            Matrix A1 = Matrix.Create(
                new double[6, 2] {
                    { 1, 1 },
                    { 1, 2 },
                    { 1, 2 },
                    { 1, -1 },
                    { 0, 1 },
                    { 2, 1 }
                    });

            Matrix B1 = Matrix.Create(
                new double[6, 1] {
                    { 2 },
                    { 2 },
                    { 2 },
                    { 2 },
                    { 2 },
                    { 2 }
                    });

            Matrix X1 = A1.SolveRobust(B1);

            // [vermorel] Values have been computed with LAD function of Systat 12
            Assert.That(X1[0, 0], NumericIs.AlmostEqualTo(1.2), "#A00 Unexpected robust regression result.");
            Assert.That(X1[1, 0], NumericIs.AlmostEqualTo(0.4), "#A01 Unexpected robust regression result.");

            Matrix A2 = Matrix.Create(
                new double[6, 3] {
                    { 2, -1, 2 },
                    { 3, 2, 0 },
                    { 1, 2, 4 },
                    { 1, -1, -1 },
                    { 0, 1, 2 },
                    { 2, 1, 1 }
                    });

            Matrix B2 = Matrix.Create(
                new double[6, 1] {
                    { 0 },
                    { 4 },
                    { 2 },
                    { -3 },
                    { 2 },
                    { 1 }
                    });

            Matrix X2 = A2.SolveRobust(B2);

            // [vermorel] Values have been computed with LAD function of Systat 12
            Assert.That(X2[0, 0], NumericIs.AlmostEqualTo(0.667, 1e-3), "#A02 Unexpected robust regression result.");
            Assert.That(X2[1, 0], NumericIs.AlmostEqualTo(1.0, 1e-5), "#A03 Unexpected robust regression result.");
            Assert.That(X2[2, 0], NumericIs.AlmostEqualTo(-0.167, 1e-2), "#A04 Unexpected robust regression result.");

            Matrix A3 = Matrix.Create(
                new double[10, 4] {
                    { -8, -29, 95, 11 },
                    { -47, 40, -81, 91 },
                    { -10, 31, -51, 77 },
                    { 1, 1, 55, -28 },
                    { 30, -27, -15, -59 },
                    { 72, -87, 47, -90 },
                    { 92, -91, -88, -48 },
                    { -28, 5, 13, -10 },
                    { 71, 16, 83, 9 },
                    { -83, 98, -48, -19 }
                    });

            Matrix B3 = Matrix.Create(
                new double[10, 1] {
                    { -49 },
                    { 68 },
                    { 95 },
                    { 16 },
                    { -96 },
                    { 43 },
                    { 53 },
                    { -82 },
                    { -60 },
                    { 62 }
                    });

            Matrix X3 = A3.SolveRobust(B3);

            // [vermorel] Values have been computed with LAD function of Systat 12
            Assert.That(X3[0, 0], NumericIs.AlmostEqualTo(-0.104, 1e-2), "#A05 Unexpected robust regression result.");
            Assert.That(X3[1, 0], NumericIs.AlmostEqualTo(-0.216, 1e-2), "#A06 Unexpected robust regression result.");
            Assert.That(X3[2, 0], NumericIs.AlmostEqualTo(-0.618, 1e-3), "#A07 Unexpected robust regression result.");
            Assert.That(X3[3, 0], NumericIs.AlmostEqualTo(0.238, 1e-3), "#A08 Unexpected robust regression result.");
        }

        /// <summary>
        /// Testing the method <see cref="Matrix.SingularValueDecomposition"/>.
        /// </summary>
        [Test]
        public void SingularValueDecomposition()
        {
            for(int k = 0; k < 20; k++)
            {
                Matrix matrix = Matrix.Random(10, 8 + random.Next(5));

                SingularValueDecomposition svd = matrix.SingularValueDecomposition;

                Matrix U = svd.LeftSingularVectors;
                Matrix Vt = svd.RightSingularVectors;
                Vt.TransposeInplace();
                Matrix product = U * svd.S * Vt;

                for(int i = 0; i < matrix.RowCount; i++)
                {
                    for(int j = 0; j < matrix.ColumnCount; j++)
                    {
                        Assert.That(product[i, j], NumericIs.AlmostEqualTo(matrix[i, j], 1e-10), "#A00");
                    }
                }
            }
        }

        // TODO: rewrite AllTests in a more NUnit style

        /// <summary>An exception is thrown at the end of the process, 
        /// if any error is encountered.</summary>
        [Test]
        public void AllTests()
        {
            Matrix A, B, C, Z, O, I, R, S, X, SUB, M, T, SQ, DEF, SOL;
            double tmp;
            double[] columnwise = { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0 };
            double[] rowwise = { 1.0, 4.0, 7.0, 10.0, 2.0, 5.0, 8.0, 11.0, 3.0, 6.0, 9.0, 12.0 };
            double[][] avals = { new double[] { 1.0, 4.0, 7.0, 10.0 }, new double[] { 2.0, 5.0, 8.0, 11.0 }, new double[] { 3.0, 6.0, 9.0, 12.0 } };
            double[][] rankdef = avals;
            double[][] tvals = { new double[] { 1.0, 2.0, 3.0 }, new double[] { 4.0, 5.0, 6.0 }, new double[] { 7.0, 8.0, 9.0 }, new double[] { 10.0, 11.0, 12.0 } };
            double[][] subavals = { new double[] { 5.0, 8.0, 11.0 }, new double[] { 6.0, 9.0, 12.0 } };
            double[][] pvals = { new double[] { 25, -5, 10 }, new double[] { -5, 17, 10 }, new double[] { 10, 10, 62 } };
            double[][] ivals = { new double[] { 1.0, 0.0, 0.0, 0.0 }, new double[] { 0.0, 1.0, 0.0, 0.0 }, new double[] { 0.0, 0.0, 1.0, 0.0 } };
            double[][] evals = { new double[] { 0.0, 1.0, 0.0, 0.0 }, new double[] { 1.0, 0.0, 2e-7, 0.0 }, new double[] { 0.0, -2e-7, 0.0, 1.0 }, new double[] { 0.0, 0.0, 1.0, 0.0 } };
            double[][] square = { new double[] { 166.0, 188.0, 210.0 }, new double[] { 188.0, 214.0, 240.0 }, new double[] { 210.0, 240.0, 270.0 } };
            double[][] sqSolution = { new double[] { 13.0 }, new double[] { 15.0 } };
            double[][] condmat = { new double[] { 1.0, 3.0 }, new double[] { 7.0, 9.0 } };
            int rows = 3, cols = 4;
            int invalidld = 5; /* should trigger bad shape for construction with val */
            int validld = 3; /* leading dimension of intended test Matrices */
            int nonconformld = 4; /* leading dimension which is valid, but nonconforming */
            int ib = 1, ie = 2, jb = 1, je = 3; /* index ranges for sub Matrix */
            int[] rowindexset = new int[] { 1, 2 };
            int[] badrowindexset = new int[] { 1, 3 };
            int[] columnindexset = new int[] { 1, 2, 3 };
            int[] badcolumnindexset = new int[] { 1, 2, 4 };
            double columnsummax = 33.0;
            double rowsummax = 30.0;
            double sumofdiagonals = 15;
            double sumofsquares = 650;

            /***** Testing constructors and constructor-like methods *****/

            /* 
            Constructors and constructor-like methods:
             double[], int
             double[,]  
             int, int
             int, int, double
             int, int, double[,]
             Create(double[,])
             Random(int,int)
             Identity(int)
            */

            Assert.That(delegate() { A = new Matrix(columnwise, invalidld); }, Throws.TypeOf<ArgumentException>());

            A = new Matrix(columnwise, validld);
            B = new Matrix(avals);
            tmp = B[0, 0];
            avals[0][0] = 0.0;
            C = B - A;
            avals[0][0] = tmp;
            B = Matrix.Create(avals);
            tmp = B[0, 0];
            avals[0][0] = 0.0;
            Assert.That((tmp - B[0, 0]), Is.EqualTo(0.0), "Create");

            avals[0][0] = columnwise[0];
            I = new Matrix(ivals);
            Assert.That(Matrix.Identity(3, 4), NumericIs.AlmostEqualTo(I), "Identity");

            /***** Testing access methods *****/

            // Access Methods:
            // getColumnDimension()
            // getRowDimension()
            // getArray()
            // getArrayCopy()
            // getColumnPackedCopy()
            // getRowPackedCopy()
            // get(int,int)
            // GetMatrix(int,int,int,int)
            // GetMatrix(int,int,int[])
            // GetMatrix(int[],int,int)
            // GetMatrix(int[],int[])
            // set(int,int,double)
            // SetMatrix(int,int,int,int,Matrix)
            // SetMatrix(int,int,int[],Matrix)
            // SetMatrix(int[],int,int,Matrix)
            // SetMatrix(int[],int[],Matrix)

            // Various get methods
            B = new Matrix(avals);
            Assert.That(rows, Is.EqualTo(B.RowCount), "getRowDimension");
            Assert.That(cols, Is.EqualTo(B.ColumnCount), "getColumnDimension");

            B = new Matrix(avals);
            double[][] barray = (Matrix)B;
            Assert.That(barray, Is.SameAs(avals), "getArray");

            barray = B.Clone();
            Assert.That(barray, Is.Not.SameAs(avals), "getArrayCopy");
            Assert.That(B, NumericIs.AlmostEqualTo(new Matrix(barray)), "getArrayCopy II");

            ////double[] bpacked = B.ColumnPackedCopy;
            ////try
            ////{
            ////    check(bpacked, columnwise);
            ////    try_success("getColumnPackedCopy... ", "");
            ////}
            ////catch(System.SystemException e)
            ////{
            ////    errorCount = try_failure(errorCount, "getColumnPackedCopy... ", "data not successfully (deep) copied by columns");
            ////    System.Console.Out.WriteLine(e.Message);
            ////}
            ////bpacked = B.RowPackedCopy;
            ////try
            ////{
            ////    check(bpacked, rowwise);
            ////    try_success("getRowPackedCopy... ", "");
            ////}
            ////catch(System.SystemException e)
            ////{
            ////    errorCount = try_failure(errorCount, "getRowPackedCopy... ", "data not successfully (deep) copied by rows");
            ////    System.Console.Out.WriteLine(e.Message);
            ////}

            Assert.That(delegate() { tmp = B[B.RowCount, B.ColumnCount - 1]; }, Throws.TypeOf<IndexOutOfRangeException>());
            Assert.That(delegate() { tmp = B[B.RowCount - 1, B.ColumnCount]; }, Throws.TypeOf<IndexOutOfRangeException>());

            Assert.That(B[B.RowCount - 1, B.ColumnCount - 1], Is.EqualTo(avals[B.RowCount - 1][B.ColumnCount - 1]), "get(int,int)");

            SUB = new Matrix(subavals);

            Assert.That(delegate() { M = B.GetMatrix(ib, ie + B.RowCount + 1, jb, je); }, Throws.TypeOf<IndexOutOfRangeException>());
            Assert.That(delegate() { M = B.GetMatrix(ib, ie, jb, je + B.ColumnCount + 1); }, Throws.TypeOf<IndexOutOfRangeException>());

            M = B.GetMatrix(ib, ie, jb, je);
            Assert.That(M, NumericIs.AlmostEqualTo(SUB), "GetMatrix(int,int,int,int)");

            Assert.That(delegate() { M = B.GetMatrix(ib, ie, badcolumnindexset); }, Throws.TypeOf<IndexOutOfRangeException>());
            Assert.That(delegate() { M = B.GetMatrix(ib, ie + B.RowCount + 1, columnindexset); }, Throws.TypeOf<IndexOutOfRangeException>());

            M = B.GetMatrix(ib, ie, columnindexset);
            Assert.That(M, NumericIs.AlmostEqualTo(SUB), "GetMatrix(int,int,int[])");

            Assert.That(delegate() { M = B.GetMatrix(badrowindexset, jb, je); }, Throws.TypeOf<IndexOutOfRangeException>());
            Assert.That(delegate() { M = B.GetMatrix(rowindexset, jb, je + B.ColumnCount + 1); }, Throws.TypeOf<IndexOutOfRangeException>());

            M = B.GetMatrix(rowindexset, jb, je);
            Assert.That(M, NumericIs.AlmostEqualTo(SUB), "GetMatrix(int[],int,int)");

            Assert.That(delegate() { M = B.GetMatrix(badrowindexset, columnindexset); }, Throws.TypeOf<IndexOutOfRangeException>());
            Assert.That(delegate() { M = B.GetMatrix(rowindexset, badcolumnindexset); }, Throws.TypeOf<IndexOutOfRangeException>());

            M = B.GetMatrix(rowindexset, columnindexset);
            Assert.That(M, NumericIs.AlmostEqualTo(SUB), "GetMatrix(int[],int[])");

            // Various set methods:
            Assert.That(delegate() { B[B.RowCount, B.ColumnCount - 1] = 0.0; }, Throws.TypeOf<IndexOutOfRangeException>());
            Assert.That(delegate() { B[B.RowCount - 1, B.ColumnCount] = 0.0; }, Throws.TypeOf<IndexOutOfRangeException>());

            B[ib, jb] = 0.0;
            tmp = B[ib, jb];
            Assert.That(0.0, NumericIs.AlmostEqualTo(tmp), "set(int,int,double)");

            M = new Matrix(2, 3, 0.0);

            Assert.That(delegate() { B.SetMatrix(ib, ie + B.RowCount + 1, jb, je, M); }, Throws.TypeOf<IndexOutOfRangeException>());
            Assert.That(delegate() { B.SetMatrix(ib, ie, jb, je + B.ColumnCount + 1, M); }, Throws.TypeOf<IndexOutOfRangeException>());

            B.SetMatrix(ib, ie, jb, je, M);
            Assert.That(M, NumericIs.AlmostEqualTo(M - B.GetMatrix(ib, ie, jb, je)), "SetMatrix(int,int,int,int,Matrix)");
            B.SetMatrix(ib, ie, jb, je, SUB);

            Assert.That(delegate() { B.SetMatrix(ib, ie + B.RowCount + 1, columnindexset, M); }, Throws.TypeOf<IndexOutOfRangeException>());
            Assert.That(delegate() { B.SetMatrix(ib, ie, badcolumnindexset, M); }, Throws.TypeOf<IndexOutOfRangeException>());

            B.SetMatrix(ib, ie, columnindexset, M);
            Assert.That(M, NumericIs.AlmostEqualTo(M - B.GetMatrix(ib, ie, columnindexset)), "SetMatrix(int,int,int[],Matrix)");
            B.SetMatrix(ib, ie, jb, je, SUB);

            Assert.That(delegate() { B.SetMatrix(rowindexset, jb, je + B.ColumnCount + 1, M); }, Throws.TypeOf<IndexOutOfRangeException>());
            Assert.That(delegate() { B.SetMatrix(badrowindexset, jb, je, M); }, Throws.TypeOf<IndexOutOfRangeException>());

            B.SetMatrix(rowindexset, jb, je, M);
            Assert.That(M, NumericIs.AlmostEqualTo(M - B.GetMatrix(rowindexset, jb, je)), "SetMatrix(int[],int,int,Matrix)");

            B.SetMatrix(ib, ie, jb, je, SUB);

            Assert.That(delegate() { B.SetMatrix(rowindexset, badcolumnindexset, M); }, Throws.TypeOf<IndexOutOfRangeException>());
            Assert.That(delegate() { B.SetMatrix(badrowindexset, columnindexset, M); }, Throws.TypeOf<IndexOutOfRangeException>());

            B.SetMatrix(rowindexset, columnindexset, M);
            Assert.That(M, NumericIs.AlmostEqualTo(M - B.GetMatrix(rowindexset, columnindexset)), "SetMatrix(int[],int[],Matrix)");

            /***** Testing array-like methods *****/

            /*
            Array-like methods:
             Subtract
             SubtractEquals
             Add
             AddEquals
             ArrayLeftDivide
             ArrayLeftDivideEquals
             ArrayRightDivide
             ArrayRightDivideEquals
             arrayTimes
             ArrayMultiplyEquals
             uminus
            */

            S = new Matrix(columnwise, nonconformld);
            R = Matrix.Random(A.RowCount, A.ColumnCount);
            A = R;

            Assert.That(delegate() { S = A - S; }, Throws.TypeOf<ArgumentException>());

            Assert.That((A - R).Norm1(), Is.EqualTo(0.0), "Subtract: difference of identical Matrices is nonzero,\nSubsequent use of Subtract should be suspect");

            A = R.Clone();
            A.SubtractInplace(R);
            Z = new Matrix(A.RowCount, A.ColumnCount);

            Assert.That(delegate() { A.SubtractInplace(S); }, Throws.TypeOf<ArgumentException>());

            Assert.That((A - Z).Norm1(), Is.EqualTo(0.0), "SubtractEquals: difference of identical Matrices is nonzero,\nSubsequent use of Subtract should be suspect");

            A = R.Clone();
            B = Matrix.Random(A.RowCount, A.ColumnCount);
            C = A - B;

            Assert.That(delegate() { S = A + S; }, Throws.TypeOf<ArgumentException>());

            Assert.That(A, NumericIs.AlmostEqualTo(C + B), "Add");

            C = A - B;
            C.AddInplace(B);

            Assert.That(delegate() { A.AddInplace(S); }, Throws.TypeOf<ArgumentException>());

            Assert.That(A, NumericIs.AlmostEqualTo(C), "AddEquals");

            A = ((Matrix)R.Clone());
            A.NegateInplace();
            Assert.That(Z, NumericIs.AlmostEqualTo(A + R), "UnaryMinus");

            A = (Matrix)R.Clone();
            O = new Matrix(A.RowCount, A.ColumnCount, 1.0);

            Assert.That(delegate() { Matrix.ArrayDivide(A, S); }, Throws.TypeOf<ArgumentException>());

            C = Matrix.ArrayDivide(A, R);
            Assert.That(O, NumericIs.AlmostEqualTo(C), "ArrayRightDivide");

            Assert.That(delegate() { A.ArrayDivide(S); }, Throws.TypeOf<ArgumentException>());

            A.ArrayDivide(R);
            Assert.That(O, NumericIs.AlmostEqualTo(A), "ArrayRightDivideEquals");

            A = (Matrix)R.Clone();
            B = Matrix.Random(A.RowCount, A.ColumnCount);

            Assert.That(delegate() { S = Matrix.ArrayMultiply(A, S); }, Throws.TypeOf<ArgumentException>());

            C = Matrix.ArrayMultiply(A, B);
            C.ArrayDivide(B);
            Assert.That(A, NumericIs.AlmostEqualTo(C), "arrayTimes");

            Assert.That(delegate() { A.ArrayMultiply(S); }, Throws.TypeOf<ArgumentException>());

            A.ArrayMultiply(B);
            A.ArrayDivide(B);
            Assert.That(R, NumericIs.AlmostEqualTo(A), "ArrayMultiplyEquals");

            /***** Testing linear algebra methods *****/

            /*
            LA methods:
             Transpose
             Multiply
             Condition
             Rank
             Determinant
             trace
             Norm1
             norm2
             normF
             normInf
             Solve
             solveTranspose
             Inverse
             chol
             Eigen
             lu
             qr
             svd 
            */

            A = new Matrix(columnwise, 3);
            T = new Matrix(tvals);
            T = Matrix.Transpose(A);
            Assert.That(T, NumericIs.AlmostEqualTo(Matrix.Transpose(A)), "Transpose");
            Assert.That(columnsummax, NumericIs.AlmostEqualTo(A.Norm1()), "Norm1");
            Assert.That(rowsummax, NumericIs.AlmostEqualTo(A.NormInf()), "NormInf");
            Assert.That(Math.Sqrt(sumofsquares), NumericIs.AlmostEqualTo(A.NormF()), "NormF");
            Assert.That(sumofdiagonals, NumericIs.AlmostEqualTo(A.Trace()), "Trace");
            Assert.That(0.0, NumericIs.AlmostEqualTo(A.GetMatrix(0, A.RowCount - 1, 0, A.RowCount - 1).Determinant()), "Determinant");

            SQ = new Matrix(square);
            Assert.That(SQ, NumericIs.AlmostEqualTo(A * Matrix.Transpose(A)), "Multiply(Matrix)");
            Assert.That(Z, NumericIs.AlmostEqualTo(0.0 * A), "Multiply(double)");

            A = new Matrix(columnwise, 4);
            QRDecomposition QR = A.QRDecomposition;
            R = QR.R;
            Assert.That(QR.Q * R, NumericIs.AlmostEqualTo(A), "QRDecomposition");

            SingularValueDecomposition SVD = A.SingularValueDecomposition;
            Assert.That(SVD.LeftSingularVectors * (SVD.S * Matrix.Transpose(SVD.RightSingularVectors)), NumericIs.AlmostEqualTo(A), "SingularValueDecomposition");

            DEF = new Matrix(rankdef);
            Assert.That((double) (Math.Min(DEF.RowCount, DEF.ColumnCount) - 1), NumericIs.AlmostEqualTo((double) DEF.Rank()), "Rank");

            B = new Matrix(condmat);
            SVD = B.SingularValueDecomposition;
            double[] singularvalues = SVD.SingularValues;
            Assert.That(singularvalues[0] / singularvalues[Math.Min(B.RowCount, B.ColumnCount) - 1], NumericIs.AlmostEqualTo(B.Condition()), "Condition");

            int n = A.ColumnCount;
            A = A.GetMatrix(0, n - 1, 0, n - 1);
            A[0, 0] = 0.0;
            LUDecomposition LU = A.LUDecomposition;
            Assert.That(LU.L * LU.U, NumericIs.AlmostEqualTo(A.GetMatrix(LU.Pivot, 0, n - 1)), "LUDecomposition");

            X = A.Inverse();
            Assert.That(Matrix.Identity(3, 3), NumericIs.AlmostEqualTo(A * X, 1e-14), "Inverse");

            O = new Matrix(SUB.RowCount, 1, 1.0);
            SOL = new Matrix(sqSolution);
            SQ = SUB.GetMatrix(0, SUB.RowCount - 1, 0, SUB.RowCount - 1);
            Assert.That(O, NumericIs.AlmostEqualTo(SQ.Solve(SOL)), "Solve");

            A = new Matrix(pvals);
            CholeskyDecomposition Chol = A.CholeskyDecomposition;
            Matrix L = Chol.TriangularFactor;
            Assert.That(L * Matrix.Transpose(L), NumericIs.AlmostEqualTo(A), "CholeskyDecomposition");

            X = Chol.Solve(Matrix.Identity(3, 3));
            Assert.That(Matrix.Identity(3, 3), NumericIs.AlmostEqualTo(A * X), "CholeskyDecomposition Solve");

            EigenvalueDecomposition Eig = A.EigenvalueDecomposition;
            Matrix D = Eig.BlockDiagonal;
            Matrix V = Eig.EigenVectors;
            Assert.That(V * D, NumericIs.AlmostEqualTo(A * V), "EigenvalueDecomposition (symmetric)");

            A = new Matrix(evals);
            Eig = A.EigenvalueDecomposition;
            D = Eig.BlockDiagonal;
            V = Eig.EigenVectors;
            Assert.That(V * D, NumericIs.AlmostEqualTo(A * V, 1e-14), "EigenvalueDecomposition (nonsymmetric)");
        }
    }
}
