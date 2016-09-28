using NLog;
using OpenGL_Test_Environment.GUI.objects;
using OpenTK;
using OpenTK.Input;
using System.Collections.Generic;
using System.Drawing;

namespace OpenGL_Test_Environment.GUI.input {
    class Input {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static List<Key> keysDown;
        private static List<Key> keysDownLast;
        private static List<MouseButton> mouseDown;
        private static List<MouseButton> mouseDownLast;
        private static Vector2 lastMousePos;
        private static Vector2 mouseDelta;
        private static GameWindow window;

        public static void Initialize(GameWindow windowIn) {
            keysDown = new List<Key>();
            keysDownLast = new List<Key>();
            mouseDown = new List<MouseButton>();
            mouseDownLast = new List<MouseButton>();
            window = windowIn;

            window.KeyDown += Window_KeyDown;
            window.KeyUp += Window_KeyUp;
            window.MouseDown += Window_MouseDown;
            window.MouseUp += Window_MouseUp;
            window.CursorVisible = false;
            lastMousePos = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
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

        public static void Update(Camera camera) {
            keysDownLast = new List<Key>(keysDown);
            mouseDownLast = new List<MouseButton>(mouseDown);
            mouseDelta = new Vector2(OpenTK.Input.Mouse.GetState().X - lastMousePos.X, OpenTK.Input.Mouse.GetState().Y - lastMousePos.Y);
            lastMousePos = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
            Point center = window.PointToScreen(new Point(window.Width / 2, window.Height / 2));
            Mouse.SetPosition(center.X, center.Y);

            float dx = 0;
            float dz = 0;
            if (Input.KeyDown(OpenTK.Input.Key.W)) {
                dz = 2;
            }
            if (Input.KeyDown(OpenTK.Input.Key.S)) {
                dz = -2;
            }
            if (Input.KeyDown(OpenTK.Input.Key.A)) {
                dx = -2;
            }
            if (Input.KeyDown(OpenTK.Input.Key.D)) {
                dx = 2;
            }
            if (Input.KeyDown(OpenTK.Input.Key.Escape)) {
                window.Close();
            }
            camera.UpdatePosition(dx, dz, Input.getMouseDelta());

        }
        public static Vector2 getMouseDelta() {
            return mouseDelta;
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
