using System.Text;
using System.Text.RegularExpressions;
using MonoGame.Extended;
using SpriteFontPlus;

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
        ///     The text displayed for the font. Use <see cref="RawText"/> for setting the value.
        /// </summary>
        private string _displayText = "";
        public string DisplayText
        {
            get => _displayText;
            private set
            {
                _displayText = value;
                RefreshSize();
            }
        }

        /// <summary>
        ///     Instance of regex used for parsing URL links.
        /// </summary>
        private static readonly Regex linkRegex = new Regex(@"(?:\[([\w\s\d]+)\]\((https?:\/\/[\w\d.?=#\/]+)\)|(https?:\/\/[\w\d.?=#\/]+))");

        /// <summary>
        ///     An array of URL links present in the text.
        /// </summary>
        public LinkInfo[] Links { get; private set; }
        
        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        public SpriteTextPlusLineRaw(WobbleFontStore font, string text, float size = 0)
        {
            Font = font;
            RawText = text;

            FontSize = size == 0 ? Font.DefaultSize : size;
            
            ParseRaw();
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

            Links = new LinkInfo[matches.Count];
            
            var displayTextBuilder = new StringBuilder();

            int currentIndex = 0;
            float currentX = 0;
            
            var height = Font.Store.GetLineHeight();

            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                
                // Add everything else before the match to the display text
                string previousText = RawText.Substring(currentIndex, match.Index - currentIndex);
                displayTextBuilder.Append(previousText);

                currentX += Font.Store.MeasureString(previousText).X;

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

                var width = Font.Store.MeasureString(linkText).X;
                
                Links[i] = new LinkInfo
                {
                    Url = linkUrl,
                    DisplayText = linkText,
                    StartIndex = displayTextBuilder.Length,
                    Bounds = new RectangleF(currentX, 0f, width, height)
                };
                
                displayTextBuilder.Append(linkText);
                
                currentIndex = match.Index + match.Length;
                currentX += width;
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
            
            // Update link sizes
            if (Links == null)
                return;

            int currentIndex = 0;
            float currentX = 0;
            
            var height = Font.Store.GetLineHeight();
            
            for (int i = 0; i < Links.Length; i++)
            {
                var link = Links[i];
                
                // Measure size of previous text
                string previousText = DisplayText.Substring(currentIndex, link.StartIndex - currentIndex);
                currentX += Font.Store.MeasureString(previousText).X;
                
                var width = Font.Store.MeasureString(link.DisplayText).X;

                // Update values
                link.Bounds.X = currentX;
                link.Bounds.Width = width;
                link.Bounds.Height = height;
                
                currentIndex = link.StartIndex + link.DisplayText.Length;
                currentX += width;
            }
        }
    }
}