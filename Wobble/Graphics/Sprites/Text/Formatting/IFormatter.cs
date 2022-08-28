﻿namespace Wobble.Graphics.Sprites.Text.Formatting
{
    public interface IFormatter
    {
        /// <summary>
        ///     Parses the input text into text fragments.
        /// </summary>
        /// <param name="rawText"></param>
        /// <returns>
        /// An array of text fragments which completely represent the input raw text,
        /// using <c>UnparsedTextFragment</c>s and this formatter's specific output fragments.
        /// 
        /// If this formatter did not find any matches for its type, it should return null.
        /// </returns>
        TextFragment[] FormatText(string rawText);
        
        /// <summary>
        ///     Returns the TextFragment types which this formatter returns.
        /// </summary>
        /// <returns></returns>
        System.Type FragmentType();
    }
}