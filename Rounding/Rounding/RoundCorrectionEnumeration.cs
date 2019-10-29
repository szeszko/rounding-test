using System;
using System.Collections.Generic;
using System.Text;

namespace Rounding
{
    /// <summary>
    /// List of possible rounding and error correction constants
    /// </summary>
    public enum RoundCorrectionEnumeration
    {
        /// <summary>
        /// Adding the error value to the first not 0 value
        /// </summary>
        FirstValue = 0,

        /// <summary>
        /// Adding the error value to the first fitting value
        /// </summary>
        FirstFittingValue = 1,

        /// <summary>
        /// Spread error correction over the maximum values in the list, by correcting only in steps of decimals
        /// </summary>
        MinimalParts = 2,

        /// <summary>
        /// Error correction from value to value
        /// </summary>
        Differential = 3
    }
}
