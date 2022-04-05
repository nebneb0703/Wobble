using System;
using Microsoft.Xna.Framework;
using Wobble.Managers;

namespace Wobble.Graphics.Sprites.Text.Formatting
{
    public class LinkRenderer: IRenderer
    {
        public static readonly Color LinkColor = Color.Aqua;
        
        public Type FragmentType() => typeof(LinkTextFragment);

        public void PostSpriteDraw(TextFragment fragment, SpriteTextPlusLineRaw sprite)
        {
            // todo: create buttons
        }
        
        public void PreSpriteDraw(TextFragment fragment, SpriteTextPlusLineRaw sprite)
        {
            sprite.Tint = LinkColor;
        }
    }
}