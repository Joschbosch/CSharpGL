using OpenTK;
using System.Collections.Generic;

namespace OpenGL_Test_Environment.GUI.data {
    public class Mesh {
        public Mesh() {
            Triangles = new List<Vector3>();
            Normals = new List<Vector3>();
            UV = new List<Vector2>();

        }

        public string Name { get; set; }

        public List<Vector3> Triangles { get; set; }
        public List<Vector3> Normals { get; set; }
        public List<Vector2> UV { get; set; }
    }
}
