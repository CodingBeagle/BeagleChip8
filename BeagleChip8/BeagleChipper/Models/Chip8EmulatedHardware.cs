using System;
using System.Collections.ObjectModel;

namespace BeagleChipper.Models
{
    /// <summary>
    /// This class represents the emulated hardware used to facilitate execution of
    /// CHIP-8 software. This includes 
    /// </summary>
    public class Chip8EmulatedHardware
    {
        /// <summary>
        /// The hardware which ran CHIP-8 was usually 4K systems, meaning they had 4096 memory locations.
        /// Each memory location was 1 byte long.
        /// </summary>
        public ReadOnlyCollection<byte> Memory => new ReadOnlyCollection<byte>(_memory);

        /// <summary>
        /// The original display resolution of the hardware used to execute CHIP-8 software was 64x32 pixels
        /// With a monochrome color. We will simply upscale the rendered image through SDL.
        /// 
        /// Sprites drawn through CHIP-8 will ALWAYS be 8 pixels wide. Although the height (the amount of stacked pixels rows)
        /// Can vary between 1 to 15 pixels.
        /// </summary>
        public ReadOnlyCollection<byte> DisplayMemory => new ReadOnlyCollection<byte>(_displayMemory);

        /// <summary>
        /// The stack for the CHIP-8 is bare-bones, meaning that it is only used to store the program counter
        /// As a return address when a subroutine is called.There are no fancy stack-frames which also store
        /// Local variables and other sorts of temporary storage.
        /// </summary>
        public ReadOnlyCollection<ushort> Stack => new ReadOnlyCollection<ushort>(_stack);

        /// <summary>
        /// CHIP-8 makes use of 16 8-bit registers.
        /// They are named V0-VF.
        /// The VF register is used as a flag for some instructions, thus this register should
        /// Not be used by user programs.
        /// 
        /// For example, for the draw instruction, register VF is used for indicating collision.
        /// It is also used as a borrow-flag for arithmetic instructions.
        /// </summary>
        public ReadOnlyCollection<byte> Registers => new ReadOnlyCollection<byte>(_registers);

        /// <summary>
        /// The original hardware for executing CHIP-8 software made use of a hex keyboard, which
        /// Had input in the range of 0 to F.
        /// </summary>
        public ReadOnlyCollection<byte> KeyboardState => new ReadOnlyCollection<byte>(_keyboardState);

        /// <summary>
        /// The stack pointer is simply used to point to the top of the stack.
        /// </summary>
        public byte StackPointer { get; private set; }

        /// <summary>
        /// The address register, named I, is 16 bits wide and is used with several opcodes that involve
        /// Memory operations.
        /// </summary>
        public ushort I { get; private set; }

        /// <summary>
        /// The delay timer will be counted down by 1 at a rate of 60Hz when it is non-zero.
        /// </summary>
        public byte DelayTimer { get; private set; }

        /// <summary>
        /// The sound timer will be counted down by 1 at a rate of 60Hz when it is non-zero.
        /// While the sound timer's value is greater than 0, a single tone will sound. The pitch
        /// Of this tone is left to the implementation of the CHIP-8 hardware.
        /// </summary>
        public byte SoundTimer { get; private set; }

        /// <summary>
        /// The program counter simply stores the address location of the currently
        /// Executing instruction.
        /// </summary>
        public ushort ProgramCounter { get; private set; }

        private byte[] _memory;

        private byte[] _displayMemory;

        private ushort[] _stack;

        private byte[] _registers;

        private byte[] _keyboardState;

        private const ushort MemorySize = 4096;

        private const ushort DisplayMemorySize = 64 * 32;

        private const byte RegisterSize = 16;

        private const byte StackSize = 16;

        private readonly byte[] _fontset = new byte[80]
        {
            0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
            0x20, 0x60, 0x20, 0x20, 0x70, // 1
            0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
            0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
            0x90, 0x90, 0xF0, 0x10, 0x10, // 4
            0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
            0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
            0xF0, 0x10, 0x20, 0x40, 0x40, // 7
            0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
            0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
            0xF0, 0x90, 0xF0, 0x90, 0x90, // A
            0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
            0xF0, 0x80, 0x80, 0x80, 0xF0, // C
            0xE0, 0x90, 0x90, 0x90, 0xE0, // D
            0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
            0xF0, 0x80, 0xF0, 0x80, 0x80  // F
        };

        public Chip8EmulatedHardware()
        {
            InitializeSystemVariables();

            InitializeMemoryRegions();

            LoadFontSpritesIntoMemory();
        }

        private void InitializeSystemVariables()
        {
            /*
            * Most programs written for the original systems begins at memory location 512 (0x200), and
            * Will never access memory below this location. Originally, the lower memory locations were
            * Reserved for the CHIP-8 interpreter. We will reserve for storing font sprites.
            */
            ProgramCounter = 0x200;

            I = 0;

            StackPointer = 0;
        }

        public void SetRegisterValue(byte registerIndex, byte value)
        {
            if (registerIndex < 0xF)
                _registers[registerIndex] = value;
            else
                throw new ArgumentException("The register index must not exceed 15.");
        }

        public void SetMemory(ushort memoryLocation, byte value)
        {
            if (memoryLocation < 4096)
                _memory[memoryLocation] = value;
            else
                throw new ArgumentException("The memory location must not exceed 4095");
        }

        public void SetDisplayMemory(ushort memoryLocation, byte value)
        {
        }

        private void InitializeMemoryRegions()
        {
            _registers = new byte[16];

            _stack = new ushort[16];

            _memory = new byte[4096];

            _displayMemory = new byte[64 * 32];

            _keyboardState = new byte[16];
        }

        private void LoadFontSpritesIntoMemory()
        {
            Array.Copy(_fontset, _memory, _fontset.Length);
        }
    }
}