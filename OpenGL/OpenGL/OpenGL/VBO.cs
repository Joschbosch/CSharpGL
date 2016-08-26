using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;
namespace OpenGL {
    class VBO {
        private int id;

        public VBO(int id) {
            this.id = id;
        }

        public static VBO CreateNewVBO(Vector2[] vertices) {
            int id = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, id);
            GL.BufferData<Vector2>(BufferTarget.ArrayBuffer, (IntPtr)(Vector2.SizeInBytes * vertices.Length), vertices, BufferUsageHint.StaticDraw);

            return new VBO(id);
        }

        public int Id { get; set; }


    }
}
