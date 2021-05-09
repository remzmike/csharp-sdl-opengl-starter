using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Security;

namespace test1
{
    public static class WinApi
    {
        // wish: some version of sdl has SDL_SetSystemTimerResolution for these?
        // https://github.com/libsdl-org/SDL/blob/33598563b35067417c2fed8fab03c503438ce6f6/src/timer/windows/SDL_systimer.c
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
        public static extern uint TimeBeginPeriod(uint uMilliseconds);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
        public static extern uint TimeEndPeriod(uint uMilliseconds);

        public static uint TIMERR_BASE = 96;
        public static uint TIMERR_NOERROR = 0; /* no error */
        public static uint TIMERR_NOCANDO = TIMERR_BASE + 1; /* request not completed */
        public static uint TIMERR_STRUCT = TIMERR_BASE + 33; /* time struct size */
    }
}
