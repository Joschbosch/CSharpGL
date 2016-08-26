using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.Drawing;

namespace OpenGL {
    struct Vertex {
        private Vector2 position;
        private Vector2 texCoord;
        private Vector4 color;


        public Vertex(Vector2 position, Vector2 texCoord, Vector4 color) {
            this.position = position;
            this.texCoord = texCoord;
            this.color = color;
        }
        public Vertex(Vector2 position, Vector2 texCoord, Color color) {
            this.position = position;
            this.texCoord = texCoord;
            this.color = new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.R / 255f);

        }

        public static int SizeInBytes { get { return Vector2.SizeInBytes * 2 + Vector4.SizeInBytes; } }


        public Vector2 Position {
            get {
                return position;
            }

            set {
                this.position = value;
            }
        }

        public Vector2 TexCoord {
            get {
                return texCoord;
            }

            set {
                this.texCoord = value;
            }
        }

        public Color Color {
            get {
                return System.Drawing.Color.FromArgb((int)color.X * 255, (int)color.Y * 255, (int)color.Z * 255, (int)color.W * 255);
            }
            set {
                this.color = new Vector4(value.R / 255f, value.G / 255f, value.B / 255f, value.R / 255f);

            }
        }
    }
}
