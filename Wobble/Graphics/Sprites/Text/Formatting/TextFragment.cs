using System;
using System.Collections.Generic;

namespace Wobble.Graphics.Sprites.Text.Formatting
{
    public abstract class TextFragment : ICloneable
    {
        public LinkedList<TextFragment> Inner { get; internal set; } = new LinkedList<TextFragment>();
        
        public object Clone() =>  MemberwiseClone();
    }

    public class UnparsedTextFragment : TextFragment
    {
        /// <summary>
        ///     The raw unparsed text of the component.
        /// </summary>
        public string RawText { get; internal set; }

        public UnparsedTextFragment(string text)
        {
            RawText = text;
        }
    }

    public class PlainTextFragment : TextFragment
    {
        /// <summary>
        ///     The display text of the component.
        /// </summary>
        public string DisplayText { get; internal set; }

        public PlainTextFragment(string text)
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

        public LinkTextFragment(string displayText, string url)
        {
            Inner.AddFirst(new PlainTextFragment(displayText));
            Url = url;
        }
    }
}