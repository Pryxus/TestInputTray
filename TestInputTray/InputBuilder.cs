using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TestInputTray
{
    internal class InputBuilder : IEnumerable<SimMouse>
    {
        private readonly List<SimMouse> _inputList = new List<SimMouse>();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint numberOfInputs, SimMouse[] inputs, int sizeOfInputStructure);

        public SimMouse[] ListToArray()
        {
            return _inputList.ToArray();
        }

        public IEnumerator<SimMouse> GetEnumerator()
        {
            return _inputList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public InputBuilder AddToMousePos(int x, int y)
        {
            var movement = new SimMouse
            {
                Type = 0,
                Data = new MouseHardware
                {
                    Mouse = new Mouseinput
                    {
                        X = x,
                        Y = y,
                        Scan = 0,
                        Flags = 0x0001,
                        Time = 0,
                        DwExtraInfo = IntPtr.Zero
                    }
                }
            };

            _inputList.Add(movement);

            return this;
        }

        public void MoveMouseBy(int pixelDeltaX, int pixelDeltaY)
        {
            AddToMousePos(pixelDeltaX, pixelDeltaY);
            var mouseArray = ListToArray();

            if (mouseArray.Length == 0)
            {
                throw new InvalidOperationException("Empty array received");
            }

            var success = SendInput((uint)mouseArray.Length, mouseArray, Marshal.SizeOf<SimMouse>());
            if (success != mouseArray.Length)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Library Access Error. Error Code: {errorCode}");
            }
        }
    }

    internal struct SimMouse
    {
        public uint Type;
        public MouseHardware Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct MouseHardware
    {
        [FieldOffset(0)]
        public Mouseinput Mouse;
    }

    internal struct Mouseinput
    {
        public int X;
        public int Y;
        public uint Scan;
        public uint Flags;
        public uint Time;
        public IntPtr DwExtraInfo;
    }
}