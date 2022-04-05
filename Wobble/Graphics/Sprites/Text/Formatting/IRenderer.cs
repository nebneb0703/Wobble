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
        ///     Called whenever sprite text is refreshed, and the
        ///     sprite is being (re)created. Modify the sprite based on fragment attributes here.
        /// </summary>
        void PreSpriteDraw(TextFragment fragment, SpriteTextPlusLineRaw sprite);

        /// <summary>
        ///     Called after the sprite has been created. Use this to create additional items for this fragment type.
        /// </summary>
        void PostSpriteDraw(TextFragment fragment, SpriteTextPlusLineRaw sprite);
    }
}