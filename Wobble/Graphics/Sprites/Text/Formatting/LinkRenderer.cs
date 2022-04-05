using System;
using Microsoft.Xna.Framework;

namespace Wobble.Graphics.Sprites.Text.Formatting
{
    public class LinkRenderer: IRenderer
    {
        public static readonly Color LinkColor = Color.Aqua;
        
        public Type FragmentType() => typeof(LinkTextFragment);

        public void OnTextUpdate(TextFragment fragment, SpriteTextPlusLineRaw sprite)
        {
            // todo: create buttons
        }
        
        public void ModifySprite(TextFragment fragment, SpriteTextPlusLineRaw sprite)
        {
            sprite.Tint = LinkColor;
        }
    }
}