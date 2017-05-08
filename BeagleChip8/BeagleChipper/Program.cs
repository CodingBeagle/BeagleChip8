﻿using System;
using System.IO;
using BeagleChipper.Models;
using BeagleSdl;

namespace BeagleChipper
{
    class Program
    {
        private static readonly byte[] Fontset = new byte[80]
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

        /*
         * CHIP-8 was most commonly implemented on 4K systems, such as the Cosmac VIP and the
         * Telmac 1800. These machines had 4096 memory locations, all of which are 1 byte (8 bits).
         * 
         * The CHIP-8 interpreter itself occupies the first 512 (0x200) bytes of the memory space on these machines,
         * And therefore most programs written for the original system begin at memory location 512 and do not
         * Access any of the memory below this location.
         * 
         * The uppermost 256 bytes (0xF00-0x1FF) are reserved for display refresh.
         * The 96 bytes below that (0xEA0-0xEFF) were reserved for call stack, internal use
         * And other variables.
         */
        private static readonly byte[] Memory = new byte[4096];

        /*
         * The stack is only used to store return addresses when subroutines are called.
         * The original 1802 version allocated 48 bytes for up to 12 levels of nesting.<
         * Modern implementations normally have at least 16 levels.
         */
        private static readonly ushort[] Stack = new ushort[16];

        /*
         * The stack pointer is used to point to the topmost level of the stack.
         */
        private static byte StackPointer = 0;

        /*
         * CHIP-8 has 16 8-bit data registers named from V0 to VF.
         * The VF register doubles as a flag for some instructions, thus this
         * Register should be avoided (i.e, don't use it). VF is also used
         * As the carry flag. While in subtraction, it is the "not borrow" flag.
         * 
         * In the draw instruction register VF is set upon pixel collision.
         * 
         */
        private static readonly byte[] Registers = new byte[16];

        /*
         * The address register, named I, is 16 bits wide and is used with several opcodes that
         * Involve memory operations.
         */
        private static ushort I = 0;

        /*
         * The CHIP-8 has two special purpose 8-bit registers for delay and sound timers.
         * When these registers are non-zero, they are automatically decremented at  rate of 60Hz.
         * 
         * The delay timer is active whenever the delay timer register is non-zero. This timer does nothing more
         * Than substrat 1 from the value of DT (Delay Timer Register) at a rate of 60Hz. When DT reaches 0,
         * It deactivates.
         * 
         * The sound timer is active wehenever the sound timer register (ST) is non-zero. This timer also decrements at a
         * Rate of 60Hz, however, as long as ST's value is greater than zero, the CHIP-8 buzzer will sound. When ST reaches
         * Zero, the sound timer deactivates. The sound produced by the CHIP-8 has only one tone.
         */
        private static byte DelayTimer = 0;
        private static byte SoundTimer = 0;

        /*
         * The program counter is used to store the currently executing address.
         */
        private static ushort ProgramCounter = 0;

        /*
         * 
         */
        private static readonly byte[] DisplayMemory = new byte[64 * 32];

        static void Main(string[] args)
        {
            /*
             * Reset program counter
             * 
             * Most programs written for the original system will begin at memory location 512 (0x200), and will never
             * Access any memory below this location, as it is reserved for system-related things.
             */
            ProgramCounter = 0x200;

            // Clear address register
            I = 0;

            // Clear stack pointer
            StackPointer = 0;
            
            // Clear registers
            Array.Clear(Registers, 0, Registers.Length);

            // Clear stack
            Array.Clear(Stack, 0, Stack.Length);

            // Clear display
            Array.Clear(DisplayMemory, 0, DisplayMemory.Length);

            // Clear memory
            Array.Clear(Memory, 0, Memory.Length);

            // Load fontset
            Array.Copy(Fontset, Memory, Fontset.Length);

            // Load program
            LoadProgram("Space Invaders.ch8", Memory);

            // Initialize SDL
            Sdl.Init((int)InitializationFlags.Video);

            // Create window
            IntPtr mainWindow = Sdl.CreateWindow("BeagleChipper!", 200, 200, 640, 320, (int)WindowFlags.OpenGl);
            if (mainWindow == IntPtr.Zero)
            {
                Console.WriteLine("Failed to initialize main window! :(");
                return;
            }

            // Create renderer
            IntPtr mainRenderer = Sdl.CreateRenderer(mainWindow, -1, (int)RendererFlags.Accelerated);
            if (mainRenderer == IntPtr.Zero)
            {
                Console.WriteLine("Failed to initialize renderer! :(");
                return;
            }

            // Set Render draw color
            Sdl.SetRenderDrawColor(mainRenderer, 0, 0, 0, 255);
            
            // Loop variables
            double t = 0.0;
            const double dt = 1.0 / 60.0;

            double currentTime = Sdl.GetTicks() / 1000.0;
            double accumulator = 0.0;

            SDLRect pixel = new SDLRect { H = 10, W = 10, X = 32, Y = 32 };
            
            bool isRunning = true;
            while (isRunning)
            {
                double newTime = Sdl.GetTicks() / 1000.0;
                double frameTime = newTime - currentTime;
                currentTime = newTime;

                accumulator += frameTime;

                SdlEvent currentEvent;
                while (Sdl.PollEvent(out currentEvent) == 1)
                {
                    if (currentEvent.Type == (int)EventType.Quit)
                        isRunning = false;
                }

                while (accumulator >= dt)
                {
                    accumulator -= dt;
                    t += dt;
                }

                // Update screen
                Sdl.RenderClear(mainRenderer);

                Sdl.SetRenderDrawColor(mainRenderer, 255, 255, 255, 255);

                Sdl.RenderDrawRect(mainRenderer, ref pixel);
                Sdl.RenderFillRect(mainRenderer, ref pixel);

                Sdl.SetRenderDrawColor(mainRenderer, 0, 0, 0, 255);

                Sdl.RenderPresent(mainRenderer);
            }

            // Terminate SDL
            Sdl.Quit();
        }

        private static void ExecuteCycle()
        {
            /*
            * Fetch opcode.
            * 
            * The CHIP-8 was implemented on hardware with 4096 memory locations, all of which were 1 byte. Each opcode of the
            * CHIP-8 instruction set is 2 bytes long.
            */
            ushort opcode = (ushort)((Memory[ProgramCounter] << 8) | (Memory[ProgramCounter + 1]));

            // Handle the fetched operation code
            HandleOperationCode(opcode);

            /*
                * Since every instruction is 2 bytes long, we need to increment the program counter by two
                * After every executed opcode.
                * 
                * In some cases, such as when an instruction needs to be skipped, the Program Counter will have
                * Been increased by 4 in total during a single instruction.
                */
            ProgramCounter += 2;

            // Update timers
            if (DelayTimer > 0)
                DelayTimer--;

            if (SoundTimer > 0)
                SoundTimer--;
        }

        private static void HandleOperationCode(ushort operationCode)
        {
            switch (operationCode & 0xF000)
            {
                case 0x0000:
                    switch (operationCode)
                    {
                        case (int)OperationCode.ClearDisplay:
                            ClearDisplay();
                            break;
                        case 0x00EE: // Return from a subroutine
                            ReturnFromSubroutine(operationCode);
                            break;
                    }
                    break;
                case 0x1000: // Jump to location
                    var location = (ushort)(operationCode & 0x0FFF);
                    ProgramCounter = location;
                    break;
                case 0x2000: // Call subroutine
                    JumpToSubRoutine(operationCode);
                    break;
                case 0x3000: // Skip next instruction if equal to register
                    SkipInstructionIfRegisterValueEqualToCompareValue(operationCode);
                    break;
                case 0x4000: // Skip next instruction if not equal to register
                    SkipInstructionIfRegisterValueNotEqualToCompareValue(operationCode);
                    break;
                case 0x5000: // Skip next instruction of the two registers are equal to each other
                    SkipInstructionIfRegistersAreEqual(operationCode);
                    break;
                case 0x6000: // SET VALUE TO REGISTER
                    SetRegisterToValue(operationCode);
                    break;
                case 0x7000: // ADD VALUE TO REGISTER
                    AddValueToRegister(operationCode);
                    break;
                case 0x8000:
                    switch (operationCode & 0x000F)
                    {
                        case 0x0000: // STORE VALUE OF REGISTER Y IN REGISTER V
                            CopyRegisterValueToOtherRegister(operationCode);
                            break;
                        case 0x0001: // Perform bitwise or on Vx and Vy and store the result in Vx
                            BitwiseOr(operationCode);
                            break;
                        case 0x0002: // Perform bitwise AND on Vx and Vy and store the result in Vx
                            BitwiseAnd(operationCode);
                            break;
                        case 0x0003: // Perform bitwise EXCLUSIVE OR on Vx and Vy and store the result in Vx
                            ExclusiveOr(operationCode);
                            break;
                        case 0x0004: // Add Vx and Vy and store result in Vx. If the result is greater than 255, VF is set to 1, otherwise 0.
                            AddAndCarry(operationCode);
                            break;
                        case 0x0005: // Subtract Vy from Vx. If Vx > 
                            SubtractAndBorrow(operationCode);
                            break;
                        case 0x0006:
                            SHR(operationCode);
                            break;
                        case 0x0007:
                            SubtractAndNotBorrow(operationCode);
                            break;
                        case 0x000E:
                            ShiftLeft(operationCode);
                            break;
                    }
                    break;
                case 0x9000:
                    SkipInstructionIfVxNotEqualVy(operationCode);
                    break;
                case 0xA000:
                    SetIToAddress(operationCode);
                    break;
                case 0xB000:
                    JumpToAddress(operationCode);
                    break;
                case 0xC000: // Random
                    Rand(operationCode);
                    break;
                case 0xD000: // Display sprite at coordinate
                    DrawSprite(operationCode);
                    break;
                case 0xE000:
                    switch (operationCode & 0x000F)
                    {
                        case 0x000E:
                            SkipIfKeyPressed(operationCode);
                            break;
                        case 0x0001:
                            SkipIfKeyNotPressed(operationCode);
                            break;
                    }
                    break;
                case 0xF000:
                    switch (operationCode & 0x00FF)
                    {
                        case 0x0007:
                            SetVxToDelayTimerValue(operationCode);
                            break;
                        case 0x000A:
                            GetKeyPressAndStoreInVx(operationCode);
                            break;
                        case 0x0015:
                            SetDelayTimerToVx(operationCode);
                            break;
                        case 0x0018:
                            SetSoundTimerToVx(operationCode);
                            break;
                        case 0x001E:
                            AddVxToI(operationCode);
                            break;
                        case 0x0029:
                            SetIToLocationOfCharacterSprite(operationCode);
                            break;
                        case 0x0033:
                            Bcd(operationCode);
                            break;
                        case 0x0055:
                            RegisterDump(operationCode);
                            break;
                        case 0x0065:
                            RegisterLoad(operationCode);
                            break;
                    }
                    break;
            }
        }

        /*
         * For the CHIP-8, every sprite will ALWAYS have a width of 8 pixels.
         * What can vary is the height (the amount of rows) each pixel have.
         * 
         * CHIP-8 sprites may be up to 15 bytes in size. This makes it possible
         * To have a sprite of maximum size 8x15.
         * 
         * Programs can also refer to a group of sprites which represent the hexadecimal digits
         * 0 through F. These sprites are all 5 bytes long, that is 8x5 pixels.
         * 
         * This data should be stored in the area of CHIP-8 memory 0x000 to 0x1FF.
         */
        private static void DrawSprite(ushort operationCode)
        {
            byte registerX = (byte)((operationCode & 0x0F00) >> 8);
            byte registerY = (byte)((operationCode & 0x00F0) >> 4);

            byte coordinateX = Registers[registerX];
            byte coordinateY = Registers[registerY];
            byte height = (byte)(operationCode & 0x000F);

            // Reset VF
            Registers[0xF] = 0;

            // Read sprite from memory which needs to be rendered
            byte[] sprite = new byte[height];
            Array.Copy(Memory, I, sprite, 0, height);

            for (int yLine = 0; yLine < height; yLine++)
            {
                byte pixelRow = Memory[I + yLine];
                for (int xLine = 0; xLine < 8; xLine++)
                {
                    byte pixel = (byte)((pixelRow & (0x80 >> xLine)) >> (7 - xLine));

                    byte displayPixel = DisplayMemory[((coordinateY + yLine) * 8) + (coordinateX + xLine)];

                    if (displayPixel == 1 && pixel == 1)
                    {
                        DisplayMemory[((coordinateY + yLine) * 8) + (coordinateX + xLine)] = 0;
                        Registers[0xF] = 1;
                    }
                    else
                    {
                        DisplayMemory[((coordinateY + yLine) * 8) + (coordinateX + xLine)] = pixel;
                        Registers[0xF] = 0;
                    }
                }
            }
        }

        private static void ClearDisplay()
        {
            Array.Clear(DisplayMemory, 0, DisplayMemory.Length);
        }

        private static void ReturnFromSubroutine(ushort operationCode)
        {
            ProgramCounter = Stack[StackPointer];
            StackPointer--;
        }

        private static void JumpToSubRoutine(ushort operationCode)
        {
            StackPointer++;
            Stack[StackPointer] = ProgramCounter;
            ProgramCounter = (ushort)(operationCode & 0x0FFF);
        }
        
        private static void SetRegisterToValue(ushort operationCode)
        {
            byte value = (byte)(operationCode & 0x00FF);
            byte register = (byte) ((operationCode & 0x0F00) >> 8);

            Registers[register] = value;
        }

        private static void AddValueToRegister(ushort operationCode)
        {
            byte value = (byte)(operationCode & 0x00FF);
            byte register = (byte)((operationCode & 0x0F00) >> 8);

            Registers[register] += value;
        }

        private static void CopyRegisterValueToOtherRegister(ushort operationCode)
        {
            byte registerY = (byte) ((operationCode & 0x00F0) >> 4);
            byte registerX = (byte) ((operationCode & 0x0F00) >> 8);

            Registers[registerX] = Registers[registerY];
        }

        private static void SkipInstructionIfRegisterValueEqualToCompareValue(ushort operationCode)
        {
            ushort register = (byte)((operationCode & 0x0F00) >> 8);
            ushort compareValue = (byte)(operationCode & 0x00FF);

            if (Registers[register] == compareValue)
            {
                ProgramCounter += 2;
            }
        }

        private static void SkipInstructionIfRegisterValueNotEqualToCompareValue(ushort operationCode)
        {
            ushort register = (byte)((operationCode & 0x0F00) >> 8);
            ushort compareValue = (byte)(operationCode & 0x00FF);

            if (Registers[register] != compareValue)
                ProgramCounter += 2;
        }

        private static void SkipInstructionIfRegistersAreEqual(ushort operationCode)
        {
            ushort registerX = (byte)( (operationCode & 0x0F00) >> 8);
            ushort registerY = (byte) ((operationCode & 0x00F0) >> 4);

            if (Registers[registerX] == Registers[registerY])
                ProgramCounter += 2;
        }

        private static void BitwiseOr(ushort operationCode)
        {
            byte registerX = (byte)((operationCode & 0x0F00) >> 8);
            byte registerY = (byte)((operationCode & 0x00F0) >> 4);

            byte registerXValue = Registers[registerX];
            byte registerYValue = Registers[registerY];

            registerXValue = (byte)(registerXValue | registerYValue);

            Registers[registerX] = registerXValue;
        }

        private static void BitwiseAnd(ushort operationCode)
        {
            byte registerX = (byte)((operationCode & 0x0F00) >> 8);
            byte registerY = (byte)((operationCode & 0x00F0) >> 4);

            byte registerXValue = Registers[registerX];
            byte registerYValue = Registers[registerY];

            registerXValue = (byte)(registerXValue & registerYValue);

            Registers[registerX] = registerXValue;
        }

        private static void ExclusiveOr(ushort operationCode)
        {
            byte registerX = (byte)((operationCode & 0x0F00) >> 8);
            byte registerY = (byte)((operationCode & 0x00F0) >> 4);

            byte registerXValue = Registers[registerX];
            byte registerYValue = Registers[registerY];

            registerXValue = (byte)(registerXValue ^ registerYValue);

            Registers[registerX] = registerXValue;
        }

        private static void AddAndCarry(ushort operationCode)
        {
            byte registerX = (byte)((operationCode & 0x0F00) >> 8);
            byte registerY = (byte)((operationCode & 0x00F0) >> 4);

            if (Registers[registerX] > (0xFF - Registers[registerY]))
                Registers[0xF] = 1;
            else
                Registers[0xF] = 0;

            Registers[registerX] += Registers[registerY];
        }

        private static void SubtractAndBorrow(ushort operationCode)
        {
            byte registerX = (byte)((operationCode & 0x0F00) >> 8);
            byte registerY = (byte)((operationCode & 0x00F0) >> 4);

            if (Registers[registerX] > Registers[registerY])
                Registers[0xF] = 1;
            else
                Registers[0xF] = 0;

            Registers[registerX] -= Registers[registerY];
        }

        private static void SubtractAndNotBorrow(ushort operationCode)
        {
            byte registerX = (byte)((operationCode & 0x0F00) >> 8);
            byte registerY = (byte)((operationCode & 0x00F0) >> 4);

            if (Registers[registerY] > Registers[registerX])
                Registers[0xF] = 1;
            else
                Registers[0xF] = 0;

            Registers[registerX] -= Registers[registerY];
        }

        private static void SHR(ushort operationCode)
        {
            byte registerX = (byte) ((operationCode & 0x0F00) >> 8);

            if ((Registers[registerX] & 0b00000001) == 1)
                Registers[0xF] = 1;
            else
                Registers[0xF] = 0;

            Registers[registerX] /= 2;
        }

        private static void ShiftLeft(ushort operationCode)
        {
            byte registerX = (byte)((operationCode & 0x0F00) >> 8);
            Registers[0xF] = (byte)(Registers[registerX] & 0b1000_0000);
            Registers[registerX] = (byte)(Registers[registerX] << 1);
        }

        private static void SkipInstructionIfVxNotEqualVy(ushort operationCode)
        {
            ushort registerX = (byte)((operationCode & 0x0F00) >> 8);
            ushort registerY = (byte) ((operationCode & 0x00F0) >> 4);

            if (Registers[registerX] != Registers[registerY])
                ProgramCounter += 2;
        }

        private static void SetIToAddress(ushort operationCode)
        {
            ushort registerValue = (ushort) (operationCode & 0x0FFF);
            I = registerValue;
        }

        private static void JumpToAddress(ushort operationCode)
        {
            ushort address = (ushort)(operationCode & 0x0FFF);
            ProgramCounter = (ushort)(Registers[0] + address);
        }

        private static void Rand(ushort opearionCode)
        {
            byte registerX = (byte)((opearionCode & 0x0F00) >> 8);
            byte theValue = (byte)(opearionCode & 0x00FF);
            Random newRand = new Random();
            Registers[registerX] = (byte)(newRand.Next(0, 255) & theValue);
        }

        private static void SkipIfKeyPressed(ushort operationCode)
        {
            Console.WriteLine("SkipIfKeyPressed Instruction not implemented...");
        }

        private static void SkipIfKeyNotPressed(ushort operationCode)
        {
            Console.WriteLine("SkipIfKeyNotPressed Instruction not implemented...");
        }

        private static void SetVxToDelayTimerValue(ushort operationCode)
        {
            byte registerX = (byte)((operationCode & 0x0F00) >> 8);

            Registers[registerX] = DelayTimer;
        }

        private static void GetKeyPressAndStoreInVx(ushort operationCode)
        {
            Console.WriteLine("GetKeyPressAndStoreInVx Instruction not implemented...");
        }

        private static void SetDelayTimerToVx(ushort operationCode)
        {
            byte registerX = (byte)((operationCode & 0x0F00) >> 8);
            DelayTimer = Registers[registerX];
        }

        private static void SetSoundTimerToVx(ushort operationCode)
        {
            byte registerX = (byte)((operationCode & 0x0F00) >> 8);
            SoundTimer = Registers[registerX];
        }

        private static void AddVxToI(ushort operationCode)
        {
            byte registerX = (byte)((operationCode & 0x0F00) >> 8);
            I += registerX;
        }

        private static void Bcd(ushort operationCode)
        {
            byte registerX = (byte)((operationCode & 0x0F00) >> 8);
            byte registerXValue = Registers[registerX]; 

            byte mostSignificantDigit = (byte)(registerXValue / 100);
            byte middleDigit = (byte)((registerXValue % 100) / 10);
            byte onesDigit = (byte)((registerXValue % 100) % 10);

            // Store them in memory
            Memory[I] = mostSignificantDigit;
            Memory[I + 1] = middleDigit;
            Memory[I + 2] = onesDigit;
        }

        private static void RegisterDump(ushort operationCode)
        {
            byte registerX = (byte)((operationCode & 0x0F00) >> 8);

            for (byte x = 0; x <= registerX; x++)
            {
                Memory[I + x] = Registers[x];
            }
        }

        private static void RegisterLoad(ushort operationCode)
        {
            byte registerX = (byte)((operationCode & 0x0F00) >> 8);

            for (byte x = 0; x <= registerX; x++)
            {
                Registers[x] = Memory[I + x];
            }
        }

        private static void SetIToLocationOfCharacterSprite(ushort operationCode)
        {
            byte registerX = (byte)((operationCode & 0x0F00) >> 8);

            byte registerXValue = Registers[registerX];

            I = (ushort)(registerXValue * 0x05);
        }

        private static void LoadProgram(string filepath, byte[] memory)
        {
            byte[] programData = File.ReadAllBytes(filepath);

            Array.Copy(programData, 0, memory, 0x200, programData.Length);
        }
    }
}