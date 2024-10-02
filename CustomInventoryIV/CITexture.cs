using System;
using System.Drawing;

namespace CustomInventoryIV
{
    /// <summary>
    /// A little class which stores a texture pointer and the texture size since.
    /// <para>This class will be removed until the ClonksCodingLib.GTAIV library has its own Texture class.</para>
    /// </summary>
    public class CITexture
    {

        public IntPtr Texture;
        public Size Size;

        public CITexture(IntPtr texture, Size size)
        {
            Texture = texture;
            Size = size;
        }

        public float GetAspectRatio()
        {
            return Size.Width / Size.Height;
        }
        public int GetWidth()
        {
            return Size.Width;
        }
        public int GetHeight()
        {
            return Size.Height;
        }

    }
}
