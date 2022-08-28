using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using SpriteFontPlus;
using Wobble.Window;

namespace Wobble.Graphics.Sprites.Text
{
    public class SpriteTextPlusLineRaw : Sprite
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
                _font = value;
                RefreshSize();
            }
        }

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
        ///     The text string to draw.
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
        ///     Current WindowManager scale.
        /// </summary>
        internal float Scale { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        public SpriteTextPlusLineRaw(WobbleFontStore font, string text, float size = 0)
        {
            Scale = GetScale();
            
            Font = font;
            Text = text;

            FontSize = size == 0 ? Font.DefaultSize : size;
        }
        
        /// <summary>
        ///     Get the current WindowManager scale and check that it's valid.
        /// </summary>
        /// <returns></returns>
        internal static float GetScale()
        {
            var scale = WindowManager.ScreenScale.X;
            Debug.Assert(scale > 0, "You're setting up text too early (WindowManager.ScreenScale.X is 0).");

            if (GameBase.Game.Graphics.PreferredBackBufferWidth < 1600)
                return scale * 2;

            return scale;
        }

        public override void Update(GameTime gameTime)
        {
            Scale = GetScale();
            
            base.Update(gameTime);
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