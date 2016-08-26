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

        public static VBO CreateNewVBO(ref Vertex[] vertices) {
            int id = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, id);
            GL.BufferData<Vertex>(BufferTarget.ArrayBuffer, (IntPtr)(Vertex.SizeInBytes * vertices.Length), vertices, BufferUsageHint.StaticDraw);

            return new VBO(id);
        }

        public int Id { get { return id; } }


    }
}
