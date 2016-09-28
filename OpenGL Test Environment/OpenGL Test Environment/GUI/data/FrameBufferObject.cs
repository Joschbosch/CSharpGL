using NLog;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL_Test_Environment.GUI.data {
    class FrameBufferObject {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public int colorTexture { get; set; }
        public int id_fbo { get; set; }
        private int renderBuffer;
        private int width;
        private int height;

        public FrameBufferObject(int width, int height, int samples) {
            this.width = width;
            this.height = height;

            id_fbo = GL.GenFramebuffer();
            bindFBO();
            colorTexture = createTextureAttachment(width, height, samples);
            renderBuffer = createRenderBufferObject(width, height, samples);
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete) {
                logger.Info("Created new FBO, size {0}x{1}", width, height);
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        }
        public FrameBufferObject(int width, int height) {
            this.width = width;
            this.height = height;

            id_fbo = GL.GenFramebuffer();
            bindFBO();
            colorTexture = createTextureAttachment(width, height);
            renderBuffer = createRenderBufferObject(width, height);
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete) {
                logger.Info("Created new FBO, size {0}x{1}", width, height);
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        }

        public void bindFBO() {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id_fbo);
            GL.Viewport(0, 0, width, height);
        }

        public static void bindDefaultFBO(int width, int height) {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, width, height);
        }

        public void dispose() {
            GL.DeleteFramebuffer(id_fbo);
        }

        public int createTextureAttachment(int width, int height) {
            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height,
                0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D,
                texture, 0);
            return texture;
        }

        protected int createDepthTextureAttachment(int width, int height) {
            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8, width, height,
                0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D,
              texture, 0);
            return texture;
        }
        public int createTextureAttachment(int width, int height, int samples) {
            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DMultisample, texture);

            GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, 8, PixelInternalFormat.Rgb, width, height, true);

            GL.TexParameter(TextureTarget.Texture2DMultisample, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DMultisample, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2DMultisample,
                texture, 0);
            return texture;
        }
        //This is read only! 
        protected int createRenderBufferObject(int width, int height, int samples) {
            int rbo = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, 8, RenderbufferStorage.Depth24Stencil8, width, height);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, rbo);
            return rbo;

        }
        protected int createDepthBufferAttachment(int width, int height) {
            int depthBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthBuffer);
            return depthBuffer;
        }

        //This is read only! 
        protected int createRenderBufferObject(int width, int height) {
            int rbo = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, rbo);
            return rbo;

        }
    }
}
