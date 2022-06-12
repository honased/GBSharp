using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GBTK
{
    public class Texture
    {
        public readonly int Handle;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public static Texture CreateFromRGBA(byte[] pixels, int width, int height)
        {
            int handle = GL.GenTexture();

            // Bind the handle
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);
            GL.Enable(EnableCap.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexImage2D(TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba,
                    width,
                    height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    pixels);

            return new Texture(handle) { Width = width, Height = height };
        }

        public Texture(int glHandle)
        {
            Handle = glHandle;
        }

        public void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        public void UpdateTexture(byte[] pixels)
        {
            Debug.Assert(pixels.Length == Width * Height * 4, "Incorrect size given for pixels array");

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Handle);

            GL.TexSubImage2D(TextureTarget.Texture2D,
                    0,
                    0,
                    0,
                    Width,
                    Height,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    pixels);
        }
    }
}
