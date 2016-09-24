using NLog;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace OpenGL_Test_Environment.GUI.content {
    class ContentManager {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static Dictionary<string, Bitmap> loadedImages = new Dictionary<string, Bitmap>();

        public static Bitmap loadImageResource(string path) {
            if (!File.Exists("resources/" + path)) {
                throw new FileNotFoundException("Resource does not exist: resources/" + path);
            }

            if (loadedImages.ContainsKey(path)) {
                logger.Info("Found image resource \"{0}\" in cache.", path);
                return loadedImages[path];
            }
            Bitmap newBmp = new Bitmap("resources/" + path);
            loadedImages.Add(path, newBmp);
            logger.Info("Loaded image resource \"{0}\" and added it to image cache.", path);
            return newBmp;


        }

        public static void disposeImageResource(string path) {
            if (loadedImages.ContainsKey(path)) {
                loadedImages[path].Dispose();
                loadedImages.Remove(path);
                logger.Info("Disposed image resource \"{0}\" from cache.", path);
            }
        }

        public static void dispose() {
            foreach (var item in loadedImages) {
                disposeImageResource(item.Key);
            }
        }
    }
}
