using System.Collections.Generic;
using System.Linq;

namespace Wobble.Graphics.Sprites.Text.Formatting
{
    public interface IFormatter
    {
        /// <summary>
        ///     Parses the input text into text fragments.
        /// </summary>
        /// <param name="rawText"></param>
        /// <returns>
        /// An array of text fragments which completely represent the input raw text,
        /// using <c>TextFragment</c>s and this formatter's specific output fragment.
        /// 
        /// If this formatter did not find any matches for its type, it should return null.
        /// </returns>
        TextFragment[] FormatText(string rawText);

        
        /// <summary>
        ///     Returns the type of TextFragment that this formatter returns.
        /// </summary>
        /// <returns></returns>
        System.Type FragmentType();
    }

    public static class TextFormatter
    {
        /// <summary>
        ///     Ordered of all text formatters by priority.
        /// </summary>
        public static readonly IFormatter[] TextFormatters =
        {
            new LinkFormatter(),
        };

        public static TextFragment[] Format(string rawText)
        {
            LinkedList<TextFragment> fragments = new LinkedList<TextFragment>();

            fragments.AddFirst(new TextFragment(rawText));

            FormatFragments(fragments, TextFormatters);

            return fragments.ToArray();
        }
        
        private static void FormatFragments(LinkedList<TextFragment> sourceFragments, IFormatter[] availableFormatters)
        {
            bool isComplete = true;
            var currentFragment = sourceFragments.First;

            while (!(currentFragment == null && isComplete))
            {
                // Single, unparsed fragment.
                if (currentFragment.Value.GetType() == typeof(TextFragment))
                {
                    // There still exists unparsed text.
                    isComplete = false;

                    bool wasParsed = false;

                    for (int i = 0; i < availableFormatters.Length; i++)
                    {
                        var formatter = availableFormatters[i];

                        var result = formatter.FormatText(currentFragment.Value.RawText);

                        if (result != null)
                        {
                            // Successfully formatted this text block.
                            wasParsed = true;

                            // Update fragments in list.
                            LinkedListNode<TextFragment> lastFragment = null;
                            for (int j = 0; j < result.Length; j++)
                                lastFragment = sourceFragments.AddBefore(currentFragment, result[j]);

                            // Current fragment has been replaced, remove from list.
                            sourceFragments.Remove(currentFragment);

                            currentFragment = lastFragment;

                            // Move onto the next block, stop parsing the current one.
                            break;
                        }
                    }

                    // If this text was not parsed by any parsers, convert to a PlainTextFragment.
                    if (!wasParsed)
                        currentFragment.Value = new PlainTextFragment(currentFragment.Value.RawText);
                }
                // Check nested elements
                else if (currentFragment.Value.Inner.Count > 0)
                {
                    var formatters = availableFormatters.Where(x => x.FragmentType() != currentFragment.Value.GetType()).ToArray();
                    
                    FormatFragments(currentFragment.Value.Inner, formatters);
                }

                currentFragment = currentFragment.Next;

                // If we have reached the end of the list,
                // but we are not yet complete, loop again.
                if (currentFragment == null && !isComplete)
                {
                    currentFragment = sourceFragments.First;

                    // Temporary change, will be reverted when at unparsed fragment.
                    isComplete = true;
                }
            }
        }
    }
}