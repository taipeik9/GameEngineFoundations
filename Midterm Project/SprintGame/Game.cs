using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace SprintGame
{
    public class Game(NativeWindowSettings nativeSettings) : GameWindow(GameWindowSettings.Default, nativeSettings)
    {
        private enum GameState { Title, Playing }
        private GameState currentState = GameState.Title;

        // cube data
        private readonly float[] vertices = {
            // front face
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,

            // back face
            -0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
            0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
            0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f
        };


        private readonly uint[] indices = {
            0,1,2, 2,3,0,
            4,5,6, 6,7,4,
            0,3,7, 7,4,0,
            1,2,6, 6,5,1,
            3,2,6, 6,7,3,
            0,1,5, 5,4,0
        };

        // GL handles for 3D cube
        private int vboHandle, vaoHandle, eboHandle, phongProgram, textureHandle = 0;

        // GL handles for 2D UI (button & crosshair)
        private int uiProgram;
        private int uiColorLoc, uiOffsetLoc, uiScaleLoc;

        // button VAO/VBO
        private int buttonVao, buttonVbo;
        private Vector2 buttonSize = new(0.3f, 0.1f);

        // crosshair VAO/VBO
        private int crosshairVao, crosshairVbo;
        private readonly float crosshairHalfLength = 0.02f;

        // input state
        private bool mousePressedLastFrame = false;

        // camera & movement
        private Vector3 cameraPos = new(0f, 0f, 3f);
        private Vector3 cameraFront = new(0f, 0f, -1f);
        private readonly Vector3 cameraUp = new(0f, 1f, 0f);
        private float yaw = -90f, pitch = 0f;
        private readonly float sensitivity = 0.1f;

        private float verticalVelocity = 0f;
        private bool isGrounded = true;
        private readonly float gravity = -9.81f;
        private readonly float jumpSpeed = 3.5f;
        private readonly float moveSpeed = 2.5f;
        private readonly float sprintModifier = 2.0f;
        private readonly float groundY = 0f;
        private float lightingStrengthModifier = 1.0f;

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);
            GL.Enable(EnableCap.DepthTest);

            // --- 3D scene setup (cube) ---
            vaoHandle = GL.GenVertexArray();
            vboHandle = GL.GenBuffer();
            eboHandle = GL.GenBuffer();

            GL.BindVertexArray(vaoHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // positions
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // tex coords
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // load phong shaders from files
            phongProgram = CreateProgram("shaders/phong.vert", "shaders/phong.frag");

            // set phong uniforms (light/object colours)
            GL.UseProgram(phongProgram);
            GL.Uniform3(GL.GetUniformLocation(phongProgram, "lightPos"), new Vector3(2f, 2f, 2f));
            GL.Uniform3(GL.GetUniformLocation(phongProgram, "lightColor"), new Vector3(1f, 1f, 1f));
            GL.Uniform3(GL.GetUniformLocation(phongProgram, "objectColor"), new Vector3(0.4f, 0.8f, 1f));

            const string texturePath = "assets/crate.jpg";
            if (File.Exists(texturePath))
            {
                textureHandle = LoadTexture(texturePath);

                int loc = GL.GetUniformLocation(phongProgram, "texture0");
                if (loc != -1) GL.Uniform1(loc, 0); // texture unit 0
            }
            else
            {
                Console.WriteLine($"Texture not found: {texturePath}");
            }

            uiProgram = CreateProgram("shaders/ui.vert", "shaders/ui.frag");

            // get ui uniforms
            uiColorLoc = GL.GetUniformLocation(uiProgram, "color");
            uiOffsetLoc = GL.GetUniformLocation(uiProgram, "offset");
            uiScaleLoc = GL.GetUniformLocation(uiProgram, "scale");

            // setup button geometry
            float[] buttonVerts = {
                -1f, -1f,
                 1f, -1f,
                 1f,  1f,
                -1f,  1f
            };
            buttonVao = GL.GenVertexArray();
            buttonVbo = GL.GenBuffer();
            GL.BindVertexArray(buttonVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, buttonVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, buttonVerts.Length * sizeof(float), buttonVerts, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            // setup crosshair geometry
            float[] crosshairVerts = {
                0f,  1.1f,   // vertical top
                0f, -1f,   // vertical bottom
               -1f,  0f,   // horizontal left
                0.9f,  0f    // horizontal right
            };
            crosshairVao = GL.GenVertexArray();
            crosshairVbo = GL.GenBuffer();
            GL.BindVertexArray(crosshairVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, crosshairVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, crosshairVerts.Length * sizeof(float), crosshairVerts, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            // initial cursor state: normal on title screen
            CursorState = CursorState.Normal;

            // ensure camera grounded if starting at or below ground (for 3D scene)
            if (cameraPos.Y <= groundY + 0.0001f)
            {
                cameraPos.Y = groundY;
                isGrounded = true;
                verticalVelocity = 0f;
            }

            GL.BindVertexArray(0);
        }

        // helper - compile shaders from file paths
        private static int CompileShaderFromFile(ShaderType type, string path)
        {
            int shader = GL.CreateShader(type);
            string src = File.ReadAllText(path);
            GL.ShaderSource(shader, src);
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
                throw new Exception($"Shader compile error ({type}): {GL.GetShaderInfoLog(shader)}");
            return shader;
        }

        // helper - create programs for vertex and fragment shaders
        private static int CreateProgram(string vertPath, string fragPath)
        {
            int v = CompileShaderFromFile(ShaderType.FragmentShader, fragPath);
            int f = CompileShaderFromFile(ShaderType.VertexShader, vertPath);

            int prog = GL.CreateProgram();
            GL.AttachShader(prog, v);
            GL.AttachShader(prog, f);
            GL.LinkProgram(prog);

            GL.DetachShader(prog, v);
            GL.DetachShader(prog, f);
            GL.DeleteShader(v);
            GL.DeleteShader(f);

            return prog;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (currentState == GameState.Title)
            {
                DrawTitleScreen();
            }
            else
            {
                DrawGameScene();
            }

            SwapBuffers();
        }

        private void DrawTitleScreen()
        {
            // depth test is not needed for title screen
            GL.Disable(EnableCap.DepthTest);
            GL.UseProgram(uiProgram);

            Vector2 scale = buttonSize / 2f;
            GL.Uniform2(uiScaleLoc, ref scale);

            // detect hover
            Vector2 mousePos = new(
                (float)(MousePosition.X / Size.X * 2.0 - 1.0),
                (float)(1.0 - MousePosition.Y / Size.Y * 2.0)
            );

            bool hovered = Math.Abs(mousePos.X) <= scale.X && Math.Abs(mousePos.Y) <= scale.Y;

            Vector3 color = hovered ? new Vector3(0.8f, 0.8f, 1.0f) : new Vector3(0.5f, 0.5f, 0.9f);
            GL.Uniform3(uiColorLoc, color);

            GL.BindVertexArray(buttonVao);
            // draw rectangle as triangle fan
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            // handle click -> start game
            bool mouseDown = MouseState.IsButtonDown(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left);
            if (hovered && mouseDown && !mousePressedLastFrame)
            {
                StartGame();
            }
            mousePressedLastFrame = mouseDown;
            GL.BindVertexArray(0);
        }

        private void DrawGameScene()
        {
            GL.Enable(EnableCap.DepthTest);

            // 3D scene
            Matrix4 model = Matrix4.Identity;
            Matrix4 view = Matrix4.LookAt(cameraPos, cameraPos + cameraFront, cameraUp);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size.X / (float)Size.Y, 0.1f, 100f);

            GL.UseProgram(phongProgram);
            GL.UniformMatrix4(GL.GetUniformLocation(phongProgram, "model"), false, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(phongProgram, "view"), false, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(phongProgram, "projection"), false, ref projection);
            GL.Uniform3(GL.GetUniformLocation(phongProgram, "viewPos"), ref cameraPos);
            GL.Uniform1(GL.GetUniformLocation(phongProgram, "lightModifier"), lightingStrengthModifier);


            if (textureHandle != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, textureHandle);
            }


            GL.BindVertexArray(vaoHandle);
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            // Crosshair (UI overlay)
            GL.Disable(EnableCap.DepthTest);
            GL.UseProgram(uiProgram);

            Vector2 crossScale = new(crosshairHalfLength, crosshairHalfLength);
            Vector2 crossOffset = Vector2.Zero;
            GL.Uniform2(uiScaleLoc, ref crossScale);
            GL.Uniform2(uiOffsetLoc, ref crossOffset);
            GL.Uniform3(uiColorLoc, new Vector3(1f, 1f, 1f));

            GL.BindVertexArray(crosshairVao);
            GL.LineWidth(2f);

            GL.DrawArrays(PrimitiveType.Lines, 0, 4);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
        }

        private void StartGame()
        {
            currentState = GameState.Playing;
            CursorState = CursorState.Grabbed;
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            // only update gameplay input when playing
            if (currentState != GameState.Playing)
                return;

            float deltaTime = (float)args.Time;

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
                Close();

            bool isSprinting = KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftControl);

            Vector3 horizontalRight = Vector3.Normalize(Vector3.Cross(cameraFront, cameraUp));
            Vector3 frontFlat = new(cameraFront.X, 0f, cameraFront.Z);
            if (frontFlat.LengthSquared > 0.0f)
                frontFlat = Vector3.Normalize(frontFlat);

            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W))
                cameraPos += frontFlat * (isSprinting ? moveSpeed * sprintModifier : moveSpeed) * deltaTime;
            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S))
                cameraPos -= frontFlat * (isSprinting ? moveSpeed * sprintModifier : moveSpeed) * deltaTime;
            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A))
                cameraPos -= horizontalRight * moveSpeed * deltaTime;
            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D))
                cameraPos += horizontalRight * moveSpeed * deltaTime;

            // jumping logic
            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Space) && isGrounded)
            {
                verticalVelocity = jumpSpeed;
                isGrounded = false;
            }

            verticalVelocity += gravity * deltaTime;
            cameraPos.Y += verticalVelocity * deltaTime;

            if (cameraPos.Y <= groundY)
            {
                cameraPos.Y = groundY;
                verticalVelocity = 0f;
                isGrounded = true;
            }
        }

        protected override void OnMouseMove(MouseMoveEventArgs args)
        {
            base.OnMouseMove(args);

            // only respond to mouse look when in Playing state
            if (currentState != GameState.Playing) return;

            yaw += args.DeltaX * sensitivity;
            pitch -= args.DeltaY * sensitivity;
            pitch = Math.Clamp(pitch, -89f, 89f);

            Vector3 front;
            front.X = MathF.Cos(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(pitch));
            front.Z = MathF.Sin(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));
            cameraFront = Vector3.Normalize(front);
        }

        private static int LoadTexture(string path)
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(path);
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            byte[] pixels = new byte[4 * image.Width * image.Height];
            image.CopyPixelDataTo(pixels);

            int handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, handle);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                image.Width, image.Height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            return handle;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs args)
        {
            base.OnMouseWheel(args);

            lightingStrengthModifier += args.OffsetY * 0.01f;
            if (lightingStrengthModifier < 0f) lightingStrengthModifier = 0f;
            if (lightingStrengthModifier > 2.0f) lightingStrengthModifier = 2.0f;
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            // delete 3D resources
            GL.DeleteBuffer(vboHandle);
            GL.DeleteBuffer(eboHandle);
            GL.DeleteVertexArray(vaoHandle);
            GL.DeleteProgram(phongProgram);

            // delete UI resources
            GL.DeleteBuffer(buttonVbo);
            GL.DeleteVertexArray(buttonVao);
            GL.DeleteBuffer(crosshairVbo);
            GL.DeleteVertexArray(crosshairVao);
            GL.DeleteProgram(uiProgram);
        }
    }
}
