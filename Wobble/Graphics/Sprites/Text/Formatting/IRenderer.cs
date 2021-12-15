using System;

namespace Wobble.Graphics.Sprites.Text.Formatting
{
    public interface IRenderer
    {
        /// <summary>
        ///     The type of fragment for which this type of renderer applies to.
        /// </summary>
        /// <returns></returns>
        Type FragmentType();

        /// <summary>
        ///     Called whenever sprite text is refreshed. Modify the sprite based on fragment attributes here.
        /// </summary>
        void ModifySprite(SpriteTextPlus parent, TextFragment fragment, ref SpriteTextPlusLine sprite);

        /// <summary>
        ///     Called after the sprite has been created. Use this to create additional items for this fragment type.
        /// </summary>
        void OnTextUpdate(SpriteTextPlus parent, TextFragment fragment, ref SpriteTextPlusLine sprite);
    }

    /// <summary>
    ///     Regular text renderer.
    /// </summary>
    public class PlainTextRenderer : IRenderer
    {
        public Type FragmentType() => typeof(PlainTextFragment);

        public void OnTextUpdate(SpriteTextPlus parent, TextFragment fragment, ref SpriteTextPlusLine sprite) { }

        /// <summary>
        ///     Creates default sprite for this text
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="fragment"></param>
        /// <param name="sprite"></param>
        public void ModifySprite(SpriteTextPlus parent, TextFragment fragment, ref SpriteTextPlusLine sprite)
        {
            sprite = new SpriteTextPlusLine(parent.Font, ((PlainTextFragment)fragment).DisplayText, parent.FontSize)
            {
                Parent = parent,
                Tint = parent.Tint,
                Alpha = parent.Alpha,
            };
        }
    }
}