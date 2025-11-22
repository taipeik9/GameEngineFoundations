namespace Collision3D
{
    static class MeshFactory
    {
        public static Mesh CreateCube()
        {
            float[] vertices =
                [
                    -0.5f, -0.5f, -0.5f,
                    0.5f, -0.5f, -0.5f,
                    0.5f,  0.5f, -0.5f,
                    -0.5f,  0.5f, -0.5f,
                    -0.5f, -0.5f,  0.5f,
                    0.5f, -0.5f,  0.5f,
                    0.5f,  0.5f,  0.5f,
                    -0.5f,  0.5f,  0.5f
                ];

            uint[] indices =
            [
                // Front face
                0, 1, 2,
                2, 3, 0,

                // Back face
                4, 5, 6,
                6, 7, 4,

                // Left face
                0, 3, 7,
                7, 4, 0,

                // Right face
                1, 2, 6,
                6, 5, 1,

                // Top face
                3, 2, 6,
                6, 7, 3,

                // Bottom face
                0, 1, 5,
                5, 4, 0
            ];

            return new Mesh(vertices, indices);
        }
    }
}