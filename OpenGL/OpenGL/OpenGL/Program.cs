

using OpenTK;
using System;


namespace OpenGL {
    class MyApplication {
        public static void Main() {
            GameWindow window = new GameWindow(800, 600, new OpenTK.Graphics.GraphicsMode(32, 8, 0, 0));
            Core core = new Core(window);
            window.Run(1.0 / 60.0);

        }
    }
}