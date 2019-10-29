using System;
using System.Collections.Generic;
using System.Linq;

namespace Rounding
{
    public static class Round
    {
        /// <summary>
        /// Rounds the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="decimals">The decimals.</param>
        /// <returns>System.Decimal.</returns>
        public static decimal RoundValue(decimal value, int decimals)
        {
            return System.Math.Round(value, decimals, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// round each value and return the sum of all
        /// Values will be changed.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="decimals">The decimals.</param>
        /// <returns>System.Decimal.</returns>
        public static decimal RoundValues(IList<decimal> values, int decimals)
        {
            decimal sum = 0;
            for (int i = 0; i < values.Count; i++)
            {
                values[i] = RoundValue(values[i], decimals);
                sum += values[i];
            }

            return values.Sum();
        }

        /// <summary>
        /// first cumulate the values and then do a final round.
        /// Values will not be changed.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="decimals">The decimals.</param>
        /// <returns>System.Decimal.</returns>
        public static decimal SumAndRound(IEnumerable<decimal> values, int decimals)
        {
            return RoundValue(values.Sum(), decimals);
        }

        /// <summary>
        /// first round the values and then return the sum of all.
        /// Values will not be changed.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="decimals">The decimals.</param>
        /// <returns>System.Decimal.</returns>
        public static decimal RoundAndSum(IEnumerable<decimal> values, int decimals)
        {
            return values.Sum(x => RoundValue(x, decimals));
        }

        /// <summary>
        /// first calculate the sum of all, then round each value and put it back into the list.
        /// Finally do a error correction by adding the error to the first not 0 value
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="decimals">The decimals.</param>
        /// <param name="mode">The mode.</param>
        /// <returns>System.Decimal.</returns>
        public static decimal SumRoundAndCorrect(IList<decimal> values, int decimals, RoundCorrectionEnumeration mode = RoundCorrectionEnumeration.MinimalParts)
        {
            decimal sum = SumAndRound(values, decimals);
            if (mode != RoundCorrectionEnumeration.Differential)
            {
                decimal error = sum - RoundValues(values, decimals);
                if (error != 0)
                {
                    switch (mode)
                    {
                        case RoundCorrectionEnumeration.FirstValue:
                            CorrectFirstValue(values, error);
                            break;

                        case RoundCorrectionEnumeration.FirstFittingValue:
                            CorrectFirstFittingValue(values, error);
                            break;

                        case RoundCorrectionEnumeration.MinimalParts:
                            CorrectMinimalParts(values, error, decimals);
                            break;

                        case RoundCorrectionEnumeration.Differential:
                            break;

                        default:
                            break;
                    }
                }
            }
            else
            {
                CorrectDifferential(values, decimals);
            }

            return sum;
        }

        /// <summary>
        /// Adding the error value to the first not 0 value
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="error">The error.</param>
        /// <returns><c>true</c> if correction took place, <c>false</c> otherwise.</returns>
        private static bool CorrectFirstValue(IList<decimal> values, decimal error)
        {
            for (int i = 0; i < values.Count; i++)
            {
                //// find the first value <> 0
                if (values[i] != 0)
                {
                    //// add the whole error and return
                    values[i] += error;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adding the error value to the first fitting value
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="error">The error.</param>
        /// <returns><c>true</c> if correction took place, <c>false</c> otherwise.</returns>
        private static bool CorrectFirstFittingValue(IList<decimal> values, decimal error)
        {
            for (int i = 0; i < values.Count; i++)
            {
                //// find the first value having |value| > |error|
                if (System.Math.Abs(values[i]) > System.Math.Abs(error))
                {
                    values[i] += error;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Spread error correction over the maximum values in the list, by correcting only in steps of decimals
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="error">The error.</param>
        /// <param name="decimals">The decimals.</param>
        /// <returns><c>true</c> if correction took place, <c>false</c> otherwise.</returns>
        private static bool CorrectMinimalParts(IList<decimal> values, decimal error, int decimals)
        {
            bool[] isCorrected = new bool[values.Count]; //// array to flag corrected values
            decimal errorCorrection = (decimal)System.Math.Pow(10, -decimals); //// correction value for increment or decrement

            //// while there is an error left
            while (error != 0)
            {
                //// find the maximum |value| and its index within already not corrected values
                decimal maximumValue = 0;
                int activeIndex = -1;

                for (int i = 0; i < values.Count; i++)
                {
                    if (!isCorrected[i]) //// only not corrected values 
                    {
                        if (System.Math.Abs(values[i]) > maximumValue)
                        {
                            maximumValue = System.Math.Abs(values[i]);
                            activeIndex = i;
                        }
                    }
                }

                //// check if an index has been found
                if (activeIndex < 0)
                {
                    return false;
                }

                //// adjust the value in the list
                if (error > 0)
                {
                    values[activeIndex] += errorCorrection;
                    error -= errorCorrection;
                }
                else
                {
                    values[activeIndex] -= errorCorrection;
                    error += errorCorrection;
                }

                //// mark the value as corrected
                isCorrected[activeIndex] = true;
            }

            return true;
        }

        /// <summary>
        /// Adding the error value to the first fitting value
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="decimals">The decimals.</param>
        /// <returns><c>true</c> if correction took place, <c>false</c> otherwise.</returns>
        private static bool CorrectDifferential(IList<decimal> values, int decimals)
        {
            decimal error = 0;
            decimal value = 0;
            decimal threshold = (decimal)System.Math.Pow(10, -decimals);

            for (int i = 0; i < values.Count; i++)
            {
                value = values[i];
                values[i] = RoundValue(value, decimals);
                error += value - values[i];

                if (System.Math.Abs(error) >= threshold)
                {
                    switch (System.Math.Sign(error))
                    {
                        case 1:
                            values[i] += threshold;
                            error -= threshold;
                            break;
                        case -1:
                            values[i] -= threshold;
                            error += threshold;
                            break;
                    }
                }
            }

            return true;
        }
    }
}
