﻿namespace OpenGL_Test_Environment.GUI.shader {
    class SimpleShader : Shader {
        public SimpleShader() : base("simple") {
            bindAttribute("vertexPosition_modelspace");
            bindAttribute("vertex_normal");
            bindAttribute("vertexUV");
        }
    }
}
