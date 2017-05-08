namespace BeagleChipper.Models
{
    /*
     * All instructions are 2 bytes long and are stored most-significat-byte first.
     * In memory, the first byte of each instruction should be located at an even address.
     * If a program includes sprite data, it should be padded so any instructions following it
     * Will be properly situated in RAM.
     * 
     */
    public enum OperationCode
    {
        ClearDisplay = 0x00E0
    }
}