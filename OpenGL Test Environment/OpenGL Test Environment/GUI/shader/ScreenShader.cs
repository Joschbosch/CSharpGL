using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL_Test_Environment.GUI.shader {
    class ScreenShader : Shader {
        public ScreenShader() : base("screen") {
            bindAttribute("position");
            bindAttribute("texCoords");
        }
    }
}
