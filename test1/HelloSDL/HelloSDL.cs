using System;
using System.Collections.Generic;
using System.Text;
using SDL2;

namespace test1
{
    class HelloSDL : GameScreen
    {
        private IntPtr Window = IntPtr.Zero;
        private IntPtr Renderer = IntPtr.Zero;
        private IntPtr PrimarySurface;

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

            if ((Window = SDL.SDL_CreateWindow("SDL Test", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, WindowWidth, WindowHeight, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN)) == IntPtr.Zero)
            {
                return false;
            }

            PrimarySurface = SDL.SDL_GetWindowSurface(Window);

            if ((Renderer = SDL.SDL_CreateRenderer(Window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED)) == IntPtr.Zero)
            {
                return false;
            }

            SDL.SDL_SetRenderDrawColor(Renderer, 0xae, 0xFF, 0x00, 0xFF);

            return true;
        }

        public override void Update(float dt)
        {
            SDL.SDL_RenderClear(Renderer);
            SDL.SDL_RenderPresent(Renderer);
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

    /*
        public static int GetWindowWidth()
        {
            return windowWidth;
        }

        public static int GetWindowHeight()
        {
            return windowHeight;
        }
    */
}
