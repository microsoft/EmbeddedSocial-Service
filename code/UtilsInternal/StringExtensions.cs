// <copyright file="StringExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System;
    using System.Text;

    /// <summary>
    /// This class implements extensions to the String class
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Takes a string and changes the case of the first letter found in the string. Returns the new string.
        /// If no letters are found (e.g., the string is "++++"), no changes to the input string are made.
        /// </summary>
        /// <param name="input">input string</param>
        /// <returns>the new string with the case of the first letter changed</returns>
        public static string ChangeCaseOfFirstLetter(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            StringBuilder output = new StringBuilder(input);

            // Find the first letter in the string (from left-to-right) and change its case
            for (int i = 0; i < input.Length; i += 1)
            {
                // if the character is a lower case, change it to an upper case and return
                if (output[i] >= 'a' && output[i] <= 'z')
                {
                    output[i] = char.ToUpper(output[i]);
                    break;
                }
                else if (output[i] >= 'A' && output[i] <= 'Z')
                {
                    output[i] = char.ToLower(output[i]);
                    break;
                }
            }

            return output.ToString();
        }

        /// <summary>
        /// Checks if a string is null or empty. In that case, it throws an ArgumentNullException
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="errorMessage">exception's error message</param>
        public static void IfNullOrEmptyThrowEx(this string input, string errorMessage)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException(errorMessage);
            }
        }
    }
}
