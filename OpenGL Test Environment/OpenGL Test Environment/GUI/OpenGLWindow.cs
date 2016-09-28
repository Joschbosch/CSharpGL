using NLog;
using OpenGL_Test_Environment.GUI.content;
using OpenGL_Test_Environment.GUI.data;
using OpenGL_Test_Environment.GUI.input;
using OpenGL_Test_Environment.GUI.objects;
using OpenGL_Test_Environment.GUI.scenes;
using OpenGL_Test_Environment.GUI.shader;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing;

namespace OpenGL_Test_Environment.GUI {
    class OpenGLWindow : GameWindow {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private FpsMonitor fpsm;
        private Camera camera;

        private FrameBufferObject renderFrameBuffer;
        private ScreenShader screenShader;
        private int quadVAO;
        private FrameBufferObject resolvingFBO;

        private Scene scene;

        public OpenGLWindow(int width, int height) : base(width, height, GraphicsMode.Default, "Title",
            GameWindowFlags.FixedWindow, DisplayDevice.Default, 4, 0, OpenTK.Graphics.GraphicsContextFlags.ForwardCompatible) {
            #region GL_VERSION
            //this will return your version of opengl
            int major, minor;
            GL.GetInteger(GetPName.MajorVersion, out major);
            GL.GetInteger(GetPName.MinorVersion, out minor);
            logger.Info("Creating OpenGL version {2}.{3} window with size {0} x {1}", width, height, major, minor);
            #endregion
            Input.Initialize(this);

            // Move to ?
            fpsm = new FpsMonitor();
            camera = new Camera(new Vector3(0.0f, 0.0f, 6.0f), new Quaternion(0, 0, 0), Width, Height);
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            scene = new CrateScene();
            scene.CreateScene();

            logger.Info("Loading screen shader and FBOs...");
            screenShader = new ScreenShader();
            createScreenQuad();
            renderFrameBuffer = new FrameBufferObject(Width, Height, 4);
            resolvingFBO = new FrameBufferObject(Width, Height);

            logger.Info("Loading complete!");
        }

        protected override void OnUpdateFrame(FrameEventArgs e) {
            base.OnUpdateFrame(e);
            Input.Update(camera);

        }


        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            renderFrameBuffer.bindFBO();
            GL.ClearColor(Color.Olive);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            scene.DrawScene(camera);

            drawScreen();

            this.SwapBuffers();
            fpsm.Update();

        }

        private void drawScreen() {

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, renderFrameBuffer.id_fbo);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, resolvingFBO.id_fbo);
            GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);

            FrameBufferObject.bindDefaultFBO(Width, Height);
            GL.ClearColor(Color.DarkRed);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Disable(EnableCap.DepthTest);
            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            screenShader.Start();
            GL.BindVertexArray(quadVAO);
            GL.BindTexture(TextureTarget.Texture2D, resolvingFBO.colorTexture);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0);
            screenShader.Stop();
            GL.Enable(EnableCap.DepthTest);
            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }



        public override void Exit() {
            base.Exit();
            ContentManager.dispose();
            OpenGLLoader.dispose();
        }


        private void createScreenQuad() {

            float[] quadVertices = {   // Vertex attributes for a quad that fills the entire screen in Normalized Device Coordinates.
                -1.0f,  1.0f,  0.0f, 1.0f,
                -1.0f, -1.0f,  0.0f, 0.0f,
                 1.0f, -1.0f,  1.0f, 0.0f,

                -1.0f,  1.0f,  0.0f, 1.0f,
                 1.0f, -1.0f,  1.0f, 0.0f,
                 1.0f,  1.0f,  1.0f, 1.0f
            };
            quadVAO = GL.GenVertexArray();
            int quadVBO = GL.GenBuffer();
            GL.BindVertexArray(quadVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, quadVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(quadVertices.Length * sizeof(float)), quadVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.BindVertexArray(0);

        }

    }
}
