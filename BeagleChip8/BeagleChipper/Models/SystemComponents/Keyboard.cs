using System;
using System.Collections.ObjectModel;

namespace BeagleChipper.Models.SystemComponents
{
    public class Keyboard
    {
        /// <summary>
        /// The original hardware for executing CHIP-8 software made use of a hex keyboard, which
        /// Had input in the range of 0 to F.
        /// </summary>
        public ReadOnlyCollection<byte> KeyboardState => new ReadOnlyCollection<byte>(_keyboardState);

        private const byte KeyboardStateSize = 0xF;

        private byte[] _keyboardState;

        public Keyboard()
        {
            InitializeMemoryRegions();
        }

        public void SetKeyboardState(byte keyboardRegister, byte value)
        {
            if (keyboardRegister < KeyboardStateSize)
                _keyboardState[keyboardRegister] = value;
            else
                throw new ArgumentOutOfRangeException($"KeyboardRegister has to be less than {KeyboardStateSize}");
        }

        private void InitializeMemoryRegions()
        {
            _keyboardState = new byte[KeyboardStateSize];
        }
    }
}