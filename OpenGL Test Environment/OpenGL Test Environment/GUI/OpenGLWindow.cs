using NLog;
using ObjLoader.Loader.Loaders;
using OpenGL_Test_Environment.GUI.content;
using OpenGL_Test_Environment.GUI.data;
using OpenGL_Test_Environment.GUI.input;
using OpenGL_Test_Environment.GUI.objects;
using OpenGL_Test_Environment.GUI.shader;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace OpenGL_Test_Environment.GUI {
    class OpenGLWindow : GameWindow {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Texture2D texture;
        private Texture2D texture_spec;
        private Texture2D texture_emission;

        private Shader objectShader;
        private Shader lightShader;
        private Dictionary<string, VertexFloatBuffer> models;
        private List<LightSource> lights = new List<LightSource>();
        private Matrix4 ProjectionMatrix;
        private Matrix4 ModelMatrix;
        private Matrix4 ViewMatrix;

        private FpsMonitor fpsm;

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
            ViewMatrix = new Matrix4();
            fpsm = new FpsMonitor();
            cameraPosition = new Vector3(0.0f, 0.0f, 5.0f);
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            objectShader = new SimpleShader();
            lightShader = new LampShader();
            models = new Dictionary<string, VertexFloatBuffer>();
            models.Add("cube", ContentManager.loadModel("cube.obj", VertexFormat.XYZ_NORMAL_UV));
            models.Add("lamp", ContentManager.loadModel("cube.obj", VertexFormat.XYZ));

            texture = OpenGLLoader.loadTexture("crate.bmp");
            texture.setWrapping(TextureWrapMode.MirroredRepeat);
            texture_spec = OpenGLLoader.loadTexture("crate_spec.bmp");
            texture_spec.setWrapping(TextureWrapMode.MirroredRepeat);
            texture_emission = OpenGLLoader.loadTexture("emission.bmp");
            texture_emission.setWrapping(TextureWrapMode.MirroredRepeat);

            lights = new List<LightSource>();
            lights.Add(new LightSource(new Vector3(0.0f, 0.0f, 5.0f), new Vector3(1f, 1f, 1f), 2f));
            lights[0].Direction = new Vector3(0, 0, -1);

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



            ViewMatrix = Matrix4.LookAt(cameraPosition, new Vector3(), new Vector3(0f, 1f, 0f));
            //combine all matrices
            //the different between GL and GLSL with matrix order
            //GL   modelview * worldview * projection;
            //GLSL projection * worldview * modelview;
            objectShader.Start();
            objectShader.LoadMatrix("viewMatrix", ViewMatrix);
            objectShader.LoadMatrix("projectionMatrix", ProjectionMatrix);
            objectShader.LoadUniform("cameraPosition", cameraPosition);



            objectShader.Stop();
            lightShader.Start();
            objectShader.LoadMatrix("viewMatrix", ViewMatrix);
            objectShader.LoadMatrix("projectionMatrix", ProjectionMatrix);
            lightShader.Stop();

        }
        private float rotationOverTime = 0f;
        private Vector3 cameraPosition;

        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.CullFace);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture.ID);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, texture_spec.ID);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, texture_emission.ID);
            objectShader.Start();
            //Vector3 newcolor = new Vector3();
            //newcolor.X = (float)Math.Sin(rotationOverTime * 2.0f);
            //newcolor.Y = (float)Math.Sin(rotationOverTime * 0.7f);
            //newcolor.Z = (float)Math.Sin(rotationOverTime * 1.3f);
            //lights[0].color = newcolor;

            objectShader.LoadUniform("light.position", new Vector4(lights[0].Position, lights[0].LightType));
            objectShader.LoadUniform("light.direction", lights[0].Direction);
            objectShader.LoadUniform("light.ambient", Vector3.Multiply(lights[0].Color, 0.2f));
            objectShader.LoadUniform("light.diffuse", Vector3.Multiply(lights[0].Color, 0.5f));
            objectShader.LoadUniform("light.specular", new Vector3(1.0f, 1.0f, 1.0f));
            objectShader.LoadUniform("light.attenuationParameter", new Vector3(1.0f, 0.09f, 0.032f));
            //objectShader.LoadUniform("light.cutOff", (float)Math.Cos((Math.PI / 180) * 12.5f));
            //objectShader.LoadUniform("light.outerCutOff", (float)Math.Cos((Math.PI / 180) * 17.5f));
            objectShader.LoadUniform("light.cutOff", 0.91f);
            objectShader.LoadUniform("light.outerCutOff", 0.82f);

            ModelMatrix = Matrix4.Identity;
            Vector3[] positions =
                { new Vector3(), new Vector3(-2,-2,-2), new Vector3(-1,5,-3), new Vector3(-1,2,-4), new Vector3(2,-2,1) };
            Quaternion[] rotations =
                { new Quaternion(0,0,0), new Quaternion(-2,-2,-2), new Quaternion(-1,1,-3), new Quaternion(-1,2,-4), new Quaternion(2,-2,1) };

            VertexFloatBuffer model = models["cube"];
            for (int i = 0; i < 5; i++) {
                Matrix4 cubeTranslation = Matrix4.CreateTranslation(positions[i]);
                Matrix4 rotation;
                if (i % 2 == 1) {
                    rotation = Matrix4.CreateFromQuaternion(rotations[i]);
                } else {
                    rotation = Matrix4.CreateFromQuaternion(Quaternion.Multiply(rotations[i], new Quaternion(rotationOverTime, 0, rotationOverTime)));

                }
                ModelMatrix = Matrix4.Identity;
                ModelMatrix = Matrix4.Mult(ModelMatrix, rotation);
                ModelMatrix = Matrix4.Mult(ModelMatrix, cubeTranslation);
                objectShader.LoadMatrix("modelMatrix", ModelMatrix);
                objectShader.LoadUniform("material.diffuse", 0);
                objectShader.LoadUniform("material.specular", 1);
                objectShader.LoadUniform("material.emission", 2);
                objectShader.LoadUniform("material.shininess", 32.0f);
                model.Bind(objectShader);
            }
            objectShader.Stop();
            lightShader.Start();
            model = models["lamp"];
            foreach (var lamp in lights) {
                Matrix4 translation = Matrix4.CreateTranslation(lamp.Position);
                Matrix4 scale = Matrix4.CreateScale(0.2f);
                ModelMatrix = Matrix4.Identity;
                ModelMatrix = Matrix4.Mult(ModelMatrix, scale);
                ModelMatrix = Matrix4.Mult(ModelMatrix, translation);
                lightShader.LoadMatrix("modelMatrix", ModelMatrix);
                //model.Bind(lightShader);
            }
            lightShader.Stop();

            rotationOverTime += 0.01f;

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
    }
}
