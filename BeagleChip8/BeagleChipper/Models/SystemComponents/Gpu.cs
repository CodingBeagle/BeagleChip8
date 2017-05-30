using System;
using System.Collections.ObjectModel;
using BeagleSdl;

namespace BeagleChipper.Models.SystemComponents
{
    /// <summary>
    /// 
    /// </summary>
    public class Gpu
    {
        /// <summary>
        /// The original display resolution of the hardware used to execute CHIP-8 software was 64x32 pixels
        /// With a monochrome color. We will simply upscale the rendered image through SDL.
        /// 
        /// Sprites drawn through CHIP-8 will ALWAYS be 8 pixels wide. Although the height (the amount of stacked pixels rows)
        /// Can vary between 1 to 15 pixels.
        /// </summary>
        public ReadOnlyCollection<byte> Memory => new ReadOnlyCollection<byte>(_memory);

        private const ushort DisplayMemorySize = 64 * 32;

        private byte[] _memory;

        public Gpu()
        {
            InitializeMemoryRegions();
        }

        public void ClearDisplayMemory()
        {
            InitializeMemoryRegions();
        }

        public void SetDisplayMemory(ushort memoryIndex, byte value)
        {
            if (memoryIndex < DisplayMemorySize)
                _memory[memoryIndex] = value;
            else
                throw new ArgumentException($"The memory index must not exceed {DisplayMemorySize}");
        }

        private void InitializeMemoryRegions()
        {
            _memory = new byte[DisplayMemorySize];
        }
    }
}