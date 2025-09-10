using System;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;

namespace WindowEngine
{
    public class Game : GameWindow
    {
        private int vertexBufferHandle;
        private int shaderProgramHandle;
        private int vertexArrayHandle;
        public Game() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.Size = new Vector2i(1280, 768);

            this.CenterWindow(this.Size);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);
            base.OnResize(e);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(new Color4(0.9098f, 0.9098f, 0.9098f, 1f));

            // using framebuffer size here due to potential discrepancy between
            // window size and framebuffer size on MacOS
            GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);

            float[] vertices = { 
                // first triangle (bottom left triangle)
                -0.6f, -0.5f, 0.0f, // Bottom-left vertex
                -0.6f,  0.5f, 0.0f, // Top-left vertex
                0.6f, -0.5f, 0.0f, // Bottom-right vertex

                // second triangle (top right triangle)
                0.6f, 0.5f, 0.0f, // Top-right vertex
                -0.6f,  0.5f, 0.0f, // Top-left vertex
                0.6f, -0.5f, 0.0f, // Bottom-right vertex
            };

            vertexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            vertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayHandle);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            string vertexShaderCode = @"
                #version 330 core
                layout(location = 0) in vec3 aPosition;

                void main() 
                {
                    gl_Position = vec4(aPosition, 0.7);
                }
            ";

            string fragmentShaderCode = @"
                #version 330 core
                out vec4 FragColor;

                void main()
                {
                    FragColor = vec4(0.231f, 0.639f, 1.000f, 1.0f); // light blue color
                }
            ";

            int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderHandle, vertexShaderCode);
            GL.CompileShader(vertexShaderHandle);
            CheckShaderCompile(vertexShaderHandle, "Vertex Shader");

            int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShaderHandle, fragmentShaderCode);
            GL.CompileShader(fragmentShaderHandle);
            CheckShaderCompile(fragmentShaderHandle, "Fragment Shader");

            shaderProgramHandle = GL.CreateProgram();
            GL.AttachShader(shaderProgramHandle, vertexShaderHandle);
            GL.AttachShader(shaderProgramHandle, fragmentShaderHandle);
            GL.LinkProgram(shaderProgramHandle);

            GL.DetachShader(shaderProgramHandle, vertexShaderHandle);
            GL.DetachShader(shaderProgramHandle, fragmentShaderHandle);
            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(fragmentShaderHandle);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(shaderProgramHandle);
            GL.BindVertexArray(vertexArrayHandle);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0); // reset

            // DrawArrays drew to the back buffer, so now we have to swap the
            // buffers to display on the screen buffer
            SwapBuffers();
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vertexBufferHandle);

            GL.BindVertexArray(0);
            GL.DeleteVertexArray(vertexArrayHandle);

            GL.UseProgram(0);
            GL.DeleteProgram(shaderProgramHandle);

            base.OnUnload();
        }

        // Helper function to check for shader compilation errors
        private void CheckShaderCompile(int shaderHandle, string shaderName)
        {
            GL.GetShader(shaderHandle, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shaderHandle);
                Console.WriteLine($"Error compiling {shaderName}: {infoLog}");
            }
        }
    }
}