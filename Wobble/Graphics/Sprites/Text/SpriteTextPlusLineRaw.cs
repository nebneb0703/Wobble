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
        private string _text = "";

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                RefreshSize();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        public SpriteTextPlusLineRaw(WobbleFontStore font, string text, float size = 0)
        {
            Font = font;
            Text = text;

            FontSize = size == 0 ? Font.DefaultSize : size;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void DrawToSpriteBatch()
        {
            if (!Visible)
                return;

            Font.Store.Size = FontSize;
            GameBase.Game.SpriteBatch.DrawString(Font.Store, Text, AbsolutePosition, _color);
        }

        private void RefreshSize()
        {
            Font.Store.Size = FontSize;

            var (x, y) = Font.Store.MeasureString(Text);
            Size = new ScalableVector2(x, y);
        }
    }
}