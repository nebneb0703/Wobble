using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SpriteFontPlus;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites.Text.Formatting;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;

namespace Wobble.Graphics.Sprites.Text
{
    public class SpriteTextPlus : Sprite
    {
        /// <summary>
        ///     The font to be used
        /// </summary>
        private WobbleFontStore _font;
        public WobbleFontStore Font
        {
            get => _font;
            set
            {
                if (value == _font)
                    return;

                _font = value;
                RefreshText();
            }
        }

        /// <summary>
        ///     The pt. font size
        /// </summary>
        private int _fontSize;
        public int FontSize
        {
            get => _fontSize;
            set
            {
                if (value == _fontSize)
                    return;

                _fontSize = value;
                RefreshText();
            }
        }

        /// <summary>
        ///     The unformatted text of this SpriteTextPlus instance.
        /// </summary>
        private string _rawText = "";
        public string RawText
        {
            get => _rawText;
            set
            {
                if (value == _rawText)
                    return;

                _rawText = value ?? "";
                RefreshText();
            }
        }

        /// <summary>
        ///     The formatted, display text to render.
        /// </summary>
        private string _text = "";
        public string Text
        {
            get => _text;
            set => RawText = value; // To not break current API
        }
        
        /// <summary>
        ///     Formatted text fragments derived from <see cref="RawText"/>.
        /// </summary>
        private LinkedList<TextFragment> Fragments { get; set; }

        /// <summary>
        ///     Text Formatter to be used for this SpriteTextPlus instance.
        /// </summary>
        private TextFormatter Formatter { get; set; } = new TextFormatter();

        /// <summary>
        ///     The alignment of the text
        /// </summary>
        private TextAlignment _textAlignment = TextAlignment.Left;
        public TextAlignment TextAlignment
        {
            get => _textAlignment;
            set
            {
                if (value == _textAlignment)
                    return;

                _textAlignment = value;
                RefreshText(); // todo: maybe update this to be more efficient?
            }
        }

        /// <summary>
        ///     The maximal width of the text; the text will be wrapped to fit.
        /// </summary>
        private float? _maxWidth = null;
        public float? MaxWidth
        {
            get => _maxWidth;
            set
            {
                if (value == _maxWidth)
                    return;

                _maxWidth = value;
                RefreshText();
            }
        }

        /// <summary>
        ///     If the text uses caching to a RenderTarget2D rather than drawing as-is.
        ///     Caching is useful for text that does not change often to increase performance and is on by default.
        ///     However, you may want to turn caching off for text that frequently changes (ex. millisecond clocks/timers)
        /// </summary>
        public bool IsCached { get; }

        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        /// <param name="cache"></param>
        public SpriteTextPlus(WobbleFontStore font, string text, int size = 0, bool cache = true, TextFormatter formatter = null)
        {
            // todo: add done flag to not refresh text so many times.
            
            if (formatter != null)
                Formatter = formatter;

            Font = font;
            RawText = text;
            IsCached = cache;
            
            FontSize = size == 0 ? Font.DefaultSize : size;
            SetChildrenAlpha = true;
        }
        /// <summary>
        ///     Format the raw text into fragments.
        /// </summary>
        private void FormatText()
        {
            Fragments = Formatter.Format(RawText);

            var builder = new StringBuilder();
            BuildText(builder, Fragments);

            _text = builder.ToString();
        }
        
        private void BuildText(StringBuilder builder, IEnumerable<TextFragment> fragments)
        {
            foreach(TextFragment fragment in fragments)
            {
                if (fragment is PlainTextFragment text)
                    builder.Append(text.DisplayText);
                else if(fragment.Inner.Count > 0)
                    BuildText(builder, fragment.Inner);
            }
        }
        
        /// <summary>
        /// </summary>
        private void RefreshText()
        {
            FormatText();
            
            for (var i = Children.Count - 1; i >= 0; i--)
                Children[i].Destroy();
            
            Formatter.CreateSprites(this, Fragments);
        }
        
        /// <summary>
        ///     Truncates the text with an elipsis according to <see cref="maxWidth"/>
        /// </summary>
        /// <param name="maxWidth"></param>
        public void TruncateWithEllipsis(int maxWidth)
        {
            // todo: uhhhhhhhhh
            return;
            
            var originalText = RawText;

            // Multi-line (MaxWidth) + Ellipis truncation
            if (Children.Count > 1 && Children.All(x => x is SpriteTextPlusLine))
            {
                var text = RawText;

                Font.Store.Size = FontSize;
                var totalWidth = Font.Store.MeasureString(text).X;

                while (totalWidth > maxWidth)
                {
                    text = text.Substring(0, text.Length - 1);

                    Font.Store.Size = FontSize;
                    totalWidth = Font.Store.MeasureString(text).X;
                }

                RawText = text;
            }
            // Single line truncation
            else
            {
                while (Width > maxWidth)
                    RawText = RawText.Substring(0, RawText.Length - 1);
            }

            if (RawText != originalText)
                RawText += "...";
        }

        public override void DrawToSpriteBatch()
        {
            // Do not draw anything here, children will draw instead.
            // todo: override Draw as well??
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal Alignment ConvertTextAlignment()
        {
            switch (TextAlignment)
            {
                case TextAlignment.Left:
                    return Alignment.TopLeft;
                case TextAlignment.Center:
                    return Alignment.TopCenter;
                case TextAlignment.Right:
                    return Alignment.TopRight;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}