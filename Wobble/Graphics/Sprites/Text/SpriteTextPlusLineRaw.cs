using System;
using System.Text;
using System.Text.RegularExpressions;
using SpriteFontPlus;
using Wobble.Window;

namespace Wobble.Graphics.Sprites.Text
{
    public class SpriteTextPlusLineRaw : Sprite
    {
        /// <summary>
        ///     The font to be used
        /// </summary>
        public WobbleFontStore Font { get; }

        /// <summary>
        ///     The pt. font size
        /// </summary>
        private float _fontSize;
        public float FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                RefreshSize();
            }
        }

        /// <summary>
        ///     The raw, unformatted text.
        /// </summary>
        private string _rawText = "";
        public string RawText
        {
            get => _rawText;
            set
            {
                _rawText = value;
                ParseRaw();
            }
        }
        
        /// <summary>
        ///     The text displayed for the font.
        /// </summary>
        private string _displayText = "";
        public string DisplayText
        {
            get => _displayText;
            set
            {
                _displayText = value;
                RefreshSize();
            }
        }

        /// <summary>
        ///     Instance of regex used for parsing links.
        /// </summary>
        private static readonly Regex linkRegex = new Regex(@"(?:\[([\w\s\d]+)\]\((https?:\/\/[\w\d.?=#]+)\)|(https?:\/\/[\w\d.?=#]+))");
        
        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        public SpriteTextPlusLineRaw(WobbleFontStore font, string text, float size = 0)
        {
            Font = font;
            RawText = text;
            ParseRaw();

            FontSize = size == 0 ? Font.DefaultSize : size;
        }

        /// <summary>
        ///     Parses the raw input text into correctly formatted display text.
        /// </summary>
        private void ParseRaw()
        {
            var matches = linkRegex.Matches(RawText);

            if (matches.Count == 0)
            {
                DisplayText = RawText;
                return;
            }
            
            var displayTextBuilder = new StringBuilder();

            int currentIndex = 0;

            foreach (Match match in matches)
            {
                // Add everything else before the match to the display text
                displayTextBuilder.Append(RawText.Substring(currentIndex, match.Index - currentIndex));

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
                
                // TODO: actually use linkUrl
                
                displayTextBuilder.Append(linkText);
                
                currentIndex = match.Index + match.Length;
            }
            
            // Add remainder if there is any
            if (currentIndex < RawText.Length - 1)
                displayTextBuilder.Append(RawText.Substring(currentIndex, RawText.Length - currentIndex));

            DisplayText = displayTextBuilder.ToString();
        }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void DrawToSpriteBatch()
        {
            if (!Visible)
                return;

            Font.Store.Size = FontSize;
            GameBase.Game.SpriteBatch.DrawString(Font.Store, DisplayText, AbsolutePosition, _color);
        }

        private void RefreshSize()
        {
            Font.Store.Size = FontSize;

            var (x, y) = Font.Store.MeasureString(DisplayText);
            Size = new ScalableVector2(x, y);
        }
    }
}