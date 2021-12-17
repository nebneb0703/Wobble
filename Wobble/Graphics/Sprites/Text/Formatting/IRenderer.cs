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
        void ModifySprite(SpriteTextPlus parent, TextFragment fragment, SpriteTextPlusLine sprite);

        /// <summary>
        ///     Called after the sprite has been created. Use this to create additional items for this fragment type.
        /// </summary>
        void OnTextUpdate(SpriteTextPlus parent, TextFragment fragment, SpriteTextPlusLine sprite);
    }
}