using NLog;
using ObjLoader.Loader.Loaders;
using OpenGL_Test_Environment.GUI.data;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace OpenGL_Test_Environment.GUI.content {
    class ContentManager {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static Dictionary<string, Bitmap> loadedImages = new Dictionary<string, Bitmap>();

        public static Bitmap loadImageResource(string path) {
            if (!File.Exists("resources/" + path)) {
                logger.Error("Resource does not exist: resources/" + path);
                throw new FileNotFoundException("Resource does not exist: resources/" + path);
            }

            if (loadedImages.ContainsKey(path)) {
                logger.Info("Found image resource \"{0}\" in cache.", path);
                return loadedImages[path];
            }
            Bitmap newBmp = new Bitmap("resources/" + path);
            loadedImages.Add(path, newBmp);
            logger.Info("Loaded image resource \"{0}\" and added it to image cache.", path);
            return newBmp;


        }

        public static VertexFloatBuffer loadModel(string modelname, VertexFormat format) {
            var objLoaderFactory = new ObjLoaderFactory();
            var objLoader = objLoaderFactory.Create(new MaterialNullStreamProvider());
            var fileStream = new FileStream("resources/model/" + modelname + "/" + modelname + ".obj", FileMode.Open);
            var result = objLoader.Load(fileStream);
            fileStream.Close();
            IList<Mesh> meshes = new OBJToMeshConverter().Convert(result);

            VertexFloatBuffer buffer = new VertexFloatBuffer(format);
            foreach (var mesh in meshes) {
                List<float[]> vertices = new List<float[]>();
                List<uint> indices = new List<uint>();
                for (int i = 0; i < mesh.Triangles.Count; i++) {
                    float[] newVertex;
                    if (format.Equals(VertexFormat.XYZ_NORMAL_UV)) {
                        newVertex = new float[] { mesh.Triangles[i].X, mesh.Triangles[i].Y, mesh.Triangles[i].Z, mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z, mesh.UV[i].X, mesh.UV[i].Y };
                    } else if (format.Equals(VertexFormat.XYZ)) {
                        newVertex = new float[] { mesh.Triangles[i].X, mesh.Triangles[i].Y, mesh.Triangles[i].Z };
                    } else if (format.Equals(VertexFormat.XYZ_UV)) {
                        newVertex = new float[] { mesh.Triangles[i].X, mesh.Triangles[i].Y, mesh.Triangles[i].Z, mesh.UV[i].X, mesh.UV[i].Y };
                    } else {
                        newVertex = new float[] { };
                    }
                    bool exists = false;
                    for (uint j = 0; j < vertices.Count; j++) {
                        if (vertices[(int)j].SequenceEqual(newVertex)) {
                            indices.Add(j);
                            exists = true;
                        }
                    }
                    if (!exists) {
                        vertices.Add(newVertex);
                        indices.Add((uint)vertices.Count - 1);
                    }
                }
                float[] vertexData = new float[vertices.Count * vertices[0].Length];

                int k = 0;
                foreach (var data in vertices) {
                    for (int j = 0; j < data.Length; j++) {
                        vertexData[k++] = data[j];
                    }
                }
                buffer.Set(vertexData, indices.ToArray());
                buffer.Load();
            }
            return buffer;
        }
        public static void disposeImageResource(string path) {
            if (loadedImages.ContainsKey(path)) {
                loadedImages[path].Dispose();
                loadedImages.Remove(path);
                logger.Info("Disposed image resource \"{0}\" from cache.", path);
            }
        }

        public static void dispose() {
            foreach (var item in loadedImages) {
                disposeImageResource(item.Key);
            }
        }
    }
}
