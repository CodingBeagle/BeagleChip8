using System;
using System.IO;
using BeagleChipper.Models.SystemComponents;
using BeagleSdl;

namespace BeagleChipper.Models
{
    public class System
    {
        private readonly Cpu _cpu;
        private readonly Gpu _gpu;
        private readonly Ram _ram;
        private readonly Keyboard _keyboard;
        private readonly IntPtr _renderer;
        
        public System(IntPtr renderer,
                        Cpu cpu,
                        Gpu gpu,
                        Ram ram,
                        Keyboard keyboard)
        {
            _cpu = cpu;
            _gpu = gpu;
            _ram = ram;
            _keyboard = keyboard;
            _renderer = renderer;
        }

        public void ExecuteInstructionCycle()
        {
            ushort operationCode = _ram.GetProgramInstruction(_cpu.ProgramCounter);
            HandleOperationCode(operationCode);
        }

        public void LoadProgram(string filepath)
        {
            byte[] programData = File.ReadAllBytes(filepath);
            _ram.LoadRangeToRam(programData);
        }

        private void HandleOperationCode(ushort operationCode)
        {
            switch (operationCode & 0xF000)
            {
                case 0x0000:
                    switch (operationCode)
                    {
                        case 0x00E0:
                            ClearDisplay();
                            break;
                        case 0x00EE:
                            ReturnFromSubroutine();
                            break;
                    }
                    break;
                case 0x1000:
                    JumpToAddress(operationCode);
                    break;
                case 0x2000:
                    CallSubroutine(operationCode);
                    break;
                case 0x3000:
                    SkipNextInstructionIfVxEqualsCompareValue(operationCode);
                    break;
                case 0x4000:
                    SkipNextInstructionIfVxNotEqualCompareValue(operationCode);
                    break;
                case 0x5000:
                    SkipNextInstructionIfTwoRegistersHaveEqualValues(operationCode);
                    break;
                case 0x6000:
                    SetRegisterToValue(operationCode);
                    break;
                case 0x7000:
                    AddValueToRegister(operationCode);
                    break;
                case 0x8000:
                    switch (operationCode & 0x000F)
                    {
                        case 0x0000:
                            CopyRegisterVyValueToRegisterVx(operationCode);
                            break;
                        case 0x0001:
                            BitwiseOrRegisterVxWithRegisterVy(operationCode);
                            break;
                        case 0x0002:
                            BitwiseAndRegisterVxWithRegisterVy(operationCode);
                            break;
                        case 0x0003:
                            BitwiseExclusiveOrRegisterVxWithRegisterVy(operationCode);
                            break;
                        case 0x0004:
                            AddRegisterVyToRegisterVx(operationCode);
                            break;
                        case 0x0005:
                            SubtractRegisterVyFromRegisterVx(operationCode);
                            break;
                        case 0x0006:
                            BitShiftRegisterVxRight(operationCode);
                            break;
                        case 0x0007:
                            SubtractRegisterVxFromRegisterVx(operationCode);
                            break;
                        case 0x000E:
                            BitShiftRegisterVxLeft(operationCode);
                            break;
                    }
                    break;
                case 0x9000:
                    SkipInstructionIfRegisterVxNotEqualRegisterVy(operationCode);
                    break;
                case 0xA000:
                    SetIToAddress(operationCode);
                    break;
                case 0xB000:
                    JumpToAddressPlusValueOfRegisterV0(operationCode);
                    break;
                case 0xC000:
                    GenerateRandomNumber(operationCode);
                    break;
                case 0xD000:
                    DrawSprite(operationCode);
                    break;
                case 0xE000:
                    switch (operationCode & 0x000F)
                    {
                        case 0x000E:
                            SkipInstructionIfKeyIsPressed(operationCode);
                            break;
                        case 0x0001:
                            SkipInstructionIfKeyIsNotPressed(operationCode);
                            break;
                    }
                    break;
                case 0xF000:
                    switch (operationCode & 0x00FF)
                    {
                        case 0x0007:
                            SetRegisterVxToDelayTimerValue(operationCode);
                            break;
                        case 0x000A:
                            GetKeyPressAndStoreInRegisterVx(operationCode);
                            break;
                        case 0x0015:
                            SetDelayTimerToValueOfRegisterVx(operationCode);
                            break;
                        case 0x0018:
                            SetSoundTimerToValueOfRegisterVx(operationCode);
                            break;
                        case 0x001E:
                            AddRegisterVxValueToI(operationCode);
                            break;
                        case 0x0029:
                            SetIToLocationOfCharacterSprite(operationCode);
                            break;
                        case 0x0033:
                            ConvertRegisterVxValueToBcd(operationCode);
                            break;
                        case 0x0055:
                            DumpGeneralPurposeRegisterValuesToRam(operationCode);
                            break;
                        case 0x0065:
                            LoadGeneralPurposeRegistersWithRam(operationCode);
                            break;
                    }
                    break;
            }
        }

        private void LoadGeneralPurposeRegistersWithRam(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);

            for (byte x = 0; x <= registerXIndex; x++)
            {
                _cpu.SetGeneralPurposeRegisterValue(x, _ram.Memory[_cpu.I + x]);
            }

            _cpu.GoToNextInstruction();
        }

        private void DumpGeneralPurposeRegisterValuesToRam(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);

            for (byte x = 0; x <= registerXIndex; x++)
            {
                _ram.SetMemory((ushort) (_cpu.I + x), _cpu.GeneralPurposeRegisters[x]);
            }

            _cpu.GoToNextInstruction();
        }

        private void ConvertRegisterVxValueToBcd(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);
            byte registerVxValue = _cpu.GeneralPurposeRegisters[registerXIndex];

            byte mostSignificantDigit = (byte)(registerVxValue / 100);
            byte middleDigit = (byte)((registerVxValue % 100) / 10);
            byte onesDigit = (byte)((registerVxValue % 100) % 10);

            _ram.SetMemory(_cpu.I, mostSignificantDigit);
            _ram.SetMemory((ushort) (_cpu.I + 1), middleDigit);
            _ram.SetMemory((ushort) (_cpu.I + 2), onesDigit);

            _cpu.GoToNextInstruction();
        }

        private void SetIToLocationOfCharacterSprite(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);

            _cpu.I = (ushort) (_cpu.GeneralPurposeRegisters[registerXIndex] * 0x05);
            _cpu.GoToNextInstruction();
        }

        private void AddRegisterVxValueToI(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);

            _cpu.I += _cpu.GeneralPurposeRegisters[registerXIndex];
            _cpu.GoToNextInstruction();
        }

        private void SetSoundTimerToValueOfRegisterVx(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);

            _cpu.SoundTimer = _cpu.GeneralPurposeRegisters[registerXIndex];
            _cpu.GoToNextInstruction();
        }

        private void SetDelayTimerToValueOfRegisterVx(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);

            _cpu.DelayTimer = _cpu.GeneralPurposeRegisters[registerXIndex];
            _cpu.GoToNextInstruction();
        }

        private void GetKeyPressAndStoreInRegisterVx(ushort operationCode)
        {
            throw new NotImplementedException();
        }

        private void SetRegisterVxToDelayTimerValue(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);

            _cpu.SetGeneralPurposeRegisterValue(registerXIndex, _cpu.DelayTimer);
            _cpu.GoToNextInstruction();
        }

        private void SkipInstructionIfKeyIsNotPressed(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);
            byte keycode = _cpu.GeneralPurposeRegisters[registerXIndex];

            if (_keyboard.KeyboardState[keycode] == 0)
                _cpu.SkipNextInstruction();
            else
                _cpu.GoToNextInstruction();
        }

        private void SkipInstructionIfKeyIsPressed(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);
            byte keycode = _cpu.GeneralPurposeRegisters[registerXIndex];

            if (_keyboard.KeyboardState[keycode] == 1)
                _cpu.SkipNextInstruction();
            else
                _cpu.GoToNextInstruction();
        }

        private void DrawSprite(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);
            byte registerYIndex = (byte)((operationCode & 0x00F0) >> 4);
            byte coordinateX = _cpu.GeneralPurposeRegisters[registerXIndex];
            byte coordinateY = _cpu.GeneralPurposeRegisters[registerYIndex];
            byte spriteHeight = (byte)(operationCode & 0x000F);

            _cpu.SetGeneralPurposeRegisterValue(0xF, 0);

            for (int yLine = 0; yLine < spriteHeight; yLine++)
            {
                ushort pixel = _ram.Memory[_cpu.I + yLine];
                for (int xLine = 0; xLine < 8; xLine++)
                {
                    if ((pixel & (0x80 >> xLine)) != 0)
                    {
                        ushort displayMemoryIndex = (ushort) (coordinateX + xLine + ((coordinateY + yLine) * 64));

                        if (_gpu.Memory[displayMemoryIndex] == 1)
                            _cpu.SetGeneralPurposeRegisterValue(0xF, 1);

                        _gpu.SetDisplayMemory(displayMemoryIndex, (byte) (_gpu.Memory[displayMemoryIndex] ^ 1));
                    }
                }
            }

            _cpu.GoToNextInstruction();
        }

        private void GenerateRandomNumber(ushort operationCode)
        {
            Random randomNumberGenerator = new Random(DateTime.Now.Second);
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);
            byte valueToBitwiseAndWithRandomNumber = (byte)(operationCode & 0x00FF);
            byte randomNumber = (byte) randomNumberGenerator.Next(0, 255);

            _cpu.SetGeneralPurposeRegisterValue(registerXIndex, (byte) (randomNumber & valueToBitwiseAndWithRandomNumber));
            _cpu.GoToNextInstruction();
        }

        private void JumpToAddressPlusValueOfRegisterV0(ushort operationCode)
        {
            ushort addressToJumpTo = (ushort)(operationCode & 0x0FFF);
            _cpu.ProgramCounter = (ushort) (addressToJumpTo + _cpu.GeneralPurposeRegisters[0]);
        }

        private void SetIToAddress(ushort operationCode)
        {
            ushort addressValue = (ushort) (operationCode & 0x0FFF);
            _cpu.I = addressValue;

            _cpu.GoToNextInstruction();
        }

        private void SkipInstructionIfRegisterVxNotEqualRegisterVy(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);
            byte registerYIndex = (byte)((operationCode & 0x00F0) >> 4);

            if (_cpu.GeneralPurposeRegisters[registerXIndex] != _cpu.GeneralPurposeRegisters[registerYIndex])
                _cpu.SkipNextInstruction();
            else
                _cpu.GoToNextInstruction();
        }

        private void BitShiftRegisterVxLeft(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);

            _cpu.SetGeneralPurposeRegisterValue(0xF, (byte) ((_cpu.GeneralPurposeRegisters[registerXIndex] & 0b1000_0000) >> 7));
            _cpu.SetGeneralPurposeRegisterValue(registerXIndex, (byte) (_cpu.GeneralPurposeRegisters[registerXIndex] << 1));

            _cpu.GoToNextInstruction();
        }

        private void SubtractRegisterVxFromRegisterVx(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);
            byte registerYIndex = (byte)((operationCode & 0x00F0) >> 4);

            _cpu.SetGeneralPurposeRegisterValue(0xF,
                (byte) (_cpu.GeneralPurposeRegisters[registerYIndex] >= _cpu.GeneralPurposeRegisters[registerXIndex] ? 1 : 0));
            _cpu.SetGeneralPurposeRegisterValue(registerXIndex, (byte) (_cpu.GeneralPurposeRegisters[registerYIndex] - _cpu.GeneralPurposeRegisters[registerXIndex]));
            _cpu.GoToNextInstruction();
        }

        private void BitShiftRegisterVxRight(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);

            _cpu.SetGeneralPurposeRegisterValue(0xF, (byte) (_cpu.GeneralPurposeRegisters[registerXIndex] & 0b0000_0001));
            _cpu.SetGeneralPurposeRegisterValue(registerXIndex, (byte) (_cpu.GeneralPurposeRegisters[registerXIndex] >> 1));
            _cpu.GoToNextInstruction();
        }

        private void SubtractRegisterVyFromRegisterVx(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);
            byte registerYIndex = (byte)((operationCode & 0x00F0) >> 4);

            _cpu.SetGeneralPurposeRegisterValue(0xF,
                (byte) (_cpu.GeneralPurposeRegisters[registerXIndex] >= _cpu.GeneralPurposeRegisters[registerYIndex] ? 1 : 0));

            _cpu.SetGeneralPurposeRegisterValue(registerXIndex, (byte) (_cpu.GeneralPurposeRegisters[registerXIndex] - _cpu.GeneralPurposeRegisters[registerYIndex]));
            _cpu.GoToNextInstruction();
        }

        private void AddRegisterVyToRegisterVx(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);
            byte registerYIndex = (byte)((operationCode & 0x00F0) >> 4);

            _cpu.SetGeneralPurposeRegisterValue(0xF,
                (byte) (_cpu.GeneralPurposeRegisters[registerXIndex] > (0xFF - _cpu.GeneralPurposeRegisters[registerYIndex])
                    ? 1
                    : 0));

            _cpu.SetGeneralPurposeRegisterValue(registerXIndex, (byte) (_cpu.GeneralPurposeRegisters[registerXIndex] + _cpu.GeneralPurposeRegisters[registerYIndex]));
            _cpu.GoToNextInstruction();
        }

        private void BitwiseExclusiveOrRegisterVxWithRegisterVy(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);
            byte registerYIndex = (byte)((operationCode & 0x00F0) >> 4);

            byte bitwiseExclusiveOrResult = (byte)(_cpu.GeneralPurposeRegisters[registerXIndex] ^
                                           _cpu.GeneralPurposeRegisters[registerYIndex]);

            _cpu.SetGeneralPurposeRegisterValue(registerXIndex, bitwiseExclusiveOrResult);
            _cpu.GoToNextInstruction();
        }

        private void BitwiseAndRegisterVxWithRegisterVy(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);
            byte registerYIndex = (byte)((operationCode & 0x00F0) >> 4);

            byte bitwiseAndResult = (byte) (_cpu.GeneralPurposeRegisters[registerXIndex] & 
                                            _cpu.GeneralPurposeRegisters[registerYIndex]);

            _cpu.SetGeneralPurposeRegisterValue(registerXIndex, bitwiseAndResult);
            _cpu.GoToNextInstruction();
        }

        private void BitwiseOrRegisterVxWithRegisterVy(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);
            byte registerYIndex = (byte)((operationCode & 0x00F0) >> 4);

            byte bitwiseOrResult = (byte) (_cpu.GeneralPurposeRegisters[registerXIndex] |
                                         _cpu.GeneralPurposeRegisters[registerYIndex]);

            _cpu.SetGeneralPurposeRegisterValue(registerXIndex, bitwiseOrResult);
            _cpu.GoToNextInstruction();
        }

        private void CopyRegisterVyValueToRegisterVx(ushort operationCode)
        {
            byte registerXIndex = (byte)((operationCode & 0x0F00) >> 8);
            byte registerYIndex = (byte)((operationCode & 0x00F0) >> 4);

            _cpu.SetGeneralPurposeRegisterValue(registerXIndex, _cpu.GeneralPurposeRegisters[registerYIndex]);
            _cpu.GoToNextInstruction();
        }

        private void AddValueToRegister(ushort operationCode)
        {
            byte registerIndex = (byte)((operationCode & 0x0F00) >> 8);
            byte valueToAddToRegister = (byte)(operationCode & 0x00FF);

            _cpu.SetGeneralPurposeRegisterValue(registerIndex, 
                                                (byte) (_cpu.GeneralPurposeRegisters[registerIndex] + valueToAddToRegister));
            _cpu.GoToNextInstruction();
        }

        private void SetRegisterToValue(ushort operationCode)
        {
            byte registerIndex = (byte)((operationCode & 0x0F00) >> 8);
            byte valueToAssignRegister = (byte)(operationCode & 0x00FF);

            _cpu.SetGeneralPurposeRegisterValue(registerIndex, valueToAssignRegister);
            _cpu.GoToNextInstruction();
        }

        private void SkipNextInstructionIfTwoRegistersHaveEqualValues(ushort operationCode)
        {
            byte registerIndexX = (byte)((operationCode & 0x0F00) >> 8);
            byte registerIndexY = (byte)((operationCode & 0x00F0) >> 4);

            if (_cpu.GeneralPurposeRegisters[registerIndexX] == _cpu.GeneralPurposeRegisters[registerIndexY])
                _cpu.SkipNextInstruction();
            else
                _cpu.GoToNextInstruction();
        }

        private void SkipNextInstructionIfVxNotEqualCompareValue(ushort operationCode)
        {
            byte registerIndex = (byte)((operationCode & 0x0F00) >> 8);
            byte compareValue = (byte) (operationCode & 0x00FF);

            if (_cpu.GeneralPurposeRegisters[registerIndex] != compareValue)
                _cpu.SkipNextInstruction();
            else
                _cpu.GoToNextInstruction();
        }

        private void SkipNextInstructionIfVxEqualsCompareValue(ushort operationCode)
        {
            byte registerIndex = (byte)((operationCode & 0x0F00) >> 8);
            byte compareValue = (byte)(operationCode & 0x00FF);

            if (_cpu.GeneralPurposeRegisters[registerIndex] == compareValue)
                _cpu.SkipNextInstruction();
            else
                _cpu.GoToNextInstruction();
        }

        private void ClearDisplay()
        {
            Sdl.RenderClear(_renderer);
            _gpu.ClearDisplayMemory();
            _cpu.GoToNextInstruction();
        }

        private void ReturnFromSubroutine()
        {
            _cpu.ProgramCounter = _cpu.PopAddressFromStack();
            _cpu.GoToNextInstruction();
        }

        private void JumpToAddress(ushort operationCode)
        {
            var addressToJumpTo = (ushort) (operationCode & 0x0FFF);
            _cpu.ProgramCounter = addressToJumpTo;
        }

        private void CallSubroutine(ushort operationCode)
        {
            _cpu.PushAddressToStack(_cpu.ProgramCounter);
            _cpu.ProgramCounter = (ushort) (operationCode & 0x0FFF);
        }
    }
}