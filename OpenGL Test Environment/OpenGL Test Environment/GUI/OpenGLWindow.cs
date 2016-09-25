using NLog;
using ObjLoader.Loader.Loaders;
using OpenGL_Test_Environment.GUI.content;
using OpenGL_Test_Environment.GUI.data;
using OpenGL_Test_Environment.GUI.input;
using OpenGL_Test_Environment.GUI.shader;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace OpenGL_Test_Environment.GUI {
    class OpenGLWindow : GameWindow {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Texture2D texture;
        private Shader shader;
        private VertexFloatBuffer buffer;

        private Matrix4 ProjectionMatrix;
        private Matrix4 ModelMatrix;
        private Matrix4 ModelviewMatrix;

        private FpsMonitor fpsm;
        private Matrix4 MVP_Matrix;

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
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Width / (float)Height, 0.1f, 100.0f);
            ModelMatrix = new Matrix4();
            ModelviewMatrix = new Matrix4();

            fpsm = new FpsMonitor();

        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            texture = OpenGLLoader.loadTexture("wall.jpg");
            texture.setWrapping(TextureWrapMode.MirroredRepeat);

            shader = new SimpleShader();

            loadModel();

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


            ModelMatrix = Matrix4.Identity;
            ModelviewMatrix = Matrix4.LookAt(new Vector3(4.0f, 3.0f, 3.0f), new Vector3(), new Vector3(0f, 1f, 0f));
            //combine all matrices
            //the different between GL and GLSL with matrix order
            //GL   modelview * worldview * projection;
            //GLSL projection * worldview * modelview;
            MVP_Matrix = ModelviewMatrix * ModelMatrix * ProjectionMatrix;


        }

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);
            //GL.Viewport(0, 0, Width, Height);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Color.DarkGoldenrod);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, texture.ID);

            shader.Start();
            shader.loadMatrix("MVP", MVP_Matrix);
            buffer.Bind(shader);
            shader.Stop();


            GL.Disable(EnableCap.Texture2D);
            this.SwapBuffers();
            fpsm.Update();
            //fpsm.Draw();
        }

        public override void Exit() {
            base.Exit();
            ContentManager.dispose();
            OpenGLLoader.dispose();
        }




        private void loadModel() {

            buffer = new VertexFloatBuffer(VertexFormat.XYZ_UV, 32);

            float[] vertices = {
                -1.0f,-1.0f,-1.0f, // triangle 1 : begin
                -1.0f,-1.0f, 1.0f,
                -1.0f, 1.0f, 1.0f, // triangle 1 : end
                1.0f, 1.0f,-1.0f, // triangle 2 : begin
                -1.0f,-1.0f,-1.0f,
                -1.0f, 1.0f,-1.0f, // triangle 2 : end
                1.0f,-1.0f, 1.0f,
                -1.0f,-1.0f,-1.0f,
                1.0f,-1.0f,-1.0f,
                1.0f, 1.0f,-1.0f,
                1.0f,-1.0f,-1.0f,
                -1.0f,-1.0f,-1.0f,
                -1.0f,-1.0f,-1.0f,
                -1.0f, 1.0f, 1.0f,
                -1.0f, 1.0f,-1.0f,
                1.0f,-1.0f, 1.0f,
                -1.0f,-1.0f, 1.0f,
                -1.0f,-1.0f,-1.0f,
                -1.0f, 1.0f, 1.0f,
                -1.0f,-1.0f, 1.0f,
                1.0f,-1.0f, 1.0f,
                1.0f, 1.0f, 1.0f,
                1.0f,-1.0f,-1.0f,
                1.0f, 1.0f,-1.0f,
                1.0f,-1.0f,-1.0f,
                1.0f, 1.0f, 1.0f,
                1.0f,-1.0f, 1.0f,
                1.0f, 1.0f, 1.0f,
                1.0f, 1.0f,-1.0f,
                -1.0f, 1.0f,-1.0f,
                1.0f, 1.0f, 1.0f,
                -1.0f, 1.0f,-1.0f,
                -1.0f, 1.0f, 1.0f,
                1.0f, 1.0f, 1.0f,
                -1.0f, 1.0f, 1.0f,
                1.0f,-1.0f, 1.0f
            };

            float[] color = {
                0.583f,  0.771f,  0.014f,
                0.609f,  0.115f,  0.436f,
                0.327f,  0.483f,  0.844f,
                0.822f,  0.569f,  0.201f,
                0.435f,  0.602f,  0.223f,
                0.310f,  0.747f,  0.185f,
                0.597f,  0.770f,  0.761f,
                0.559f,  0.436f,  0.730f,
                0.359f,  0.583f,  0.152f,
                0.483f,  0.596f,  0.789f,
                0.559f,  0.861f,  0.639f,
                0.195f,  0.548f,  0.859f,
                0.014f,  0.184f,  0.576f,
                0.771f,  0.328f,  0.970f,
                0.406f,  0.615f,  0.116f,
                0.676f,  0.977f,  0.133f,
                0.971f,  0.572f,  0.833f,
                0.140f,  0.616f,  0.489f,
                0.997f,  0.513f,  0.064f,
                0.945f,  0.719f,  0.592f,
                0.543f,  0.021f,  0.978f,
                0.279f,  0.317f,  0.505f,
                0.167f,  0.620f,  0.077f,
                0.347f,  0.857f,  0.137f,
                0.055f,  0.953f,  0.042f,
                0.714f,  0.505f,  0.345f,
                0.783f,  0.290f,  0.734f,
                0.722f,  0.645f,  0.174f,
                0.302f,  0.455f,  0.848f,
                0.225f,  0.587f,  0.040f,
                0.517f,  0.713f,  0.338f,
                0.053f,  0.959f,  0.120f,
                0.393f,  0.621f,  0.362f,
                0.673f,  0.211f,  0.457f,
                0.820f,  0.883f,  0.371f,
                0.982f,  0.099f,  0.879f
            };

            float[] uvData = {
                0.000059f, 1.0f-0.000004f,
                0.000103f, 1.0f-0.336048f,
                0.335973f, 1.0f-0.335903f,
                1.000023f, 1.0f-0.000013f,
                0.667979f, 1.0f-0.335851f,
                0.999958f, 1.0f-0.336064f,
                0.667979f, 1.0f-0.335851f,
                0.336024f, 1.0f-0.671877f,
                0.667969f, 1.0f-0.671889f,
                1.000023f, 1.0f-0.000013f,
                0.668104f, 1.0f-0.000013f,
                0.667979f, 1.0f-0.335851f,
                0.000059f, 1.0f-0.000004f,
                0.335973f, 1.0f-0.335903f,
                0.336098f, 1.0f-0.000071f,
                0.667979f, 1.0f-0.335851f,
                0.335973f, 1.0f-0.335903f,
                0.336024f, 1.0f-0.671877f,
                1.000004f, 1.0f-0.671847f,
                0.999958f, 1.0f-0.336064f,
                0.667979f, 1.0f-0.335851f,
                0.668104f, 1.0f-0.000013f,
                0.335973f, 1.0f-0.335903f,
                0.667979f, 1.0f-0.335851f,
                0.335973f, 1.0f-0.335903f,
                0.668104f, 1.0f-0.000013f,
                0.336098f, 1.0f-0.000071f,
                0.000103f, 1.0f-0.336048f,
                0.000004f, 1.0f-0.671870f,
                0.336024f, 1.0f-0.671877f,
                0.000103f, 1.0f-0.336048f,
                0.336024f, 1.0f-0.671877f,
                0.335973f, 1.0f-0.335903f,
                0.667969f, 1.0f-0.671889f,
                1.000004f, 1.0f-0.671847f,
                0.667979f, 1.0f-0.335851f
            };

            for (int i = 0; i < 32; i++) {
                buffer.AddVertex(vertices[i * 3], vertices[i * 3 + 1], vertices[i * 3 + 2], uvData[i * 2], uvData[i * 2 + 1]);
            }
            buffer.IndexFromLength();
            buffer.Load();

            var objLoaderFactory = new ObjLoaderFactory();
            var objLoader = objLoaderFactory.Create(new MaterialNullStreamProvider());
            var fileStream = new FileStream("resources/model/cube.obj", FileMode.Open);
            var result = objLoader.Load(fileStream);
            IList<Mesh> meshes = new OBJToMeshConverter().Convert(result);

            //setup the vertex buffer [vbo]
            foreach (var mesh in meshes) {
                buffer = new VertexFloatBuffer(VertexFormat.XYZ_NORMAL_UV);

                for (int i = 0; i < mesh.Triangles.Count; i++) {
                    buffer.AddVertex(mesh.Triangles[i].X, mesh.Triangles[i].Y, mesh.Triangles[i].Z, mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z, mesh.UV[i].X, mesh.UV[i].Y);
                }

                buffer.IndexFromLength();
                buffer.Load();
            }

        }
    }
}
