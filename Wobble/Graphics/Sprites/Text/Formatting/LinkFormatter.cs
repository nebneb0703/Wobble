using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Wobble.Graphics.Sprites.Text.Formatting
{
    public class LinkFormatter : IFormatter
    {
        private static readonly Regex linkRegex = new Regex(@"(?:\[([\w\s\d]+)\]\(((?:https?|quaver)?:\/\/[\w\d:#@%\/;$~_?\+-=\\\.&]+)\)|((?:https?|quaver)?:\/\/[\w\d:#@%\/;$~_?\+-=\\\.&]+))");

        public System.Type FragmentType() => typeof(LinkTextFragment);
        
        public TextFragment[] FormatText(string rawText)
        {
            var matches = linkRegex.Matches(rawText);

            if (matches.Count == 0)
                return null;

            var fragments = new List<TextFragment>();

            int currentIndex = 0;

            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                
                // Add everything else before the match to the display text
                string previousText = rawText.Substring(currentIndex, match.Index - currentIndex);
                fragments.Add(new TextFragment(previousText));

                // Text from markdown
                string linkText = match.Groups[1].Value;
                // Link from markdown
                string linkUrl = match.Groups[2].Value;

                // Occurs when the pattern is only a link, not a markdown link.
                if (string.IsNullOrEmpty(linkUrl))
                {
                    linkText = match.Groups[3].Value;
                    linkUrl = linkText;
                }

                var link = new LinkTextFragment(match.Value, linkText, linkUrl);
                fragments.Add(link);
                
                currentIndex = match.Index + match.Length;
            }
            
            // Add remainder if there is any
            if (currentIndex < rawText.Length - 1)
            {
                var remainderText = rawText.Substring(currentIndex, rawText.Length - currentIndex);
                fragments.Add(new TextFragment(remainderText));
            }

            return fragments.ToArray();
        }
    }
}