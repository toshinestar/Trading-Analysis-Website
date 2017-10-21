﻿using StockTradingAnalysis.Interfaces.Enumerations;
using StockTradingAnalysis.Interfaces.Services;
using StockTradingAnalysis.Interfaces.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockTradingAnalysis.Services.Services
{
    /// <summary>
    /// The class InterestRateCalculatorService defines a service for interest rate calculation XIRR
    /// </summary>
    public class InterestRateCalculatorService : IInterestRateCalculatorService
    {
        /// <summary>
        /// Calculates the interest rate with bisection method p.a. and recognizes outpayments
        /// Inpayments have to be negative, outpayments or the last value of a stock have to be postive
        /// </summary>
        /// <param name="cashFlows">Cashflow</param>
        /// <returns>interest rate</returns>
        public AlgorithmResult<ApproximateResultKind, double> Calculate(IEnumerable<CashFlow> cashFlows)
        {
            return CalculateXIRR(cashFlows, 0.00000001, 50000);
        }

        internal AlgorithmResult<ApproximateResultKind, double> CalculateXIRR(IEnumerable<CashFlow> cashFlows, double tolerance, int maxIters)
        {
            var brackets = FindBrackets(CalculateXNPV, cashFlows);

            if (Math.Abs(brackets.First - brackets.Second) < tolerance)
                return new AlgorithmResult<ApproximateResultKind, double>(ApproximateResultKind.NoSolutionWithinTolerance, brackets.First);

            return Bisection(r => CalculateXNPV(cashFlows, r), brackets, tolerance, maxIters);
        }

        internal decimal CalculateXNPV(IEnumerable<CashFlow> cfs, double r)
        {

            if (r <= -1)
                r = -0.99999999; // Very funky ... Better check what an IRR <= -100% means

            return (from cf in cfs
                    let startDate = cfs.OrderBy(cf1 => cf1.Date).First().Date
                    select cf.Amount / (decimal)Math.Pow(1 + r, (cf.Date - startDate).Days / 365.0)).Sum();
        }

        internal Pair<double, double> FindBrackets(Func<IEnumerable<CashFlow>, double, decimal> func, IEnumerable<CashFlow> cfs)
        {
            const int maxIter = 100;
            const double bracketStep = 0.5;
            const double guess = 0.1;

            var leftBracket = guess - bracketStep;
            var rightBracket = guess + bracketStep;
            var iter = 0;

            while (func(cfs, leftBracket) * func(cfs, rightBracket) > 0 && iter++ < maxIter)
            {

                leftBracket -= bracketStep;
                rightBracket += bracketStep;
            }

            if (iter >= maxIter)
                return new Pair<double, double>(0, 0);

            return new Pair<double, double>(leftBracket, rightBracket);
        }

        private static AlgorithmResult<ApproximateResultKind, double> Bisection(Func<double, decimal> func, Pair<double, double> brackets, double tol, int maxIters)
        {

            var iter = 1;

            decimal f3 = 0;
            double x3 = 0;
            var x1 = brackets.First;
            var x2 = brackets.Second;

            do
            {
                var f1 = func(x1);
                var f2 = func(x2);

                if (f1 == 0 && f2 == 0)
                    return new AlgorithmResult<ApproximateResultKind, double>(ApproximateResultKind.NoSolutionWithinTolerance, x1);

                if (f1 * f2 > 0)
                    throw new ArgumentException("x1 x2 values don't bracket a root");

                x3 = (x1 + x2) / 2;
                f3 = func(x3);

                if (f3 * f1 < 0)
                    x2 = x3;
                else
                    x1 = x3;

                iter++;

            } while (Math.Abs(x1 - x2) / 2 > tol && f3 != 0 && iter < maxIters);

            if (f3 == 0)
                return new AlgorithmResult<ApproximateResultKind, double>(ApproximateResultKind.ExactSolution, x3);

            if (Math.Abs(x1 - x2) / 2 < tol)
                return new AlgorithmResult<ApproximateResultKind, double>(ApproximateResultKind.ApproximateSolution, x3);

            if (iter > maxIters)
                return new AlgorithmResult<ApproximateResultKind, double>(ApproximateResultKind.NoSolutionWithinTolerance, x3);

            throw new Exception("It should never get here");
        }

        internal struct Pair<T, Z>
        {

            public Pair(T first, Z second) { First = first; Second = second; }

            public readonly T First;

            public readonly Z Second;

        }
    }
}
