//-----------------------------------------------------------------------
// <copyright file="GammaDistribution.cs" company="Math.NET Project">
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
// <contribution>
//    Troschuetz.Random Class Library, Stefan Trosch�tz (stefan@troschuetz.de)
// </contribution>
//-----------------------------------------------------------------------

using System;

namespace MathNet.Numerics.Distributions
{
    using RandomSources;

    /// <summary>
    /// Provides generation of gamma distributed random numbers.
    /// Often models the sum of k exponentially distributed random variables, each of which has mean theta.
    /// </summary>
    /// <remarks>
    /// The implementation of the <see cref="GammaDistribution"/> type bases upon information presented on
    ///   <a href="http://en.wikipedia.org/wiki/Gamma_distribution">Wikipedia - Gamma distribution</a>.
    /// </remarks>
    public sealed class GammaDistribution : ContinuousDistribution
    {
        double _alpha;
        double _theta;
        double _helper1;
        double _helper2;
        double _lngammaAlpha;
        double _alphaLnTheta;

        #region Construction
        /// <summary>
        /// Initializes a new instance of the GammaDistribution class,
        /// using a <see cref="SystemRandomSource"/> as underlying random number generator.
        /// </summary>
        public
        GammaDistribution()
        {
            SetDistributionParameters(1.0, 1.0);
        }

        /// <summary>
        /// Initializes a new instance of the GammaDistribution class,
        /// using the specified <see cref="RandomSource"/> as underlying random number generator.
        /// </summary>
        /// <param name="random">A <see cref="RandomSource"/> object.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="random"/> is NULL (<see langword="Nothing"/> in Visual Basic).
        /// </exception>
        public
        GammaDistribution(RandomSource random)
            : base(random)
        {
            SetDistributionParameters(1.0, 1.0);
        }

        /// <summary>
        /// Initializes a new instance of the GammaDistribution class,
        /// using a <see cref="SystemRandomSource"/> as underlying random number generator.
        /// </summary>
        public
        GammaDistribution(
            double alpha,
            double theta)
        {
            SetDistributionParameters(alpha, theta);
        }
        #endregion

        #region Distribution Parameters
        /// <summary>
        /// Gets or sets the number of summed exponentially distributed random variables.
        /// </summary>
        public double Alpha
        {
            get { return _alpha; }
            set { SetDistributionParameters(value, _theta); }
        }

        /// <summary>
        /// Gets or sets the mean of the summed exponentially distributed random variables.
        /// </summary>
        public double Theta
        {
            get { return _theta; }
            set { SetDistributionParameters(_alpha, value); }
        }

        /// <summary>
        /// Configure all distribution parameters.
        /// </summary>
        public
        void
        SetDistributionParameters(
            double alpha,
            double theta)
        {
            if(!IsValidParameterSet(alpha, theta))
            {
                throw new ArgumentException(Properties.LocalStrings.ArgumentParameterSetInvalid);
            }

            _alpha = alpha;
            _theta = theta;
            _helper1 = alpha - Math.Floor(alpha);
            _helper2 = Math.E / (Math.E + _helper1);
            _lngammaAlpha = Fn.GammaLn(alpha);
            _alphaLnTheta = alpha * Math.Log(theta);
        }

        /// <summary>
        /// Determines whether the specified parameters is valid.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if both alpha and theta are greater than 0.0; otherwise, <see langword="false"/>.
        /// </returns>
        public static
        bool
        IsValidParameterSet(
            double alpha,
            double theta)
        {
            return (alpha > 0) && (theta > 0);
        }
        #endregion

        #region Distribution Properties
        /// <summary>
        /// Gets the minimum possible value of generated random numbers.
        /// </summary>
        public override double Minimum
        {
            get { return 0.0; }
        }

        /// <summary>
        /// Gets the maximum possible value of generated random numbers.
        /// </summary>
        public override double Maximum
        {
            get { return double.MaxValue; }
        }

        /// <summary>
        /// Gets the mean value of generated random numbers.
        /// </summary>
        public override double Mean
        {
            get { return _alpha * _theta; }
        }

        /// <summary>
        /// Gets the median of generated random numbers.
        /// </summary>
        public override double Median
        {
            get { return double.NaN; }
        }

        /// <summary>
        /// Gets the variance of distributed random numbers.
        /// </summary>
        public override double Variance
        {
            get { return _alpha * _theta * _theta; }
        }

        /// <summary>
        /// Gets the skewness of distributed random numbers.
        /// </summary>
        public override double Skewness
        {
            get { return 2.0 / Math.Sqrt(_alpha); }
        }

        /// <summary>
        /// Continuous probability density function (pdf) of this probability distribution.
        /// </summary>
        public override
        double
        ProbabilityDensity(double x)
        {
            return Math.Exp(
                ((_alpha - 1) * Math.Log(x))
                - (x / _theta)
                - _lngammaAlpha
                - _alphaLnTheta);
        }

        /// <summary>
        /// Continuous cumulative distribution function (cdf) of this probability distribution.
        /// </summary>
        public override
        double
        CumulativeDistribution(double x)
        {
            return Fn.GammaRegularized(_alpha, x / _theta);
        }

        /// <summary>
        /// Continuous inverse of the cumulativ distribution function (icdf) of this probabilit distribution.
        /// </summary>
        public
        double
        InverseCumulativeDistribution(double x)
        {
            return Fn.InverseGammaRegularized(_alpha, x) * _theta;
        }
        #endregion

        #region Generator
        /// <summary>
        /// Returns a gamma distributed floating point random number.
        /// </summary>
        /// <returns>A gamma distributed double-precision floating point number.</returns>
        public override
        double
        NextDouble()
        {
            double xi, eta, gen1, gen2;
            do
            {
                gen1 = 1.0 - RandomSource.NextDouble();
                gen2 = 1.0 - RandomSource.NextDouble();
                if(gen1 <= _helper2)
                {
                    xi = Math.Pow(gen1 / _helper2, 1.0 / _helper1);
                    eta = gen2 * Math.Pow(xi, _helper1 - 1.0);
                }
                else
                {
                    xi = 1.0 - Math.Log((gen1 - _helper2) / (1.0 - _helper2));
                    eta = gen2 * Math.Pow(Math.E, -xi);
                }
            }
            while(eta > Math.Pow(xi, _helper1 - 1.0) * Math.Pow(Math.E, -xi));

            for(int i = 1; i <= _alpha; i++)
            {
                xi -= Math.Log(RandomSource.NextDouble());
            }

            return xi * _theta;
        }
        #endregion
    }
}
