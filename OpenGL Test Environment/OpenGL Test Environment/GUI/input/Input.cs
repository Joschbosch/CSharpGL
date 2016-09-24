using NLog;
using OpenTK;
using OpenTK.Input;
using System.Collections.Generic;

namespace OpenGL_Test_Environment.GUI.input {
    class Input {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static List<Key> keysDown;
        private static List<Key> keysDownLast;
        private static List<MouseButton> mouseDown;
        private static List<MouseButton> mouseDownLast;

        public static void Initialize(GameWindow window) {
            keysDown = new List<Key>();
            keysDownLast = new List<Key>();
            mouseDown = new List<MouseButton>();
            mouseDownLast = new List<MouseButton>();

            window.KeyDown += Window_KeyDown;
            window.KeyUp += Window_KeyUp;
            window.MouseDown += Window_MouseDown;
            window.MouseUp += Window_MouseUp;
            logger.Info("Initialized input manager.");
        }

        private static void Window_MouseUp(object sender, MouseButtonEventArgs e) {
            while (mouseDown.Contains(e.Button)) {
                mouseDown.Remove(e.Button);
            }
        }

        private static void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            mouseDown.Add(e.Button);
        }

        private static void Window_KeyUp(object sender, KeyboardKeyEventArgs e) {
            while (keysDown.Contains(e.Key)) {
                keysDown.Remove(e.Key);
            }
        }

        private static void Window_KeyDown(object sender, KeyboardKeyEventArgs e) {
            keysDown.Add(e.Key);
        }

        public static void Update() {
            keysDownLast = new List<Key>(keysDown);
            mouseDownLast = new List<MouseButton>(mouseDown);
        }

        public static bool KeyPress(Key key) {
            return (keysDown.Contains(key) && !keysDownLast.Contains(key));
        }

        public static bool KeyRelease(Key key) {
            return (!keysDown.Contains(key) && keysDownLast.Contains(key));
        }

        public static bool KeyDown(Key key) {
            return (keysDown.Contains(key));
        }


        public static bool ButtonPress(MouseButton button) {
            return (mouseDown.Contains(button) && !mouseDownLast.Contains(button));
        }

        public static bool ButtonRelease(MouseButton button) {
            return (!mouseDown.Contains(button) && mouseDownLast.Contains(button));
        }

        public static bool ButtonDown(MouseButton button) {
            return (mouseDown.Contains(button));
        }
    }
}
