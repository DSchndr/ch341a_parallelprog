using System;

/* TODO:
 *  - Implement high voltage programming in code and hw
 *  - Add support for bigger address bus 
 *  - Add support for 16bit data bus 
 * 
 */

namespace CH341a_i2c_par_eeprom
{
    class EEProm
    {
        private readonly MCP23017 PX1;
        private readonly MCP23017 PX2;

        private byte PX2B = 0b00000111;
        private byte byte2_old = 0;
        private byte byte3_old = 0;

        public EEProm()
        {
            PX1 = new MCP23017(config.device, config.PX1);
            PX2 = new MCP23017(config.device, config.PX2);
            //Set Address and control to output
            PX1.SetDirection(1, 0x00);
            PX2.SetDirection(0, 0x00);
            PX2.SetDirection(1, 0x00);
            //Set address lines to 0...
            PX1.GPIOWrite(1, 0);
            PX2.GPIOWrite(0, 0);
        }

        //Deletes flash page i
        public void FlashErasePage(uint i)
        {
            PrepWrite();
            Write(0x5555, 0xAA);
            Write(0x2AAA, 0x55);
            Write(0x5555, 0x80);
            Write(0x5555, 0xAA);
            Write(0x2AAA, 0x55);
            Write(i, 0x30);
        }

        //Fully erases flash
        public void FlashErase()
        {
            PrepWrite();
            Write(0x5555, 0xAA);
            Write(0x2AAA, 0x55);
            Write(0x5555, 0x80);
            Write(0x5555, 0xAA);
            Write(0x2AAA, 0x55);
            Write(0x5555, 0x10);
        }

        //Prepare bus, adr, control for read
        public void PrepRead()
        {
            //Reset Chip
            PX2B = 0b00000111;
            PX2.GPIOWrite(1, PX2B);
            //Set data pins to input
            PX1.SetDirection(0, 0xff);
            SetAddress(0);
            //Set chip to read mode
            PX2B = (byte)(PX2B | (1 << 1)); //Set WE high
            PX2B = (byte)(PX2B & ~(1 << 0)); //Set CE Low
            PX2B = (byte)(PX2B & ~(1 << 2)); //Set OE Low
            PX2.GPIOWrite(1, PX2B);
        }

        //Set address, read data
        // Run PrepRead() beforehand
        public byte Read(uint pAddress)
        {
            /*
            //Datasheet says this (slow)
            SetAddress(pAddress);
            PX2B = (byte)(PX2B & ~(1 << 0)); //Set CE Low
            PX2.GPIOWrite(1, PX2B);
            PX2B = (byte)(PX2B & ~(1 << 2)); //Set OE Low
            PX2.GPIOWrite(1, PX2B);
            byte data = GetBus();
            PX2B = (byte)(PX2B | (1 << 2)); //Set OE high
            PX2.GPIOWrite(1, PX2B);
            return data;
            */
            SetAddress(pAddress);
            byte data = GetBus();
            return data;
        }

        //Prepare bus, adr, control for write
        public void PrepWrite()
        {
            //Set all high because we DONT WANT TO OVERWRITE RANDOM DATA
            PX2B = 0b00000111;
            PX2.GPIOWrite(1, PX2B);
            //Set data pins to output
            PX1.SetDirection(0, 0x00);
            //Enable chip with WE and OE high, CE low
            PX2.GPIOWrite(1, 0b00000110);
        }
        public void Write(uint pAddress, byte pData)
        {
            SetAddress(pAddress);
            WriteBus(pData);
            PX2B = (byte)(PX2B & ~(1 << 1)); //Set WE Low
            PX2.GPIOWrite(1, PX2B);
            PX2B = (byte)(PX2B | (1 << 1)); //Set WE high
            PX2.GPIOWrite(1, PX2B);
        }

        /* Disable hardware sector protection on parallel flash chips
        *   AMD: 12v on A9
        *   Datasheet says nothing about how to set it but i suppose
        *   CE: L | OE: L | WE: H | A0: H | A1: H | A9: Vid | DQ0-DQ7: sector 0-7 (High: Prot, Low: Unprot)
        */
        public void SetFlashProtection(uint sectors)
        {
            return;
        }

        /* Get hardware sector protection on parallel flash chips
         *  AMD: 12v on A9
         *  CE: L | OE: L | WE: H | A0: L | A1: H | A9: Vid | DQ0-DQ7: sector 0-7 (High: Prot, Low: Unprot)
         */
        public uint GetFlashProtection()
        {
            return 0;
        }

        //Get bits on Data bus
        private byte GetBus()
        {
            return PX1.GPIORead(0); 
        }

        //Write bits onto Data bus
        private void WriteBus(byte pData)
        {
            PX1.GPIOWrite(0, pData);
        }

        public void SetAddress(uint pAddr)
        {
            byte[] Bytes = BitConverter.GetBytes(pAddr);
            //Console.WriteLine($"{Bytes[3]}|{Bytes[2]}|{Bytes[1]}|{Bytes[0]}");
            PX1.GPIOWrite(1, Bytes[0]);
            byte b2_reversed = ReverseBitsWith4Operations(Bytes[1]); //Order on PX2.B Fucked up by me
            if (byte2_old != b2_reversed)
            {
                PX2.GPIOWrite(0, b2_reversed);
                byte2_old = b2_reversed;
            }
            if (byte3_old != Bytes[2])
            {
                if (Bytes[2] == 1) PX2B = (byte)(PX2B | (1 << 3));
                else PX2B = (byte)(PX2B & ~(1 << 3));
                PX2.GPIOWrite(1, PX2B);
                byte3_old = Bytes[2];
            }
        }

        //https://stackoverflow.com/questions/3587826/is-there-a-built-in-function-to-reverse-bit-order
        private static byte ReverseBitsWith4Operations(byte b)
        {
            return (byte)(((b * 0x80200802ul) & 0x0884422110ul) * 0x0101010101ul >> 32);
        }
    }
}
