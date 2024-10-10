using System;
using System.Drawing;

namespace CustomInventoryIV
{
    /// <summary>
    /// A little class which stores a texture pointer and the texture size since.
    /// <para>This class will be removed until the ClonksCodingLib.GTAIV (or IV-SDK .NET) library has its own Texture class.</para>
    /// </summary>
    public class CITexture
    {

        private IntPtr texture;
        private Size size;

        public CITexture(IntPtr texturePtr, Size textureSize)
        {
            texture = texturePtr;
            size = textureSize;
        }

        public IntPtr GetTexture()
        {
            return texture;
        }

        public float GetAspectRatio()
        {
            return size.Width / size.Height;
        }
        public int GetWidth()
        {
            return size.Width;
        }
        public int GetHeight()
        {
            return size.Height;
        }

    }
}
