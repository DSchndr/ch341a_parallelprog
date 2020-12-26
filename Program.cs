using System;
using System.IO;

namespace CH341a_i2c_par_eeprom
{
    class Program
    {

        private static int flashsize = 0x1FFFF;
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("DSchndr's slow af CH341a i2c Parallel programmer v0.1\n");
            long initv = CH341a.Init(config.device);
            Console.ResetColor();
            if (initv == -1) return;

            /* 0x60: 20khz
             * 0x61: 100khz
             * 0x62: 400khz
             * 0x63: 750khz
             * 
             * For spi:
             * CH341a.CH341SetStream(config.device, 0b00000000)
             */
            if (!CH341a.CH341SetStream(config.device, 0x63)) {
                Console.WriteLine("Set I2C speed failed"); 
                return; 
            }


            EEProm eeprom = new EEProm();

            /*
            //EEProm writer
            Console.WriteLine("Overwriting EEProm with 0xFF");
            eeprom.PrepWrite();
            for (uint i = 0; i <= flashsize; i++)
            {
                eeprom.WriteEEProm(i, 0xFF);
                Console.SetCursorPosition(0, 10);
                Console.Write($"ADDR: {i}");
                Console.SetCursorPosition(0, 11);
                Console.Write(" |{0}|", ((decimal)i / (decimal)flashsize).ToString("0.0%"));
            }

            //Flash writer
            Console.WriteLine("Overwriting flash with 0xAA");
            eeprom.FlashErase();
            eeprom.PrepWrite();
            for (uint i = 0; i <= flashsize; i++)
            {
                eeprom.WriteEEProm(0x5555, (byte)0xAA);
                eeprom.WriteEEProm(0x2AAA, (byte)0x55);
                eeprom.WriteEEProm(0x5555, (byte)0xA0);
                eeprom.WriteEEProm(i, 0xAB);
                Console.SetCursorPosition(0, 10);
                Console.Write($"ADDR: {i}");
                Console.SetCursorPosition(0, 11);
                Console.Write(" |{0}|", ((decimal)i / (decimal)flashsize).ToString("0.0%"));
            }*/

            //EEPROM DUMPER
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Press [ESC] to stop reading and save to file\n");
            Console.ResetColor();
            Console.WriteLine("00|01|02|03|04|05|06|07|08|09|0A|0B|0C|0D|0E|0F|");
            Console.Write    ("--|--|--|--|--|--|--|--|--|--|--|--|--|--|--|--|");

            byte[] buffer = new byte[16];
            int c = 0;

            eeprom.PrepRead();
            FileStream fileStream = new FileStream("dump.bin", FileMode.Create);
            
            //Dump EEProm till flashsize
            for (uint i = 0; i <= flashsize; i++)
            {
                if(Console.KeyAvailable && (Console.ReadKey(true).Key == ConsoleKey.Escape))
                {
                    fileStream.Close();
                    CH341a.CH341CloseDevice(0);
                    return;
                }
                if (i % 16 == 0) {
                    //Console.Write(System.Text.Encoding.UTF8.GetString(buffer));
                    c = 0;
                    if (i % 128 == 0)
                    {
                        Console.Write(" |{0}|",((decimal)i / (decimal)flashsize).ToString("0.0%"));
                    }
                    Console.WriteLine();
                }
                byte readA = eeprom.Read(i);
                buffer[c++] = readA;
                Console.Write(readA.ToString("X2"));
                fileStream.WriteByte(readA);
                Console.Write("|");
            }
        }

    }
}
