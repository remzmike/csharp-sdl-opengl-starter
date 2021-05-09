using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

// https://stackoverflow.com/questions/1897555/what-is-the-equivalent-of-memset-in-c
// https://stackoverflow.com/a/25808955
namespace test1
{
    public static class Memory
    {
        private static Action<IntPtr, byte, int> SetBytesDelegate;
        private static Action<IntPtr, bool, int> SetBoolsDelegate;

        static Memory()
        {
            { // byte
                var method = new DynamicMethod("MemsetBytes", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard,
                    null, new[] { typeof(IntPtr), typeof(byte), typeof(int) }, typeof(Memory), true);

                var generator = method.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldarg_2);
                generator.Emit(OpCodes.Initblk);
                generator.Emit(OpCodes.Ret);

                SetBytesDelegate = (Action<IntPtr, byte, int>)method.CreateDelegate(typeof(Action<IntPtr, byte, int>));
            }

            { // bool
                var method = new DynamicMethod("MemsetBools", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard,
                    null, new[] { typeof(IntPtr), typeof(bool), typeof(int) }, typeof(Memory), true);

                var generator = method.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldarg_2);
                generator.Emit(OpCodes.Initblk);
                generator.Emit(OpCodes.Ret);

                SetBoolsDelegate = (Action<IntPtr, bool, int>) method.CreateDelegate(typeof(Action<IntPtr, bool, int>));
            }
        }

        public static void SetBytes(byte[] array, byte what, int length)
        {
            var gcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
            SetBytesDelegate(gcHandle.AddrOfPinnedObject(), what, length);
            gcHandle.Free();
        }

        public static void SetBools(bool[] array, bool what, int length)
        {
            var gcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
            SetBoolsDelegate(gcHandle.AddrOfPinnedObject(), what, length);
            gcHandle.Free();
        }

        /*public static void MemsetEquivalent(byte[] array, byte what, int length)
        {
            for (var i = 0; i < length; i++)
            {
                array[i] = what;
            }
        }*/

    }
}
