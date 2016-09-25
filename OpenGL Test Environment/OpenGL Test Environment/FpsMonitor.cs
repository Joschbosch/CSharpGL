using System;
using System.Diagnostics;

namespace OpenGL_Test_Environment {
    public class FpsMonitor {
        public float Value { get; private set; }
        public TimeSpan Sample { get; set; }
        private Stopwatch sw;
        private int Frames;
        public FpsMonitor() {
            this.Sample = TimeSpan.FromSeconds(1);
            this.Value = 0;
            this.Frames = 0;
            this.sw = Stopwatch.StartNew();
        }
        public void Update() {
            if (sw.Elapsed > Sample) {
                this.Value = (float)(Frames / sw.Elapsed.TotalSeconds);
                this.sw.Reset();
                this.sw.Start();
                this.Frames = 0;
            }
        }
        public void Draw() {
            this.Frames++;
            Console.WriteLine("Current framerate : " + Value);
        }
    }
}
