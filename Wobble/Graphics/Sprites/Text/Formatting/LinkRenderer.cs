using System;
using Microsoft.Xna.Framework;

namespace Wobble.Graphics.Sprites.Text.Formatting
{
    public class LinkRenderer: IRenderer
    {
        public static readonly Color LinkColor = Color.Aqua;
        
        public Type FragmentType() => typeof(LinkTextFragment);

        public void OnTextUpdate(SpriteTextPlus parent, TextFragment fragment, SpriteTextPlusLine sprite)
        {
            // todo: create buttons
        }
        
        public void ModifySprite(SpriteTextPlus parent, TextFragment fragment, SpriteTextPlusLine sprite)
        {
            sprite.Tint = LinkColor;
        }
    }
}