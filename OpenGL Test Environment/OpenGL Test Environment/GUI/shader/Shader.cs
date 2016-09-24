using NLog;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenGL_Test_Environment.GUI.shader {
    class Shader {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public string VertexSourceLocation { get; private set; }
        public string GeometrySourceLocation { get; private set; }
        public string FragmentSourceLocation { get; private set; }
        public int VertexID { get; private set; }
        public int GeometryID { get; private set; }
        public int FragmentID { get; private set; }
        public string BaseDir { get; private set; }

        public int Program { get; private set; }

        private Dictionary<string, int> attributes;

        public int PositionLocation { get; set; }
        public int NormalLocation { get; set; }
        public int TexCoordLocation { get; set; }
        public int ColorLocation { get; set; }


        public Shader(string baseDir) {
            this.BaseDir = baseDir;
            VertexSourceLocation = baseDir + "/vertex.glsl";
            GeometrySourceLocation = baseDir + "/geometry.glsl";
            FragmentSourceLocation = baseDir + "/fragment.glsl";
            attributes = new Dictionary<string, int>();
            Build();
        }


        private void Build() {


            VertexID = GL.CreateShader(ShaderType.VertexShader);
            if (File.Exists(GeometrySourceLocation)) {
                GeometryID = GL.CreateShader(ShaderType.FragmentShader);
            } else {
                GeometryID = -1;
            }
            FragmentID = GL.CreateShader(ShaderType.FragmentShader);

            compileShader(VertexID, VertexSourceLocation);
            if (GeometryID != -1) {
                compileShader(VertexID, VertexSourceLocation);
            }
            compileShader(FragmentID, FragmentSourceLocation);

            Program = GL.CreateProgram();
            GL.AttachShader(Program, VertexID);
            if (GeometryID != -1) {
                GL.AttachShader(Program, GeometryID);
            }
            GL.AttachShader(Program, FragmentID);

            GL.LinkProgram(Program);

        }

        public void bindAttribute(string attribute) {
            Start();
            int location = GL.GetAttribLocation(Program, attribute);
            if (location >= 0)
                GL.BindAttribLocation(Program, location, attribute);
            attributes.Add(attribute, location);

            Stop();

        }

        private void compileShader(int id, string location) {
            int status;
            string info;
            GL.ShaderSource(id, LoadSource(location));
            GL.CompileShader(id);
            GL.GetShaderInfoLog(id, out info);
            GL.GetShader(id, ShaderParameter.CompileStatus, out status);

            if (status != 1) {
                logger.Error("Could not compile shader: {0}", info);
                throw new ApplicationException(info);
            }
            logger.Info("Compiled shader {0} with id {1}", location, id);

        }

        private string LoadSource(string sourceLocation) {
            return System.IO.File.ReadAllText(@"resources/shader/" + sourceLocation);
        }


        public void loadUniform(string attribute, float value) {
            GL.Uniform1(getUniformLocation(attribute), value);
        }
        public void loadUniform(string attribute, int value) {
            GL.Uniform1(getUniformLocation(attribute), value);
        }
        public void loadUniform(string attribute, Vector2 value) {
            GL.Uniform2(getUniformLocation(attribute), value);
        }
        public void loadUniform(string attribute, Vector3 value) {
            GL.Uniform3(getUniformLocation(attribute), value);
        }
        public void loadUniform(string attribute, Vector4 value) {
            GL.Uniform4(getUniformLocation(attribute), value);
        }
        public void loadUniform(string attribute, bool value) {
            float toLoad = 0.0f;
            if (value) {
                toLoad = 1.0f;
            }
            loadUniform(attribute, toLoad);
        }

        public void loadMatrix(string attribute, Matrix4 value) {
            GL.UniformMatrix4(getUniformLocation(attribute), false, ref value);
        }

        public void loadMatrix(string attribute, Matrix3 value) {
            GL.UniformMatrix3(getUniformLocation(attribute), false, ref value);
        }
        public int getUniformLocation(string attribute) {
            return GL.GetUniformLocation(Program, attribute);
        }

        public void Dispose() {
            if (Program != 0) {
                GL.DeleteProgram(Program);
                logger.Info("Deleted shader program {0}", Program);
            }
            if (FragmentID != 0) {
                GL.DeleteShader(FragmentID);
                logger.Info("Deleted fragment shader {0}: {1}", FragmentID, FragmentSourceLocation);
            }
            if (GeometryID != 0 && GeometryID != -1) {
                GL.DeleteShader(GeometryID);
                logger.Info("Deleted geometry shader {0}: {1}", GeometryID, GeometrySourceLocation);
            }
            if (VertexID != 0) {
                GL.DeleteShader(VertexID);
                logger.Info("Deleted vertex shader {0}: {1}", VertexID, VertexSourceLocation);
            }
        }

        internal int GetAttributeLocation(string attribute) {
            if (attributes.ContainsKey(attribute)) {
                return attributes[attribute];
            } else {
                logger.Error("Attribute not found: " + attribute);
                throw new ApplicationException("Attribute not found: " + attribute);
            }
        }

        public void Start() {
            GL.UseProgram(Program);
        }
        public void Stop() {
            GL.UseProgram(0);
        }
    }
}
