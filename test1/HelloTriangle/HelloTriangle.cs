using System;
using System.Collections.Generic;
using System.Text;
using SDL2;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;


namespace test1
{
    class HelloTriangle : GameScreen
    {
        private IntPtr Window = IntPtr.Zero;
        private IntPtr Renderer = IntPtr.Zero;
        private IntPtr PrimarySurface = IntPtr.Zero;
        private IntPtr gContext = IntPtr.Zero;

        private readonly float[] _vertices =
        {
            -0.5f, -0.5f, 0.0f, // Bottom-left vertex
             0.5f, -0.5f, 0.0f, // Bottom-right vertex
             0.0f,  0.5f, 0.0f  // Top vertex
        };
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private Shader _shader;

        private const int WindowWidth = 1024;
        private const int WindowHeight = 768;
    
        public override bool Init()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
                return false;

            if (SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_SCALE_QUALITY, "1") == SDL.SDL_bool.SDL_FALSE)
            {
                // some problem
            }

            // use opengl 3.3 core
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, 3);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, 3);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE);

            var flags = SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN;
            if ((Window = SDL.SDL_CreateWindow("SDL Test", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, WindowWidth, WindowHeight, flags)) == IntPtr.Zero)
            {
                return false;
            }

            { // opengl
                gContext = SDL.SDL_GL_CreateContext(Window);
                if (gContext == IntPtr.Zero)
                {
                    Console.WriteLine("OpenGL context could not be created! SDL Error: {0}", SDL.SDL_GetError());
                }
                else
                {
                    // use vsync
                    if (SDL.SDL_GL_SetSwapInterval(1) < 0)
                    {
                        Console.WriteLine("Warning: Unable to set VSync! SDL Error: {0}", SDL.SDL_GetError());
                    }

                    // initialize
                    if (!initGL())
                    {
                        Console.WriteLine("Unable to initialize OpenGL!");
                        return false;
                    }
                }
            }

            return true;
        }

        public bool initGL()
        {
            //Success flag
            bool success = true;

            // hoi: https://github.com/mellinoe/ImGui.NET/issues/34#issuecomment-339567003
            var contextHandle = new ContextHandle(gContext);
            GraphicsContext ___MOKAY___ = new GraphicsContext(
                contextHandle,
                (proc) => SDL.SDL_GL_GetProcAddress(proc),
                () => new ContextHandle(SDL.SDL_GL_GetCurrentContext())
            );

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
            _shader = new Shader("HelloTriangle/shader.vert", "HelloTriangle/shader.frag");
            _shader.Use();
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            return success;
        }

        public void printShaderLog(int shader)
        {
            //Make sure name is shader
            if (GL.IsShader(shader))
            {
                //Shader log length
                int infoLogLength = 0;
                int maxLength = infoLogLength;

                //Get info string length
                GL.GetShader(shader, ShaderParameter.InfoLogLength, out maxLength);

                //Allocate string
                string infoLog;

                //Get info log
                GL.GetShaderInfoLog(shader, maxLength, out infoLogLength, out infoLog);
                if (infoLogLength > 0)
                {
                    //Print Log
                    Console.WriteLine("{0}", infoLog);
                }

            }
            else
            {
                Console.WriteLine("Name `{0}` is not a shader", shader);
            }
        }

        public void printProgramLog(int program)
        {
            //Make sure name is shader
            if (GL.IsProgram(program))
            {
                //Program log length
                int infoLogLength = 0;
                int maxLength = infoLogLength;

                //Get info string length
                GL.GetProgram(program, GetProgramParameterName.InfoLogLength, out maxLength);

                //Allocate string
                string infoLog;

                //Get info log
                GL.GetProgramInfoLog(program, maxLength, out infoLogLength, out infoLog);
                if (infoLogLength > 0)
                {
                    //Print Log
                    Console.WriteLine("{0}", infoLog);
                }
            }
            else
            {
                Console.WriteLine("Name %d is not a program\n", program);
            }
        }

        public override void Update(float dt)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            _shader.Use();
            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            //SwapBuffers();
            SDL.SDL_GL_SwapWindow(Window);
        }

        public override void Cleanup()
        {
            if (Renderer != IntPtr.Zero)
            {
                SDL.SDL_DestroyRenderer(Renderer);
                Renderer = IntPtr.Zero;
            }

            if (Window != IntPtr.Zero)
            {
                SDL.SDL_DestroyWindow(Window);
                Window = IntPtr.Zero;
            }

            SDL.SDL_Quit();
        }

    }

}
