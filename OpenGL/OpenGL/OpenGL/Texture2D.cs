using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL {
    class Texture2D {
        private int id;
        private int width, height;

        public int ID { get; }

        public int Height { get; set; }

        public int Width { get; set; }

        public Texture2D(int id, int width, int height) {
            this.id = id;
            this.width = width;
            this.height = height;
        }

    }
}
