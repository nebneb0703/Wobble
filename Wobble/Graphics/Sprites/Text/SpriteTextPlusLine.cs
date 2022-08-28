using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Wobble.Assets;
using Wobble.Logging;
using Wobble.Window;

namespace Wobble.Graphics.Sprites.Text
{
    public class SpriteTextPlusLine : Sprite
    {
        // /// <summary>
        // ///     The underlying text rendering component.
        // /// </summary>
        // private SpriteTextPlusLineRaw[] raw = Array.Empty<SpriteTextPlusLineRaw>();

        /// <summary>
        ///     Whether the cached texture needs to be refreshed.
        /// </summary>
        private bool _dirty;

        // todo: fix this to properly display inner text.
        
        // /// <summary>
        // ///     The text displayed for the font.
        // /// </summary>
        // public string Text
        // {
        //     get => raw;
        //     set
        //     {
        //         _raw.Text = value;
        //         SetSize();
        //         _dirty = true;
        //     }
        // }

        /// <summary>
        ///     The rendertarget used to cache the text
        /// </summary>
        private RenderTarget2D RenderTarget { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        public SpriteTextPlusLine(SpriteTextPlusLineRaw[] components = null)
        {
            SetChildrenAlpha = true;
            
            if (components != null)
                SetComponents(components);

            SetSize();

            Image = WobbleAssets.WhiteBox;
            _dirty = true;
        }

        /// <summary>
        ///     Set raw components of this line. Assume correct scaling already.
        /// </summary>
        /// <param name="components"></param>
        public void SetComponents(SpriteTextPlusLineRaw[] components)
        {
            // // Clean up old components
            // for (int i = 0; i < raw.Length; i++)
            //     raw[i].Destroy();

            for (int i = 0; i < components.Length; i++)
                components[i].Parent = this;
            
            // raw = components;
        }
        
        

        /// <summary>
        ///     Set the component size taking rounding into account.
        /// </summary>
        private void SetSize()
        {
            float width = 0, height = 0;
            for (int i = 0; i < Children.Count; i++)
            {
                var rawSprite = Children[i];
                
                // Round the size the same way it will be rounded during rendering.
                var (rawWidth, rawHeight) = rawSprite.AbsoluteSize;
                var pixelWidth = Math.Ceiling(rawWidth);
                var pixelHeight = Math.Ceiling(rawHeight);

                // Update bounds of line
                width += (float) pixelWidth;
                height = Math.Max(height, (float) pixelHeight);
            }
            
            Size = new ScalableVector2(width, height);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Update the Scale and schedules the component to be rendered into a texture if necessary.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (_dirty)
            {
                _dirty = false;
                GameBase.Game.ScheduledRenderTargetDraws.Add(() => Cache(gameTime));
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            if (RenderTarget != null && !RenderTarget.IsDisposed)
                RenderTarget.Dispose();

            Image = null;

            base.Destroy();
        }

        /// <summary>
        ///     Round the position to align with pixels exactly.
        /// </summary>
        protected override void OnRectangleRecalculated()
        {
            // Update the render rectangle.
            var x = ScreenRectangle.X;
            var y = ScreenRectangle.Y;

            if (Rotation == 0)
            {
                // Round the coordinates. Not rounding the coordinates means bad text.
                var pixelX = (int) (x * WindowManager.ScreenScale.X);
                var pixelY = (int) (y * WindowManager.ScreenScale.Y);

                x = pixelX / WindowManager.ScreenScale.X;
                y = pixelY / WindowManager.ScreenScale.Y;
            }

            // Add Width / 2 and Height / 2 to X, Y because that's what Origin is set to (in the Image setter).
            RenderRectangle = new RectangleF(x + ScreenRectangle.Width / 2f, y + ScreenRectangle.Height / 2f,
                ScreenRectangle.Width, ScreenRectangle.Height);
        }

        /// <summary>
        ///     Render the text into a texture.
        /// </summary>
        /// <param name="gameTime"></param>
        private void Cache(GameTime gameTime)
        {
            if (IsDisposed)
                return;

            try
            {
                GameBase.Game.SpriteBatch.End();
            }
            catch (Exception)
            {
                // ignored
            }
            
            int width = 0, height = 0;
            for (int i = 0; i < Children.Count; i++)
            {
                var rawSprite = Children[i];
                
                // Round the size the same way it will be rounded during rendering.
                var (rawWidth, rawHeight) = rawSprite.AbsoluteSize;
                var pixelWidth = Math.Ceiling(rawWidth);
                var pixelHeight = Math.Ceiling(rawHeight);

                // Update bounds of line
                width += (int) pixelWidth;
                height = Math.Max(height, (int) pixelHeight);
            }

            if (width == 0 || height == 0)
            {
                Visible = false;
                return;
            }

            // todo: fix this.
            
            Visible = true;

            if (RenderTarget != null && !RenderTarget.IsDisposed)
                RenderTarget?.Dispose();

            RenderTarget = new RenderTarget2D(GameBase.Game.GraphicsDevice, width, height, false,
                GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

            GameBase.Game.GraphicsDevice.SetRenderTarget(RenderTarget);
            GameBase.Game.GraphicsDevice.Clear(Color.TransparentBlack);

            // for (int i = 0; i < raw.Length; i++)
            //     raw[i].Draw(gameTime);

            // GameBase.Game.SpriteBatch.End();

            GameBase.Game.GraphicsDevice.SetRenderTarget(null);

            Image = RenderTarget;
        }
    }
}