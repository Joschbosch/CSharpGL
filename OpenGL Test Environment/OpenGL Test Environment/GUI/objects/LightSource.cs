using OpenTK;
using System;

namespace OpenGL_Test_Environment.GUI.objects {
    class LightSource {

        public static int TYPE_POINT_LIGHT = 0;
        public static int TYPE_DIRECTIONAL_LIGHT = 1;
        public static int TYPE_SPOT_LIGHT = 2;

        public Vector3 Color { get; set; }
        public int LightType { get; set; } // 0 = Point light, 1 = directional light, 2 = spot light
        public Vector3 Ambient { get; set; }
        public Vector3 Diffuse { get; set; }
        public Vector3 Specular { get; set; }

        // Point and spot
        public Vector3 Direction { get; set; }

        // Point and spot
        public Vector3 Position { get; set; }

        // point and spot light
        public Vector3 Attenuation { get; set; }

        // spot light
        public float CutOff { get; set; }
        public float outerCutOff { get; set; }

        public LightSource(Vector3 position, int type) {
            this.Position = position;
            if (type == TYPE_DIRECTIONAL_LIGHT) {
                this.Direction = new Vector3(position);
            }
            this.LightType = type;
            this.Ambient = new Vector3(0.05f, 0.05f, 00.5f);
            this.Diffuse = new Vector3(0.8f, 0.8f, 0.8f);
            this.Specular = new Vector3(1f, 1f, 1f);
            this.Attenuation = new Vector3(1f, 0.09f, 0.032f);
            this.CutOff = (float)Math.Cos(Math.PI * 12.5f / 180.0f);
            this.outerCutOff = (float)Math.Cos(Math.PI * 15f / 180.0f);
        }

    }
}
