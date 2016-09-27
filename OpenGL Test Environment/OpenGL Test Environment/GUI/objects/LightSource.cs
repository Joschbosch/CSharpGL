using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL_Test_Environment.GUI.objects {
    class LightSource {
        public Vector3 Position { get; set; }
        public Vector3 Color { get; set; }
        public float LightType { get; set; } // 0 = Point light, 1 = directional light, 2 = spot light
        public Vector3 Direction { get; set; }

        public LightSource(Vector3 position, Vector3 color, float type) {
            this.Position = position;
            this.Color = color;
            this.LightType = type;
        }

    }
}
