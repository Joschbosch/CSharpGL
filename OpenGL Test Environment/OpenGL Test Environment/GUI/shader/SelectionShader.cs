namespace OpenGL_Test_Environment.GUI.shader {
    class SelectionShader : Shader {
        public SelectionShader() : base("selection") {
            bindAttribute("vertexPosition_modelspace");
            bindAttribute("vertexUV");
        }
    }
}
