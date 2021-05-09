using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SDL2;

namespace test1
{
    public class Input
    {
        private static int _numScancodes = (int) SDL.SDL_Scancode.SDL_NUM_SCANCODES;
        private static bool[] scancodesPressed = new bool[_numScancodes];
        private static bool[] scancodesReleased = new bool[_numScancodes];

        public static int mousewheelDeltaX;        
        public static int mousewheelDeltaY;

        public static void FrameStart()
        {
            Memory.SetBools(scancodesPressed, false, _numScancodes);
            Memory.SetBools(scancodesReleased, false, _numScancodes);
            mousewheelDeltaX = 0;
            mousewheelDeltaY = 0;
        }

        public static bool WasPressed(SDL.SDL_Scancode scanCode)
        {
            var x = scancodesPressed[(int) scanCode];
            return x;
        }

        public static bool WasReleased(SDL.SDL_Scancode scanCode)
        {
            var x = scancodesReleased[(int) scanCode];
            return x;
        }

        unsafe public bool IsKeyDown(SDL.SDL_Scancode scanCode)
        {
            int numKeys;
            // https://stackoverflow.com/questions/9732625/can-intptr-be-cast-into-a-byte-array-without-doing-a-marshal-copy
            // https://wiki.libsdl.org/SDL_GetKeyboardState
            IntPtr keyboardState = SDL.SDL_GetKeyboardState(out numKeys);
            byte* b = (byte*) keyboardState;
            return b[(int) scanCode] == 1;
        }

        /*
        https://github.com/zielmicha/SDL2/tree/master/include
        
        event_.key.keysym <SDL2.SDL.SDL_Keysym>
            mod: KMOD_NUM
            scancode: SDL_SCANCODE_W
            sym: SDL.SDL_Keycode.SDLK_w
            unicode: 0

        // event_.key.keysym.sym == SDL.SDL_Keycode.SDLK_w ('w')
        // event_.key.keysym.scancode == SDL.SDL_Scancode.SDL_SCANCODE_W (26)

        // SDL_keycode enum (signed 32 bit int)
        // SDL_Scancode enum (unsigned 32 bit int) [SDL_NUM_SCANCODES element value shows max]
        // Scancodes are converted to Keycodes by or'ing with (1<<30) [which is equivalent to adding 1,073,741,824]
        //   So, keycode values span a range of over a billion indexes
        //   And scancode values span a range of 512 indexes, as defined by SDL_NUM_SCANCODES
        */
        public static void HandleKeyDown(SDL.SDL_Event event_)
        {
            if (event_.key.repeat == 0)
            {
                scancodesPressed[(int) event_.key.keysym.scancode] = true;
            }
        }

        public static void HandleKeyUp(SDL.SDL_Event event_)
        {
            if (event_.key.repeat == 0)
            {
                scancodesReleased[(int) event_.key.keysym.scancode] = true;
            }
        }

        public static void HandleMouseDown(SDL.SDL_Event event_)
        {
            
        }

        public static void HandleMouseUp(SDL.SDL_Event event_)
        {
            
        }

        public static void HandleMouseWheel(SDL.SDL_Event event_)
        {
            if (event_.wheel.which != SDL.SDL_TOUCH_MOUSEID)
            {
                mousewheelDeltaX = event_.wheel.x; // delta values
                mousewheelDeltaY = event_.wheel.y;
                //Console.WriteLine("{0}, {1}", mousewheelDeltaX, mousewheelDeltaY);
                if (event_.wheel.direction == (uint) SDL.SDL_MouseWheelDirection.SDL_MOUSEWHEEL_FLIPPED)
                {
                    mousewheelDeltaX *= -1;
                    mousewheelDeltaY *= -1;
                }
            }
        }
    }
}
