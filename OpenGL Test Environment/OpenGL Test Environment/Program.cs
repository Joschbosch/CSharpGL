using NLog;
using OpenGL_Test_Environment.GUI;

namespace OpenGL_Test_Environment {
    class Program {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args) {
            logger.Info("Starting main application");

            OpenGLWindow window = new OpenGLWindow(1024, 768);
            window.Run();

        }
    }
}
