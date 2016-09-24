using NLog;
using OpenGL_Test_Environment.GUI.content;
using OpenGL_Test_Environment.GUI.data;
using OpenGL_Test_Environment.GUI.input;
using OpenGL_Test_Environment.GUI.shader;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing;
namespace OpenGL_Test_Environment.GUI {
    class OpenGLWindow : GameWindow {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Texture2D texture;
        private Shader shader;
        private VertexFloatBuffer buffer;

        private Matrix4 ProjectionMatrix;
        private Matrix4 WorldMatrix;
        private Matrix4 ModelviewMatrix;

        private Vector3 CameraPosition;

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
            //ProjectionMatrix = Matrix4.CreateOrthographic(MathHelper.PiOver4, Width / (float)Height, 0.5f, 10000.0f);
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Width / (float)Height, 0.5f, 10000.0f);
            WorldMatrix = new Matrix4();
            ModelviewMatrix = new Matrix4();

            CameraPosition = new Vector3(0.0f, 0.0f, -2.0f);

        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            texture = OpenGLLoader.loadTexture("wall.jpg");
            texture.setWrapping(TextureWrapMode.MirroredRepeat);
            shader = new SimpleShader();

            //setup the vertex buffer [vbo]
            buffer = new VertexFloatBuffer(VertexFormat.XYZ, 8);

            float[] vertices = new float[]  {
                -0.8f, -0.8f,  -0.8f,
                0.8f, -0.8f,  -0.8f,
                0.8f, 0.8f,  -0.8f,
                -0.8f, 0.8f,  -0.8f,
                -0.8f, -0.8f,  0.8f,
                0.8f, -0.8f,  0.8f,
                0.8f, 0.8f,  0.8f,
                -0.8f, 0.8f,  0.8f,
            };

            uint[] indices = new uint[] {// front
		       0, 7, 3,
                0, 4, 7,
                //back
                1, 2, 6,
                6, 5, 1,
                //left
                0, 2, 1,
                0, 3, 2,
                //right
                4, 5, 6,
                6, 7, 4,
                //top
                2, 3, 6,
                6, 3, 7,
                //bottom
                0, 1, 5,
                0, 5, 4 };
            buffer.DrawMode = BeginMode.Triangles;
            buffer.Set(vertices, indices);

            buffer.Load();
        }

        protected override void OnUpdateFrame(FrameEventArgs e) {
            base.OnUpdateFrame(e);
            Input.Update();
            if (Input.ButtonDown(OpenTK.Input.MouseButton.Right)) {
                if (!texture.isPixelated()) {
                    texture.setFilter(true);
                }
            } else {
                if (texture.isPixelated()) {
                    texture.setFilter(false);
                }
            }


            WorldMatrix = Matrix4.CreateTranslation(-CameraPosition);

            ModelviewMatrix = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);

            //combine all matrices
            //the different between GL and GLSL with matrix order
            //GL   modelview * worldview * projection;
            //GLSL projection * worldview * modelview;
            Matrix4 MVP_Matrix = ModelviewMatrix * WorldMatrix * ProjectionMatrix;

            //send to shader
            shader.Start();
            shader.loadMatrix("mvp_matrix", MVP_Matrix);
            shader.Stop();
        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);
            GL.Viewport(0, 0, Width, Height);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Color.DarkGoldenrod);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, texture.ID);

            shader.Start();

            buffer.Bind(shader);
            shader.Stop();




            GL.Disable(EnableCap.Texture2D);
            this.SwapBuffers();
        }

        public override void Exit() {
            base.Exit();
            ContentManager.dispose();
            OpenGLLoader.dispose();
        }
    }
}
