using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BeagleChipper.Models.SystemComponents
{
    /// <summary>
    /// 
    /// </summary>
    public class Cpu
    {
        /// <summary>
        /// CHIP-8 makes use of 16 8-bit registers.
        /// They are named V0-VF.
        /// The VF register is used as a flag for some instructions, thus this register should
        /// Not be used by user programs.
        /// 
        /// For example, for the draw instruction, register VF is used for indicating collision.
        /// It is also used as a borrow-flag for arithmetic instructions.
        /// </summary>
        public ReadOnlyCollection<byte> GeneralPurposeRegisters => new ReadOnlyCollection<byte>(_generalPurposeRegisters);

        /// <summary>
        /// The stack for the CHIP-8 is bare-bones, meaning that it is only used to store the program counter
        /// As a return address when a subroutine is called. There are no fancy stack-frames which also store
        /// Local variables and other sorts of temporary storage. Typically, modern implementations will support
        /// At minimum 16 levels.
        /// </summary>
        public ReadOnlyCollection<ushort> Stack => new ReadOnlyCollection<ushort>(_stack.ToArray());

        /// <summary>
        /// The address register, named I, is 16 bits wide and is used with several opcodes that involve
        /// Memory operations.
        /// </summary>
        public ushort I { get; set; }

        /// <summary>
        /// The program counter simply stores the address location of the currently
        /// Executing instruction.
        /// </summary>
        public ushort ProgramCounter { get; set; }

        /// <summary>
        /// The delay timer will be counted down by 1 at a rate of 60Hz when it is non-zero.
        /// </summary>
        public byte DelayTimer { get; set; }

        /// <summary>
        /// The sound timer will be counted down by 1 at a rate of 60Hz when it is non-zero.
        /// While the sound timer's value is greater than 0, a single tone will sound. The pitch
        /// Of this tone is left to the implementation of the CHIP-8 hardware.
        /// </summary>
        public byte SoundTimer { get; set; }

        private byte[] _generalPurposeRegisters;

        private Stack<ushort> _stack;

        private const byte RegisterSize = 16;

        public Cpu()
        {
            InitializeRegisters();
        }

        public void SetGeneralPurposeRegisterValue(byte registerIndex, byte value)
        {
            if (registerIndex < RegisterSize)
                _generalPurposeRegisters[registerIndex] = value;
            else
                throw new ArgumentException($"The register index must not exceed {RegisterSize-1}");
        }

        public void PushAddressToStack(ushort address)
        {
            _stack.Push(address);
        }

        public ushort PopAddressFromStack()
        {
            return _stack.Pop();
        }

        public void GoToNextInstruction()
        {
            ProgramCounter += 2;
        }

        public void SkipNextInstruction()
        {
            ProgramCounter += 4;
        }

        private void InitializeRegisters()
        {
            /*
            * Most programs written for the original systems begins at memory location 512 (0x200), and
            * Will never access memory below this location. Originally, the lower memory locations were
            * Reserved for the CHIP-8 interpreter. We will reserve for storing font sprites.
            */
            ProgramCounter = 0x200;

            I = 0;
            _generalPurposeRegisters = new byte[RegisterSize];
            _stack = new Stack<ushort>();
            DelayTimer = 0;
            SoundTimer = 0;
        }
    }
}