namespace OpenGL_Test_Environment.GUI.shader {
    class SimpleShader : Shader {
        public SimpleShader() : base("simple") {
            bindAttribute("vertex_position");
            bindAttribute("vertex_color");
            bindAttribute("vertex_normal");
            bindAttribute("vertex_texcoord");
        }
    }
}
