using OpenTK.Graphics.OpenGL4;

struct Texture2D {
    private int id;
    private int width, height;

    private bool pixelated;

    public int ID {
        get { return id; }
    }
    public int Width {
        get { return width; }
    }
    public int Height {
        get { return height; }
    }

    public Texture2D(int id, int width, int height) {
        this.id = id;
        this.width = width;
        this.height = height;
        this.pixelated = false;
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
    }

    public void setFilter(bool pixelated) {
        this.pixelated = pixelated;
        GL.BindTexture(TextureTarget.Texture2D, id);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, pixelated ? (int)TextureMinFilter.Nearest : (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, pixelated ? (int)TextureMinFilter.Nearest : (int)TextureMinFilter.Linear);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public bool isPixelated() {
        return pixelated;
    }

    internal void setWrapping(TextureWrapMode repeat) {
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)repeat);
    }
}