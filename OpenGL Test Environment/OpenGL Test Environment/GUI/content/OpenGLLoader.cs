using NLog;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace OpenGL_Test_Environment.GUI.content {
    class OpenGLLoader {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static Dictionary<string, Texture2D> loadedTextures = new Dictionary<string, Texture2D>();

        public static Texture2D loadTexture(string path) {
            if (loadedTextures.ContainsKey(path)) {
                logger.Info("Found texture \"{0}\" in cache with id {1}.", path, loadedTextures[path]);
                return loadedTextures[path];
            }
            int id = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, id);
            Bitmap bmp = ContentManager.loadImageResource("textures/" + path);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Texture2D newTexture = new Texture2D(id, data.Width, data.Height);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            newTexture.setFilter(false);
            newTexture.setWrapping(TextureWrapMode.ClampToEdge);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            bmp.UnlockBits(data);

            loadedTextures.Add(path, newTexture);
            logger.Info("Loaded texture \"{0}\" with size {1}x{2} into memory, assigned id {3}.", path, data.Width, data.Height, id);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            return newTexture;
        }

        public static void loadModel() {

        }

        public static void dispose() {
            foreach (var item in loadedTextures) {
                logger.Info("NOT IMPLEMENTED YET : Disposed texture {0} with path \"{1}\"", item.Value, item.Key);

            }
        }
    }
}

