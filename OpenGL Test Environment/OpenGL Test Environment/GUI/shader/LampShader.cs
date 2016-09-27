namespace OpenGL_Test_Environment.GUI.shader {
    class LampShader : Shader {
        public LampShader() : base("lamp") {
            bindAttribute("vertexPosition_modelspace");

        }
    }
}
