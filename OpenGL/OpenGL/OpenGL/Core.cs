using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

namespace OpenGL {
    class Core {
        private GameWindow window;
        private Texture2D texture;
        private VBO vbo;
        private Vertex[] vertices;

        public Core(GameWindow window) {
            this.window = window;
            window.Load += Window_Load;
            window.UpdateFrame += Window_UpdateFrame;
            window.RenderFrame += Window_RenderFrame;

        }

        private void Window_RenderFrame(object sender, FrameEventArgs e) {
            GL.ClearColor(Color.CornflowerBlue);
            GL.ClearDepth(1);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.VertexPointer(2, VertexPointerType.Float, Vector2.SizeInBytes, 0);
            GL.VertexPointer(2, VertexPointerType.Float, Vector2.SizeInBytes, Vector2.SizeInBytes);
            GL.VertexPointer(4, VertexPointerType.Float, Vector2.SizeInBytes, Vector2.SizeInBytes * 2);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.Id);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length);

            window.SwapBuffers();
        }

        private void Window_UpdateFrame(object sender, FrameEventArgs e) {

        }

        private void Window_Load(object sender, EventArgs e) {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable(EnableCap.Texture2D);

            texture = ContentPipe.LoadTexture("resources/penguin.png");

            vertices = new Vertex[6] {
                new Vertex(new Vector2(0,0), new Vector2(0,0), Color.Red),
                new Vertex(new Vector2(100,0), new Vector2(1,0), Color.Yellow),
                new Vertex(new Vector2(100,100), new Vector2(1,1), Color.Blue),
                new Vertex(new Vector2(0,0), new Vector2(0,0), Color.Red),
               new Vertex( new Vector2(100,100), new Vector2(1,1), Color.Blue),
               new Vertex( new Vector2(0,100), new Vector2(0,1), Color.Green)
            };
            vbo = VBO.CreateNewVBO(ref vertices);

        }
    }

}
