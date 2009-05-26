//-----------------------------------------------------------------------
// <copyright file="SingularValueDecomposition.cs" company="Math.NET Project">
//    Copyright (c) 2004-2009, Joannes Vermorel, Christoph R�egg.
//    All Right Reserved.
// </copyright>
// <author>
//    Joannes Vermorel, http://www.vermorel.com
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
// <contribution>
//    The MathWorks
//    NIST
// </contribution>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;

namespace MathNet.Numerics.LinearAlgebra
{
    /// <summary>Singular Value Decomposition.</summary>
    /// <remarks>
    /// <para>
    /// For an m-by-n matrix A with m >= n, the singular value decomposition 
    /// is an m-by-n orthogonal matrix U, an n-by-n diagonal matrix S, and
    /// an n-by-n orthogonal matrix V so that A = U*S*V'.
    /// </para>
    /// <para>
    /// The singular values, sigma[k] = S[k, k], are ordered so that
    /// sigma[0] >= sigma[1] >= ... >= sigma[n-1].
    /// </para>
    /// <para>
    /// The singular value decomposition always exists, so the constructor will
    /// never fail.  The matrix condition number and the effective numerical
    /// rank can be computed from this decomposition.
    /// </para>
    /// </remarks>
    [Serializable]
    public class SingularValueDecomposition
    {
        Matrix _u;
        Matrix _v;

        /// <summary>Array for internal storage of singular values.</summary>
        Vector _singular;

        /// <summary>Row dimensions.</summary>
        int m;

        /// <summary>Column dimensions.</summary>
        int n;

        /// <summary>Indicates whether all the results provided by the
        /// method or properties should be transposed.</summary>
        /// <remarks>
        /// (vermorel) The initial implementation was assuming that
        /// m &gt;= n, but in fact, it is easy to handle the case m &lt; n
        /// by transposing all the results.
        /// </remarks>
        bool transpose;

        OnDemandComputation<Matrix> _diagonalSingularValuesOnDemand;
        OnDemandComputation<int> _rankOnDemand;

        private enum IterationStep
        {
            /// <summary>if s[p] and e[k-1] are negligible and k&lt;p.</summary>
            DeflateNeglible,

            /// <summary>if s[k] is negligible and k&lt;p.</summary>
            SplitAtNeglible,

            /// <summary>if e[k-1] is negligible, k&lt;p, and s[k], ..., s[p] are not negligible.</summary>
            QR,

            /// <summary>if e[p-1] is negligible.</summary>
            Convergence
        }

        /// <summary>
        /// Initializes a new instance of the SingularValueDecomposition class.
        /// </summary>
        /// <remarks>Provides access to U, S and V.</remarks>
        /// <param name="Arg">Rectangular matrix</param>
        public
        SingularValueDecomposition(Matrix Arg)
        {
            transpose = (Arg.RowCount < Arg.ColumnCount);

            // Derived from LINPACK code.
            // Initialize.
            double[][] A;
            if(transpose)
            {
                // copy of internal data, independent of Arg
                A = Matrix.Transpose(Arg).GetArray();
                m = Arg.ColumnCount;
                n = Arg.RowCount;
            }
            else
            {
                A = Arg.CopyToJaggedArray();
                m = Arg.RowCount;
                n = Arg.ColumnCount;
            }

            int nu = Math.Min(m, n);
            double[] s = new double[Math.Min(m + 1, n)];
            double[][] U = Matrix.CreateMatrixData(m, nu);
            double[][] V = Matrix.CreateMatrixData(n, n);

            double[] e = new double[n];
            double[] work = new double[m];
            bool wantu = true;
            bool wantv = true;

            /*
            Reduce A to bidiagonal form, storing the diagonal elements
            in s and the super-diagonal elements in e.
            */

            int nct = Math.Min(m - 1, n);
            int nrt = Math.Max(0, Math.Min(n - 2, m));
            for(int k = 0; k < Math.Max(nct, nrt); k++)
            {
                if(k < nct)
                {
                    // Compute the transformation for the k-th column and
                    // place the k-th diagonal in s[k].
                    // Compute 2-norm of k-th column without under/overflow.
                    s[k] = 0;

                    for(int i = k; i < m; i++)
                    {
                        s[k] = Fn.Hypot(s[k], A[i][k]);
                    }

                    if(s[k] != 0.0)
                    {
                        if(A[k][k] < 0.0)
                        {
                            s[k] = -s[k];
                        }

                        for(int i = k; i < m; i++)
                        {
                            A[i][k] /= s[k];
                        }

                        A[k][k] += 1.0;
                    }

                    s[k] = -s[k];
                }

                for(int j = k + 1; j < n; j++)
                {
                    if((k < nct) & (s[k] != 0.0))
                    {
                        /* Apply the transformation */

                        double t = 0;
                        for(int i = k; i < m; i++)
                        {
                            t += A[i][k] * A[i][j];
                        }

                        t = (-t) / A[k][k];
                        for(int i = k; i < m; i++)
                        {
                            A[i][j] += t * A[i][k];
                        }
                    }

                    /*
                    Place the k-th row of A into e for the
                    subsequent calculation of the row transformation.
                    */

                    e[j] = A[k][j];
                }

                if(wantu & (k < nct))
                {
                    /*
                    Place the transformation in U for subsequent back
                    multiplication.
                    */

                    for(int i = k; i < m; i++)
                    {
                        U[i][k] = A[i][k];
                    }
                }

                if(k < nrt)
                {
                    // Compute the k-th row transformation and place the
                    // k-th super-diagonal in e[k].
                    // Compute 2-norm without under/overflow.
                    e[k] = 0;

                    for(int i = k + 1; i < n; i++)
                    {
                        e[k] = Fn.Hypot(e[k], e[i]);
                    }

                    if(e[k] != 0.0)
                    {
                        if(e[k + 1] < 0.0)
                        {
                            e[k] = -e[k];
                        }

                        for(int i = k + 1; i < n; i++)
                        {
                            e[i] /= e[k];
                        }

                        e[k + 1] += 1.0;
                    }

                    e[k] = -e[k];

                    if((k + 1 < m) & (e[k] != 0.0))
                    {
                        /* Apply the transformation */

                        for(int i = k + 1; i < m; i++)
                        {
                            work[i] = 0.0;
                        }

                        for(int j = k + 1; j < n; j++)
                        {
                            for(int i = k + 1; i < m; i++)
                            {
                                work[i] += e[j] * A[i][j];
                            }
                        }

                        for(int j = k + 1; j < n; j++)
                        {
                            double t = (-e[j]) / e[k + 1];
                            for(int i = k + 1; i < m; i++)
                            {
                                A[i][j] += t * work[i];
                            }
                        }
                    }

                    if(wantv)
                    {
                        /*
                        Place the transformation in V for subsequent
                        back multiplication.
                        */

                        for(int i = k + 1; i < n; i++)
                        {
                            V[i][k] = e[i];
                        }
                    }
                }
            }

            /* Set up the final bidiagonal matrix or order p. */

            int p = Math.Min(n, m + 1);

            if(nct < n)
            {
                s[nct] = A[nct][nct];
            }

            if(m < p)
            {
                s[p - 1] = 0.0;
            }

            if(nrt + 1 < p)
            {
                e[nrt] = A[nrt][p - 1];
            }

            e[p - 1] = 0.0;

            /* If required, generate U */

            if(wantu)
            {
                for(int j = nct; j < nu; j++)
                {
                    for(int i = 0; i < m; i++)
                    {
                        U[i][j] = 0.0;
                    }

                    U[j][j] = 1.0;
                }

                for(int k = nct - 1; k >= 0; k--)
                {
                    if(s[k] != 0.0)
                    {
                        for(int j = k + 1; j < nu; j++)
                        {
                            double t = 0;
                            for(int i = k; i < m; i++)
                            {
                                t += U[i][k] * U[i][j];
                            }

                            t = (-t) / U[k][k];
                            for(int i = k; i < m; i++)
                            {
                                U[i][j] += t * U[i][k];
                            }
                        }

                        for(int i = k; i < m; i++)
                        {
                            U[i][k] = -U[i][k];
                        }

                        U[k][k] = 1.0 + U[k][k];
                        for(int i = 0; i < k - 1; i++)
                        {
                            U[i][k] = 0.0;
                        }
                    }
                    else
                    {
                        for(int i = 0; i < m; i++)
                        {
                            U[i][k] = 0.0;
                        }

                        U[k][k] = 1.0;
                    }
                }
            }

            /* If required, generate V */

            if(wantv)
            {
                for(int k = n - 1; k >= 0; k--)
                {
                    if((k < nrt) & (e[k] != 0.0))
                    {
                        for(int j = k + 1; j < nu; j++)
                        {
                            double t = 0;
                            for(int i = k + 1; i < n; i++)
                            {
                                t += V[i][k] * V[i][j];
                            }

                            t = (-t) / V[k + 1][k];
                            for(int i = k + 1; i < n; i++)
                            {
                                V[i][j] += t * V[i][k];
                            }
                        }
                    }

                    for(int i = 0; i < n; i++)
                    {
                        V[i][k] = 0.0;
                    }

                    V[k][k] = 1.0;
                }
            }

            /* Main iteration loop for the singular values */

            int pp = p - 1;
            int iter = 0;
            double eps = Number.PositiveRelativeAccuracy;
            while(p > 0)
            {
                int k;
                IterationStep step;

                /* Here is where a test for too many iterations would go */

                /*
                This section of the program inspects for
                negligible elements in the s and e arrays.  On
                completion the variables kase and k are set as follows.

                DeflateNeglible:  if s[p] and e[k-1] are negligible and k<p
                SplitAtNeglible:  if s[k] is negligible and k<p
                QR:               if e[k-1] is negligible, k<p, and s[k], ..., s[p] are not negligible.
                Convergence:      if e[p-1] is negligible.
                */

                for(k = p - 2; k >= 0; k--)
                {
                    if(Math.Abs(e[k]) <= eps * (Math.Abs(s[k]) + Math.Abs(s[k + 1])))
                    {
                        e[k] = 0.0;
                        break;
                    }
                }

                if(k == p - 2)
                {
                    step = IterationStep.Convergence;
                }
                else
                {
                    int ks;
                    for(ks = p - 1; ks >= k; ks--)
                    {
                        if(ks == k)
                        {
                            break;
                        }

                        double t = (ks != p ? Math.Abs(e[ks]) : 0.0) + (ks != k + 1 ? Math.Abs(e[ks - 1]) : 0.0);
                        if(Math.Abs(s[ks]) <= eps * t)
                        {
                            s[ks] = 0.0;
                            break;
                        }
                    }

                    if(ks == k)
                    {
                        step = IterationStep.QR;
                    }
                    else if(ks == p - 1)
                    {
                        step = IterationStep.DeflateNeglible;
                    }
                    else
                    {
                        step = IterationStep.SplitAtNeglible;
                        k = ks;
                    }
                }

                k++;

                /* Perform the task indicated by 'step'. */

                switch(step)
                {
                    // Deflate negligible s(p).
                    case IterationStep.DeflateNeglible:
                        {
                            double f = e[p - 2];
                            e[p - 2] = 0.0;
                            for(int j = p - 2; j >= k; j--)
                            {
                                double t = Fn.Hypot(s[j], f);
                                double cs = s[j] / t;
                                double sn = f / t;
                                s[j] = t;

                                if(j != k)
                                {
                                    f = (-sn) * e[j - 1];
                                    e[j - 1] = cs * e[j - 1];
                                }

                                if(wantv)
                                {
                                    for(int i = 0; i < n; i++)
                                    {
                                        t = (cs * V[i][j]) + (sn * V[i][p - 1]);
                                        V[i][p - 1] = ((-sn) * V[i][j]) + (cs * V[i][p - 1]);
                                        V[i][j] = t;
                                    }
                                }
                            }
                        }

                        break;

                    // Split at negligible s(k)
                    case IterationStep.SplitAtNeglible:
                        {
                            double f = e[k - 1];
                            e[k - 1] = 0.0;
                            for(int j = k; j < p; j++)
                            {
                                double t = Fn.Hypot(s[j], f);
                                double cs = s[j] / t;
                                double sn = f / t;
                                s[j] = t;
                                f = (-sn) * e[j];
                                e[j] = cs * e[j];
                                if(wantu)
                                {
                                    for(int i = 0; i < m; i++)
                                    {
                                        t = (cs * U[i][j]) + (sn * U[i][k - 1]);
                                        U[i][k - 1] = ((-sn) * U[i][j]) + (cs * U[i][k - 1]);
                                        U[i][j] = t;
                                    }
                                }
                            }
                        }

                        break;

                    // Perform one qr step.
                    case IterationStep.QR:
                        {
                            /* Calculate the shift */

                            double scale = Math.Max(Math.Max(Math.Max(Math.Max(Math.Abs(s[p - 1]), Math.Abs(s[p - 2])), Math.Abs(e[p - 2])), Math.Abs(s[k])), Math.Abs(e[k]));
                            double sp = s[p - 1] / scale;
                            double spm1 = s[p - 2] / scale;
                            double epm1 = e[p - 2] / scale;
                            double sk = s[k] / scale;
                            double ek = e[k] / scale;
                            double b = (((spm1 + sp) * (spm1 - sp)) + (epm1 * epm1)) / 2.0;
                            double c = (sp * epm1) * (sp * epm1);
                            double shift = 0.0;
                            if((b != 0.0) | (c != 0.0))
                            {
                                shift = Math.Sqrt((b * b) + c);

                                if(b < 0.0)
                                {
                                    shift = -shift;
                                }

                                shift = c / (b + shift);
                            }

                            double f = ((sk + sp) * (sk - sp)) + shift;
                            double g = sk * ek;

                            /* Chase zeros */

                            for(int j = k; j < p - 1; j++)
                            {
                                double t = Fn.Hypot(f, g);
                                double cs = f / t;
                                double sn = g / t;

                                if(j != k)
                                {
                                    e[j - 1] = t;
                                }

                                f = (cs * s[j]) + (sn * e[j]);
                                e[j] = (cs * e[j]) - (sn * s[j]);
                                g = sn * s[j + 1];
                                s[j + 1] = cs * s[j + 1];

                                if(wantv)
                                {
                                    for(int i = 0; i < n; i++)
                                    {
                                        t = (cs * V[i][j]) + (sn * V[i][j + 1]);
                                        V[i][j + 1] = ((-sn) * V[i][j]) + (cs * V[i][j + 1]);
                                        V[i][j] = t;
                                    }
                                }

                                t = Fn.Hypot(f, g);
                                cs = f / t;
                                sn = g / t;
                                s[j] = t;
                                f = (cs * e[j]) + (sn * s[j + 1]);
                                s[j + 1] = ((-sn) * e[j]) + (cs * s[j + 1]);
                                g = sn * e[j + 1];
                                e[j + 1] = cs * e[j + 1];

                                if(wantu && (j < m - 1))
                                {
                                    for(int i = 0; i < m; i++)
                                    {
                                        t = (cs * U[i][j]) + (sn * U[i][j + 1]);
                                        U[i][j + 1] = ((-sn) * U[i][j]) + (cs * U[i][j + 1]);
                                        U[i][j] = t;
                                    }
                                }
                            }

                            e[p - 2] = f;
                            iter = iter + 1;
                        }

                        break;

                    // Convergence.
                    case IterationStep.Convergence:
                        {
                            /* Make the singular values positive */

                            if(s[k] <= 0.0)
                            {
                                s[k] = (s[k] < 0.0 ? -s[k] : 0.0);
                                if(wantv)
                                {
                                    for(int i = 0; i <= pp; i++)
                                    {
                                        V[i][k] = -V[i][k];
                                    }
                                }
                            }

                            /* Order the singular values */

                            while(k < pp)
                            {
                                if(s[k] >= s[k + 1])
                                {
                                    break;
                                }

                                double t = s[k];
                                s[k] = s[k + 1];
                                s[k + 1] = t;

                                if(wantv && (k < n - 1))
                                {
                                    for(int i = 0; i < n; i++)
                                    {
                                        t = V[i][k + 1];
                                        V[i][k + 1] = V[i][k];
                                        V[i][k] = t;
                                    }
                                }

                                if(wantu && (k < m - 1))
                                {
                                    for(int i = 0; i < m; i++)
                                    {
                                        t = U[i][k + 1];
                                        U[i][k + 1] = U[i][k];
                                        U[i][k] = t;
                                    }
                                }

                                k++;
                            }

                            iter = 0;
                            p--;
                        }

                        break;
                }
            }

            // (vermorel) transposing the results if needed
            if(transpose)
            {
                // swaping U and V
                double[][] T = V;
                V = U;
                U = T;
            }

            _u = new Matrix(U);
            _v = new Matrix(V);
            _singular = new Vector(s);

            InitOnDemandComputations();
        }

        /// <summary>Gets the one-dimensional array of singular values.</summary>
        /// <returns>diagonal of S.</returns>
        public Vector SingularValues
        {
            get { return _singular; }
        }

        /// <summary>Get the diagonal matrix of singular values.</summary>
        public Matrix S
        {
            get
            {
                // TODO: bad name for this property
                return _diagonalSingularValuesOnDemand.Compute();
            }
        }

        /// <summary>Gets the left singular vectors (U matrix).</summary>
        public Matrix LeftSingularVectors
        {
            get { return _u; }
        }

        /// <summary>Gets the right singular vectors (V matrix).</summary>
        public Matrix RightSingularVectors
        {
            get { return _v; }
        }

        /// <summary>Two norm.</summary>
        /// <returns>max(S)</returns>
        public
        double
        Norm2()
        {
            // TODO (ruegg, 2008-03-11): Change to property
            return _singular[0];
        }

        /// <summary>Two norm condition number.</summary>
        /// <returns>max(S)/min(S)</returns>
        public
        double
        Condition()
        {
            // TODO (ruegg, 2008-03-11): Change to property
            return _singular[0] / _singular[Math.Min(m, n) - 1];
        }

        /// <summary>Effective numerical matrix rank - Number of nonnegligible singular values.</summary>
        public
        int
        Rank()
        {
            // TODO (ruegg, 2008-03-11): Change to property
            return _rankOnDemand.Compute();
        }

        void
        InitOnDemandComputations()
        {
            _diagonalSingularValuesOnDemand = new OnDemandComputation<Matrix>(ComputeDiagonalSingularValues);
            _rankOnDemand = new OnDemandComputation<int>(ComputeRank);
        }

        Matrix
        ComputeDiagonalSingularValues()
        {
            return Matrix.Diagonal(_singular);
        }

        int
        ComputeRank()
        {
            double tol = Math.Max(m, n) * _singular[0] * Number.PositiveRelativeAccuracy;
            int r = 0;

            for(int i = 0; i < _singular.Length; i++)
            {
                if(_singular[i] > tol)
                {
                    r++;
                }
            }

            return r;
        }
    }
}
