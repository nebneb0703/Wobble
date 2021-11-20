using System.Collections.Generic;

namespace Wobble.Graphics.Sprites.Text.Formatting
{
    public class TextFragment
    {
        /// <summary>
        ///     The raw text of the fragnment.
        /// </summary>
        public string RawText { get; protected set; }
        
        public LinkedList<TextFragment> Inner { get; protected set; } = new LinkedList<TextFragment>();
        
        public TextFragment(string rawText)
        {
            RawText = rawText;
        }
    }

    public class PlainTextFragment : TextFragment
    {
        /// <summary>
        ///     The display text of the component.
        /// </summary>
        public string DisplayText { get; protected set; }

        public PlainTextFragment(string text) : base(text)
        {
            DisplayText = text;
        }
    }

    public class LinkTextFragment : TextFragment
    {
        /// <summary>
        ///     The URL of this link.
        /// </summary>
        public string Url;

        public LinkTextFragment(string rawText, string displayText, string url) : base(rawText)
        {
            Inner.AddFirst(new TextFragment(displayText));
            Url = url;
        }
    }
}