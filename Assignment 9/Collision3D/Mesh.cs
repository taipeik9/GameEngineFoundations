using OpenTK.Graphics.OpenGL4;

namespace Collision3D
{
    class Mesh
    {
        public float[] Vertices;
        public uint[] Indices;

        public int VertexCount => Vertices.Length;
        public int IndexCount => Indices.Length;

        private int _vao;
        private int _vbo;
        private int _ebo;
        public Mesh(float[] vertices, uint[] indices)
        {
            Vertices = vertices;
            Indices = indices;
        }

        public void UploadToGPU()
        {
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, VertexCount * sizeof(float), Vertices, BufferUsageHint.StaticDraw);

            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, IndexCount * sizeof(uint), Indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        }

        public void Draw()
        {
            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, IndexCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }
    }
}