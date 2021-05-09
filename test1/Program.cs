using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SDL2;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace test1
{
    class Program
    {
		private static Program Instance { get { return Nested.instance; } }     
        
        public static UInt64 PerfFrequency;
        public static bool IsRunning;

        private IntPtr sdlWindow = IntPtr.Zero;
        private IntPtr sdlGlContext = IntPtr.Zero;
        public const int windowWidth = 1024;
        public const int windowHeight = 768;
        private uint desiredSchedulerMs = 1;
        public GameScreen gameScreen;
        public GameScreen nextGameScreen;
        private bool isSleepGranular = false;
        public float targetFrameDuration;

        public Program()
        {
        }

        private class Nested
        {
            static Nested()
            {
            }

            internal static readonly Program instance = new Program();
        }

        public static Program GetInstance()
        {
            return Program.Instance;
        }

        static int Main(String[] args)
        {
            PerfFrequency = SDL.SDL_GetPerformanceFrequency();
            IsRunning = true;

            return Program.GetInstance().Execute(args);
        }

        public int Execute(String[] args)
        {
            int monitorRefreshRate = 60; // todo: get with sdl?
            int gameUpdateRate = monitorRefreshRate;
            targetFrameDuration = 1f / gameUpdateRate;
            Debug.Assert(targetFrameDuration != 0);

            EngineInit(); // sdl & opengl

            UInt64 prevCounter = GetCounter();
            SDL.SDL_Event event_;
            while (IsRunning) // main loop
            {
                UInt64 gaugeCounter = GetCounter();

                // frame resets
                Input.FrameStart();

                // events
                while (SDL.SDL_PollEvent(out event_) != 0)
                {
                    switch (event_.type)
                    {
                        case SDL.SDL_EventType.SDL_QUIT:
                            IsRunning = false;
                            break;
                        case SDL.SDL_EventType.SDL_KEYDOWN:
                            Input.HandleKeyDown(event_);
                            break;
                        case SDL.SDL_EventType.SDL_KEYUP:
                            Input.HandleKeyUp(event_);
                            break;
                        case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                            Input.HandleMouseDown(event_);
                            break;
                        case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                            Input.HandleMouseUp(event_);
                            break;
                        case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                            Input.HandleMouseWheel(event_);
                            break;
                        default:
                            break;
                    }                    
                }
                //Console.WriteLine("^-a {0:0.0000}", GetCounterDeltaSeconds(gaugeCounter, GetCounter()));

                if (Input.WasPressed(SDL.SDL_Scancode.SDL_SCANCODE_Q))
                {
                    Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++");
                    Console.WriteLine("pressed Q...");
                    Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++");
                }

                if (Input.WasReleased(SDL.SDL_Scancode.SDL_SCANCODE_Q))
                {
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine("released Q...");
                    Console.WriteLine("--------------------------------------------");
                }

                // update + render-prep
                EngineUpdate(targetFrameDuration);
                //Console.WriteLine("^-b {0:0.0000}", GetCounterDeltaSeconds(gaugeCounter, GetCounter()));

                // time since last frame, includes last frame's swap-window time plus the above GameUpdate and event handling code times
                double workSecondsElapsed = GetCounterDeltaSeconds(prevCounter, GetCounter());

                // sleep for rest of frame time
                double frameSecondsElapsed = workSecondsElapsed;
                if (frameSecondsElapsed < targetFrameDuration)
                {
                    if (isSleepGranular)
                    {
                        // how many ms left to wait, truncated, busy-wait remainder later
                        UInt32 sleepMs = (UInt32) (1000 * (targetFrameDuration - frameSecondsElapsed));
                        //Console.WriteLine("sleepMs: {0}", sleepMs);
                        // then we sleep
                        if (sleepMs > 0)
                        {
                            SDL.SDL_Delay(sleepMs);
                        }
                    }

                    double checkFrameSecondsElapsed = GetCounterDeltaSeconds(prevCounter, GetCounter());
                    if (checkFrameSecondsElapsed > targetFrameDuration)
                    {
                        Console.WriteLine("overshot target frame time, {0} / {1}", checkFrameSecondsElapsed, targetFrameDuration);
                    };

                    // busy-wait the remaining time
                    while (frameSecondsElapsed < targetFrameDuration)
                    {
                        // can optionally put the (isSleepGranular) block here, to be more robust in weird environments
                        frameSecondsElapsed = GetCounterDeltaSeconds(prevCounter, GetCounter());
                    }
                    //Console.WriteLine("^-d {0:0.0000}", GetCounterDeltaSeconds(gaugeCounter, GetCounter()));
                }
                else
                {
                    // dropped a frame
                    Console.WriteLine("--- dropped a frame, frameSecondsElapsed: {0:0.0000} | {0:0.0000} ---", frameSecondsElapsed, targetFrameDuration);
                }

                UInt64 endCounter = GetCounter();
                double msPerFrame = 1000 * GetCounterDeltaSeconds(prevCounter, endCounter);
                prevCounter = endCounter;

                // RENDER HERE // Get sdlWindow Dimension & Render (SWAP BUFFER)
                SDL.SDL_GL_SwapWindow(sdlWindow);

                // SOUND OPTIONS: SDL (Basic), OpenAL (in OpenTK), FAudio (from FNA/SDLCS author)

            } // isRunning

            EngineCleanup();            

            return 1;
        }

        public bool EngineInit()
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
            if ((sdlWindow = SDL.SDL_CreateWindow("SDL Test", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, windowWidth, windowHeight, flags)) == IntPtr.Zero)
            {
                return false;
            }

            { // opengl
                sdlGlContext = SDL.SDL_GL_CreateContext(sdlWindow);
                if (sdlGlContext == IntPtr.Zero)
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

                    // configure OpenTK so it uses the OpenGL context made by SDL
                    // https://github.com/mellinoe/ImGui.NET/issues/34#issuecomment-339567003
                    var contextHandle = new ContextHandle(sdlGlContext);
                    GraphicsContext ___MOKAY___ = new GraphicsContext(
                        contextHandle,
                        (proc) => SDL.SDL_GL_GetProcAddress(proc),
                        () => new ContextHandle(SDL.SDL_GL_GetCurrentContext())
                    );

                    // initialize
                    bool openGlSuccess = true;
                    if (!openGlSuccess)
                    {
                        Console.WriteLine("Unable to initialize OpenGL!");
                        return false;
                    }
                }

            }

            // ----------

            // wish: later: SDL.SDL_SetSystemTimerResolution(1); // need to update sdl?
            isSleepGranular = WinApi.TimeBeginPeriod(desiredSchedulerMs) == WinApi.TIMERR_NOERROR;
            Debug.Assert(isSleepGranular);
            SDL.SDL_Delay(128); // "wait for it to stabilize" (via stackoverflow) // also, prefer sdl

            // ----------

            gameScreen = new HelloMultipleLights();
            gameScreen.Init();

            return true;
        }

        public void EngineUpdate(float dt)
        {
            if (nextGameScreen != null)
            {
                gameScreen.Cleanup();
                gameScreen = nextGameScreen;
                gameScreen.Init();
                nextGameScreen = null;
            }

            gameScreen.Update(dt);            
        }

        public void EngineCleanup()
        {
            gameScreen.Cleanup();

            if (sdlWindow != IntPtr.Zero)
            {
                SDL.SDL_DestroyWindow(sdlWindow);
                sdlWindow = IntPtr.Zero;
            }

            // change system timer granularity back
            if (isSleepGranular)
            {
                WinApi.TimeEndPeriod(desiredSchedulerMs); // send same param, that's the ms api design o,O
                // wish: later: SDL.SDL_SetSystemTimerResolution(desiredSchedulerMs); // need to update sdl?
            }

            SDL.SDL_Quit();
        }

        // ---

        /*public void GameEvent(SDL.SDL_Event event_)
        {
            // todo: helloMulti.OnUpdate(event_);
            if (event_.type == SDL.SDL_EventType.SDL_MOUSEMOTION)
            {
                // todo: move to engine... just get mouse pos every frame
                int x, y;
                SDL.SDL_GetMouseState(out x, out y);
                Console.WriteLine("{0}, {1}", x, y);
            }
            else if (event_.type == SDL.SDL_EventType.SDL_MOUSEWHEEL)
            {

            }
        }*/

        // ---

        // high resolution timer
        public UInt64 GetCounter()
        {
            UInt64 result = SDL.SDL_GetPerformanceCounter();
            return result;
        }

        // get delta seconds from 2 counter values
        public double GetCounterDeltaSeconds(UInt64 start, UInt64 end)
        {
            double result = ( (double) (end - start) ) / PerfFrequency;
            return result;
        }

    }
}
