using System;
using System.Collections.ObjectModel;

namespace BeagleChipper.Models.SystemComponents
{
    // TODO: Perhaps see if Ram class can be made to be accessed as array?
    /// <summary>
    /// 
    /// </summary>
    public class Ram
    {
        /// <summary>
        /// The hardware which ran CHIP-8 was usually 4K systems, meaning they had 4096 memory locations.
        /// Each memory location was 1 byte long.
        /// </summary>
        public ReadOnlyCollection<byte> Memory => new ReadOnlyCollection<byte>(_memory);

        private byte[] _memory;

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

        private const ushort MemorySize = 4096;

        public Ram()
        {
            InitializeMemoryRegions();
            LoadFontSpritesIntoMemory();
        }

        public void SetMemory(ushort memoryLocation, byte value)
        {
            if (memoryLocation < MemorySize)
                _memory[memoryLocation] = value;
            else
                throw new ArgumentException($"The memory location must not exceed {MemorySize}");
        }

        public ushort GetProgramInstruction(ushort programCounter)
        {
            return (ushort) ((Memory[programCounter] << 8) | (Memory[programCounter + 1]));
        }

        public void LoadRangeToRam(byte[] range)
        {
            Array.Copy(range, 0, _memory, 0x200, range.Length);
        }

        private void InitializeMemoryRegions()
        {
            _memory = new byte[MemorySize];
        }

        private void LoadFontSpritesIntoMemory()
        {
            Array.Copy(_fontset, _memory, _fontset.Length);
        }
    }
}