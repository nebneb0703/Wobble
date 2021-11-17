using MonoGame.Extended;

namespace Wobble.Graphics.Sprites.Text
{
    public class LinkInfo
    {
        /// <summary>
        ///     The URL of this link.
        /// </summary>
        public string Url;

        /// <summary>
        ///     The text which is displayed in place of the link.
        /// </summary>
        public string DisplayText;

        /// <summary>
        ///     The start index of the link in the display text.
        /// </summary>
        public int StartIndex;

        /// <summary>
        ///     The bounds of the link display text.
        /// </summary>
        public RectangleF Bounds;
    }
}