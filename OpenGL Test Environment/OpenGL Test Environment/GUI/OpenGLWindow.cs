using NLog;
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

namespace OpenGL_Test_Environment.GUI {
    class OpenGLWindow : GameWindow {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Texture2D texture;
        private Texture2D texture_spec;
        private Texture2D texture_emission;

        private Shader objectShader;
        private Shader lightShader;
        private Shader selectionShader;

        private Dictionary<string, VertexFloatBuffer> models;
        private List<LightSource> lights = new List<LightSource>();
        private Matrix4 ProjectionMatrix;
        private Matrix4 ViewMatrix;

        private FpsMonitor fpsm;
        private float rotationOverTime = 0f;
        private Vector3 cameraPosition;
        private Texture2D texture_grass;
        private FrameBufferObject renderFrameBuffer;
        private ScreenShader screenShader;
        private int quadVAO;
        private FrameBufferObject resolvingFBO;

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
            ViewMatrix = new Matrix4();
            fpsm = new FpsMonitor();
            cameraPosition = new Vector3(0.0f, 0.0f, 6.0f);
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            objectShader = new SimpleShader();
            lightShader = new LampShader();
            selectionShader = new SelectionShader();
            screenShader = new ScreenShader();


            models = new Dictionary<string, VertexFloatBuffer>();
            models.Add("cube", ContentManager.loadModel("cube", VertexFormat.XYZ_NORMAL_UV));
            models.Add("lamp", ContentManager.loadModel("cube", VertexFormat.XYZ));
            models.Add("quad", ContentManager.loadModel("quad", VertexFormat.XYZ_NORMAL_UV));
            //models.Add("suit", ContentManager.loadModel("nanosuit", VertexFormat.XYZ_NORMAL_UV));


            createScreenQuad();

            texture = OpenGLLoader.loadTexture("misc/crate.bmp");
            texture.setWrapping(TextureWrapMode.MirroredRepeat);
            texture_spec = OpenGLLoader.loadTexture("misc/crate_spec.bmp");
            texture_spec.setWrapping(TextureWrapMode.MirroredRepeat);
            texture_emission = OpenGLLoader.loadTexture("misc/emission.bmp");
            texture_emission.setWrapping(TextureWrapMode.MirroredRepeat);
            texture_grass = OpenGLLoader.loadTexture("misc/window.png");
            texture_grass.setWrapping(TextureWrapMode.MirroredRepeat);
            addLights();

            renderFrameBuffer = new FrameBufferObject(Width, Height, 4);
            resolvingFBO = new FrameBufferObject(Width, Height);


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
            lightShader.LoadMatrix("viewMatrix", ViewMatrix);
            lightShader.LoadMatrix("projectionMatrix", ProjectionMatrix);
            lightShader.Stop();

            selectionShader.Start();
            selectionShader.LoadMatrix("viewMatrix", ViewMatrix);
            selectionShader.LoadMatrix("projectionMatrix", ProjectionMatrix);
            selectionShader.Stop();

        }


        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            renderFrameBuffer.bindFBO();
            GL.ClearColor(Color.Olive);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            drawCrates();
            drawLightBulbs();

            rotationOverTime += 0.01f;

            drawScreen();


            this.SwapBuffers();
            fpsm.Update();
            //fpsm.Draw();
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

        private void drawLightBulbs() {
            lightShader.Start();
            VertexFloatBuffer model = models["lamp"];
            foreach (var lamp in lights) {
                if (lamp.LightType == LightSource.TYPE_POINT_LIGHT) {
                    Matrix4 translation = Matrix4.CreateTranslation(lamp.Position);
                    Matrix4 scale = Matrix4.CreateScale(0.2f);
                    Matrix4 ModelMatrix = Matrix4.Identity;
                    ModelMatrix = Matrix4.Mult(ModelMatrix, scale);
                    ModelMatrix = Matrix4.Mult(ModelMatrix, translation);
                    lightShader.LoadMatrix("modelMatrix", ModelMatrix);
                    model.Bind(lightShader);
                }
            }

        }


        private void drawCrates() {
            GL.Enable(EnableCap.DepthTest);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture.ID);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, texture_spec.ID);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, texture_emission.ID);
            GL.StencilFunc(StencilFunction.Always, 1, 0xff);
            GL.StencilMask(0xff);
            objectShader.Start();
            addLightInformation(lights, objectShader);

            Matrix4 ModelMatrix = Matrix4.Identity;
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

            model = models["quad"];
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture_grass.ID);
            objectShader.LoadMatrix("modelMatrix", Matrix4.Identity);
            objectShader.LoadUniform("material.diffuse", 0);
            objectShader.LoadUniform("material.specular", 0);
            objectShader.LoadUniform("material.emission", 0);
            objectShader.LoadUniform("material.shininess", 0);
            model.Bind(objectShader);

            objectShader.Stop();

            //GL.StencilFunc(StencilFunction.Notequal, 1, 0xff);
            //GL.StencilMask(0x00);
            //GL.Disable(EnableCap.DepthTest);
            //selectionShader.Start();
            //for (int i = 0; i < 5; i++) {
            //    Matrix4 cubeTranslation = Matrix4.CreateTranslation(positions[i]);
            //    Matrix4 rotation;
            //    if (i % 2 == 1) {
            //        rotation = Matrix4.CreateFromQuaternion(rotations[i]);
            //    } else {
            //        rotation = Matrix4.CreateFromQuaternion(Quaternion.Multiply(rotations[i], new Quaternion(rotationOverTime, 0, rotationOverTime)));

            //    }
            //    Matrix4 cubeScale = Matrix4.CreateScale(1.5f);
            //    ModelMatrix = Matrix4.Identity;
            //    ModelMatrix = Matrix4.Mult(ModelMatrix, cubeScale);
            //    ModelMatrix = Matrix4.Mult(ModelMatrix, rotation);
            //    ModelMatrix = Matrix4.Mult(ModelMatrix, cubeTranslation);
            //    selectionShader.LoadMatrix("modelMatrix", ModelMatrix);
            //    model.Bind(selectionShader);
            //}
            //selectionShader.Stop();
            //GL.StencilMask(0xff);
            //GL.Enable(EnableCap.DepthTest);


        }

        private void addLightInformation(List<LightSource> lights, Shader objectShader) {

            int spotCount = 0;
            int directionalCount = 0;
            int pointCount = 0;
            foreach (var light in lights) {
                if (light.LightType == LightSource.TYPE_POINT_LIGHT) {
                    String pointString = "pointLights[" + pointCount + "]";
                    objectShader.LoadUniform(pointString + ".position", light.Position);
                    objectShader.LoadUniform(pointString + ".ambient", light.Ambient);
                    objectShader.LoadUniform(pointString + ".diffuse", light.Diffuse);
                    objectShader.LoadUniform(pointString + ".specular", light.Specular);
                    objectShader.LoadUniform(pointString + ".attenuation", light.Attenuation);
                    pointCount++;
                } else if (light.LightType == LightSource.TYPE_DIRECTIONAL_LIGHT) {
                    String directionalString = "dirLights[" + directionalCount + "]";
                    objectShader.LoadUniform(directionalString + ".direction", light.Direction);
                    objectShader.LoadUniform(directionalString + ".ambient", light.Ambient);
                    objectShader.LoadUniform(directionalString + ".diffuse", light.Diffuse);
                    objectShader.LoadUniform(directionalString + ".specular", light.Specular);
                    directionalCount++;
                } else if (light.LightType == LightSource.TYPE_SPOT_LIGHT) {
                    String spotString = "spotLights[" + spotCount + "]";
                    objectShader.LoadUniform(spotString + ".position", light.Position);
                    objectShader.LoadUniform(spotString + ".direction", light.Direction);
                    objectShader.LoadUniform(spotString + ".ambient", light.Ambient);
                    objectShader.LoadUniform(spotString + ".diffuse", light.Diffuse);
                    objectShader.LoadUniform(spotString + ".specular", light.Specular);
                    objectShader.LoadUniform(spotString + ".attenuation", light.Attenuation);
                    objectShader.LoadUniform(spotString + ".cutOff", light.CutOff);
                    objectShader.LoadUniform(spotString + ".outerCutOff", light.outerCutOff);
                    spotCount++;
                }
            }

            objectShader.LoadUniform("pointCount", pointCount);
            objectShader.LoadUniform("directionalCount", directionalCount);
            objectShader.LoadUniform("spotCount", spotCount);


        }
        private void addLights() {
            lights = new List<LightSource>();
            LightSource pointLight = new LightSource(new Vector3(0.7f, 0.2f, 2.0f), LightSource.TYPE_POINT_LIGHT);
            pointLight.Ambient = new Vector3(0.01f, 0.01f, 0.01f);
            pointLight.Diffuse = new Vector3(0.1f, 0.1f, 0.1f);
            pointLight.Specular = new Vector3(0.1f, 0.1f, 0.1f);
            pointLight.Attenuation = new Vector3(1.0f, 0.14f, 0.07f);
            lights.Add(pointLight);

            pointLight = new LightSource(new Vector3(2.3f, -3.3f, -4.0f), LightSource.TYPE_POINT_LIGHT);
            pointLight.Ambient = new Vector3(0.01f, 0.01f, 0.01f);
            pointLight.Diffuse = new Vector3(0.1f, 0.1f, 0.1f);
            pointLight.Specular = new Vector3(0.1f, 0.1f, 0.1f);
            pointLight.Attenuation = new Vector3(1.0f, 0.14f, 0.07f);
            lights.Add(pointLight);

            pointLight = new LightSource(new Vector3(-4.0f, 2.0f, -12.0f), LightSource.TYPE_POINT_LIGHT);
            pointLight.Ambient = new Vector3(0.01f, 0.01f, 0.01f);
            pointLight.Diffuse = new Vector3(0.1f, 0.1f, 0.1f);
            pointLight.Specular = new Vector3(0.1f, 0.1f, 0.1f);
            pointLight.Attenuation = new Vector3(1.0f, 0.22f, 0.020f);
            lights.Add(pointLight);

            pointLight = new LightSource(new Vector3(0.0f, 0.0f, -3.0f), LightSource.TYPE_POINT_LIGHT);
            pointLight.Ambient = new Vector3(0.03f, 0.01f, 0.01f);
            pointLight.Diffuse = new Vector3(0.3f, 0.1f, 0.1f);
            pointLight.Specular = new Vector3(0.3f, 0.1f, 0.1f);
            pointLight.Attenuation = new Vector3(1.0f, 0.14f, 0.07f);
            lights.Add(pointLight);

            LightSource spotLight = new LightSource(new Vector3(0.0f, 0.0f, 5.0f), LightSource.TYPE_SPOT_LIGHT);
            spotLight.Direction = new Vector3(0.0f, 0.0f, -1.0f);
            spotLight.Ambient = new Vector3(0.0f, 0.0f, 0.0f);
            spotLight.Diffuse = new Vector3(1.0f, 1.0f, 1.0f);
            spotLight.Specular = new Vector3(1.0f, 1.0f, 1.0f);
            spotLight.Attenuation = new Vector3(1.0f, 0.09f, 0.032f);
            spotLight.CutOff = (float)Math.Cos(Math.PI * 10f / 180.0);
            spotLight.outerCutOff = (float)Math.Cos(Math.PI * 15f / 180.0);
            //lights.Add(spotLight);

            LightSource directionalLight = new LightSource(new Vector3(-0.2f, -1.0f, -0.3f), LightSource.TYPE_DIRECTIONAL_LIGHT);
            directionalLight.Ambient = new Vector3(0.00f, 0.00f, 0.00f);
            directionalLight.Diffuse = new Vector3(0.05f, 0.05f, 0.05f);
            directionalLight.Specular = new Vector3(0.2f, 0.2f, 0.2f);
            lights.Add(directionalLight);
        }


        public override void Exit() {
            base.Exit();
            ContentManager.dispose();
            OpenGLLoader.dispose();
        }
    }
}
