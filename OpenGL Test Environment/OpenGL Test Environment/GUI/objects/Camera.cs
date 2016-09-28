using OpenTK;

namespace OpenGL_Test_Environment.GUI.objects {
    class Camera {

        public Vector3 Position { get; set; }
        public Quaternion Orientiation { get; set; }
        public Matrix4 ViewMatrix { get; set; }
        public Vector3 ryp;
        public Matrix4 ProjectionMatrix { get; set; }
        public Camera(Vector3 position, Quaternion orientation, int Width, int Height) {
            this.Position = position;
            this.Orientiation = orientation;
            ryp = new Vector3(0, 0, 0);
            CreateViewMatrix();
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Width / (float)Height, 0.1f, 100.0f);

        }


        private void CreateViewMatrix() {
            Matrix4 roll = Matrix4.CreateFromAxisAngle(new Vector3(0, 0, 1), ryp.X);
            Matrix4 yaw = Matrix4.CreateFromAxisAngle(new Vector3(0, 1, 0), ryp.Y);
            Matrix4 pitch = Matrix4.CreateFromAxisAngle(new Vector3(1, 0, 0), ryp.Z);

            Matrix4 rotation = Matrix4.Identity;

            rotation = Matrix4.Mult(rotation, yaw);
            rotation = Matrix4.Mult(rotation, pitch);
            rotation = Matrix4.Mult(rotation, roll);

            Matrix4 translate = Matrix4.CreateTranslation(Vector3.Multiply(Position, -1));
            ViewMatrix = Matrix4.Mult(translate, rotation);
        }

        internal void UpdatePosition(float dx, float dz, Vector2 mouseDelta) {

            const float mouseX_Sensitivity = 0.0025f;
            const float mouseY_Sensitivity = 0.0025f;
            ryp.Y += mouseX_Sensitivity * mouseDelta.X;
            ryp.Z += mouseY_Sensitivity * mouseDelta.Y;
            CreateViewMatrix();


            Vector3 forward = new Vector3(ViewMatrix[0, 2], ViewMatrix[1, 2], ViewMatrix[2, 2]);
            Vector3 strafe = new Vector3(ViewMatrix[0, 0], ViewMatrix[1, 0], ViewMatrix[2, 0]);
            float speed = 0.12f;

            forward = Vector3.Multiply(forward, -dz);
            strafe = Vector3.Multiply(strafe, dx);

            Vector3 newPos = Vector3.Add(forward, strafe);
            newPos = Vector3.Multiply(newPos, speed);
            Position = Vector3.Add(Position, newPos);
            CreateViewMatrix();
        }
    }
}


