using NLog;
using OpenGL_Test_Environment.GUI.content;
using OpenGL_Test_Environment.GUI.data;
using OpenGL_Test_Environment.GUI.objects;
using OpenGL_Test_Environment.GUI.shader;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace OpenGL_Test_Environment.GUI.scenes {
    class CrateScene : Scene {

        private static Logger logger = LogManager.GetCurrentClassLogger();


        private Texture2D texture;
        private Texture2D texture_spec;
        private Texture2D texture_emission;
        private Texture2D texture_grass;

        private Dictionary<string, VertexFloatBuffer> models;
        private List<LightSource> lights = new List<LightSource>();


        private Shader objectShader;
        private Shader lightShader;
        private Shader selectionShader;

        private float rotationOverTime = 0f;
        private Texture2D texture_floor;

        public void CreateScene() {
            logger.Info("Start loading scene 'crates' ...");
            logger.Info("Loading scene shaders ...");

            objectShader = new SimpleShader();
            lightShader = new LampShader();
            selectionShader = new SelectionShader();

            logger.Info("Loading scene models ...");
            models = new Dictionary<string, VertexFloatBuffer>();
            models.Add("cube", ContentManager.loadModel("cube", VertexFormat.XYZ_NORMAL_UV));
            models.Add("lamp", ContentManager.loadModel("cube", VertexFormat.XYZ));
            models.Add("quad", ContentManager.loadModel("quad", VertexFormat.XYZ_NORMAL_UV));
            //models.Add("suit", ContentManager.loadModel("nanosuit", VertexFormat.XYZ_NORMAL_UV));
            logger.Info("Loading scene textures ...");
            texture = OpenGLLoader.loadTexture("misc/crate.bmp");
            texture.setWrapping(TextureWrapMode.MirroredRepeat);
            texture_spec = OpenGLLoader.loadTexture("misc/crate_spec.bmp");
            texture_spec.setWrapping(TextureWrapMode.MirroredRepeat);
            texture_emission = OpenGLLoader.loadTexture("misc/emission.bmp");
            texture_emission.setWrapping(TextureWrapMode.MirroredRepeat);
            texture_grass = OpenGLLoader.loadTexture("misc/window.png");
            texture_grass.setWrapping(TextureWrapMode.MirroredRepeat);

            texture_floor = OpenGLLoader.loadTexture("misc/marble_floor.bmp");
            texture_floor.setWrapping(TextureWrapMode.MirroredRepeat);



            logger.Info("Adding lights to scene ...");
            addLights();
            logger.Info("Scene 'crate' created completly.");

        }

        public void DrawScene(Camera camera) {
            //combine all matrices
            //the different between GL and GLSL with matrix order
            //GL   modelview * worldview * projection;
            //GLSL projection * worldview * modelview;
            objectShader.Start();
            objectShader.LoadMatrix("viewMatrix", camera.ViewMatrix);
            objectShader.LoadMatrix("projectionMatrix", camera.ProjectionMatrix);
            objectShader.LoadUniform("cameraPosition", camera.Position);
            objectShader.Stop();

            lightShader.Start();
            lightShader.LoadMatrix("viewMatrix", camera.ViewMatrix);
            lightShader.LoadMatrix("projectionMatrix", camera.ProjectionMatrix);
            lightShader.Stop();

            selectionShader.Start();
            selectionShader.LoadMatrix("viewMatrix", camera.ViewMatrix);
            selectionShader.LoadMatrix("projectionMatrix", camera.ProjectionMatrix);
            selectionShader.Stop();

            drawFloor();
            drawLightBulbs();
            drawCrates();


            rotationOverTime += 0.01f;


        }

        private void drawFloor() {
            VertexFloatBuffer floor = models["quad"];



            objectShader.Start();
            addLightInformation(lights, objectShader);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture_floor.ID);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, texture_floor.ID);

            for (int i = 0; i < 70; i++) {
                for (int j = 0; j < 70; j++) {
                    Matrix4 translation = Matrix4.CreateTranslation(new Vector3(-35 + i, 0, -35 + j));
                    Matrix4 rotation = Matrix4.CreateFromAxisAngle(new Vector3(1, 0, 0), -1.57f);
                    Matrix4 ModelMatrix = Matrix4.Identity;
                    ModelMatrix = ModelMatrix * rotation;
                    ModelMatrix = ModelMatrix * translation;

                    objectShader.LoadMatrix("modelMatrix", ModelMatrix);

                    floor.Bind(objectShader);

                }
            }

            objectShader.Stop();
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
                    objectShader.LoadUniform("material.diffuse", 0);
                    objectShader.LoadUniform("material.specular", 1);
                    model.Bind(lightShader);
                }
            }
            lightShader.Stop();
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
                { new Vector3(0,0.5f,0), new Vector3(-2,0.5f,-2), new Vector3(-1,0.5f,-3), new Vector3(-1,0.5f,-4), new Vector3(2,0.5f,1) };
            Quaternion[] rotations =
                { new Quaternion(0,0,0), new Quaternion(-2,-2,-2), new Quaternion(-1,1,-3), new Quaternion(-1,2,-4), new Quaternion(2,-2,1) };

            VertexFloatBuffer model = models["cube"];
            for (int i = 0; i < 5; i++) {
                Matrix4 cubeTranslation = Matrix4.CreateTranslation(positions[i]);
                Matrix4 rotation = Matrix4.Identity;
                bool onfloor = true;
                if (!onfloor) {
                    if (i % 2 == 0) {
                        rotation = Matrix4.CreateFromQuaternion(rotations[i]);
                    } else {
                        rotation = Matrix4.CreateFromQuaternion(Quaternion.Multiply(rotations[i], new Quaternion(rotationOverTime, 0, rotationOverTime)));

                    }
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
            Matrix4 modelMatrix = Matrix4.CreateTranslation(new Vector3(0, 1f, 0));
            objectShader.LoadMatrix("modelMatrix", modelMatrix);
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
        private void addLights() {
            lights = new List<LightSource>();
            LightSource pointLight = new LightSource(new Vector3(0.7f, 2.2f, 2.0f), LightSource.TYPE_POINT_LIGHT);
            pointLight.Ambient = new Vector3(0.01f, 0.01f, 0.01f);
            pointLight.Diffuse = new Vector3(0.1f, 0.1f, 0.1f);
            pointLight.Specular = new Vector3(0.1f, 0.1f, 0.1f);
            pointLight.Attenuation = new Vector3(1.0f, 0.14f, 0.07f);
            lights.Add(pointLight);

            pointLight = new LightSource(new Vector3(2.3f, 3.3f, -4.0f), LightSource.TYPE_POINT_LIGHT);
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

            pointLight = new LightSource(new Vector3(0.0f, 1.0f, -3.0f), LightSource.TYPE_POINT_LIGHT);
            pointLight.Ambient = new Vector3(0.03f, 0.01f, 0.01f);
            pointLight.Diffuse = new Vector3(0.3f, 0.1f, 0.1f);
            pointLight.Specular = new Vector3(0.3f, 0.1f, 0.1f);
            pointLight.Attenuation = new Vector3(1.0f, 0.14f, 0.07f);
            lights.Add(pointLight);

            LightSource spotLight = new LightSource(new Vector3(0.0f, 1.0f, 5.0f), LightSource.TYPE_SPOT_LIGHT);
            spotLight.Direction = new Vector3(0.0f, 0.0f, -1.0f);
            spotLight.Ambient = new Vector3(0.0f, 0.0f, 0.0f);
            spotLight.Diffuse = new Vector3(1.0f, 1.0f, 1.0f);
            spotLight.Specular = new Vector3(1.0f, 1.0f, 1.0f);
            spotLight.Attenuation = new Vector3(1.0f, 0.09f, 0.032f);
            spotLight.CutOff = (float)Math.Cos(Math.PI * 10f / 180.0);
            spotLight.outerCutOff = (float)Math.Cos(Math.PI * 15f / 180.0);
            //lights.Add(spotLight);

            LightSource directionalLight = new LightSource(new Vector3(-0.2f, 1.0f, -0.3f), LightSource.TYPE_DIRECTIONAL_LIGHT);
            directionalLight.Ambient = new Vector3(0.00f, 0.00f, 0.00f);
            directionalLight.Diffuse = new Vector3(0.05f, 0.05f, 0.05f);
            directionalLight.Specular = new Vector3(0.2f, 0.2f, 0.2f);
            lights.Add(directionalLight);
        }
    }
}
