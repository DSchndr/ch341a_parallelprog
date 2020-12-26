using System;
using System.Runtime.InteropServices;

//From: http://ftp.netbsd.org/pub/NetBSD/NetBSD-release-9/src/sys/dev/usb/uchcom.c
/*
 * ~driver~ Wrapper for WinChipHead CH341/340, the worst USB-serial chip in the world.
 */

namespace CH341a_i2c_par_eeprom
{
    class CH341a
    {
        [DllImport("ch341dll.DLL")]
        public extern static long CH341OpenDevice(int iIndex);

        [DllImport("ch341dll.DLL")]
        public extern static void CH341CloseDevice(int iIndex);
        // CH341SetStream() konfiguriert I2C und SPI
        // Bit 1-0: I2C speed 00= low speed /20KHz
        // 01= standard /100KHz
        // 10= fast /400KHz
        // 11= high speed /750KHz           plus  0x60  , das ist wichtig. Also :
        // BOOL	WINAPI	CH341SetStream( ULONG iIndex, ULONG	iMode );*/
        [DllImport("ch341dll.DLL")]
        public extern static bool CH341SetStream(int iIndex, int iMode);


        // ********* I2C FUNCTIONS *********

        [DllImport("ch341dll.DLL")]
        public extern static bool CH341WriteI2C(int iIndex, byte iDevice, byte iAddr, byte iByte);

        [DllImport("ch341dll.DLL")]
        public extern static bool CH341ReadI2C(int iIndex, byte iDevice, byte iAddr, ref byte oByte);


        [DllImport("ch341dll.DLL")]
        public extern static bool CH341StreamI2C(int iIndex, int wlen, ref byte WBuf, int rlen, ref byte RBuf);

        // ********* END I2C FUNCTIONS *********


        // ********* SPI FUNCTIONS *********
        [DllImport("ch341dll.DLL")]
        public extern static bool CH341StreamSPI4( // Processing the SPI data stream, 4-wire interface, the clock line for the DCK / D3 pin, the output data line DOUT / D5 pin, the input data line for the DIN / D7 pin, chip line for the D0 / D1 / D2, the speed of about 68K bytes
            /* SPI Timing: The DCK / D3 pin is clocked and defaults to the low level. The DOUT / D5 pin is output during the low period before the rising edge of the clock. The DIN / D7 pin is at a high level before the falling edge of the clock enter */
            int iIndex, // Specify the CH341 device serial number
            int iChipSelect, // Chip select control, bit 7 is 0 is ignored chip select control, bit 7 is 1 parameter is valid: bit 1 bit 0 is 00/01/10 select D0 / D1 / D2 pin as low active chip select
            int iLength, // The number of bytes of data to be transferred
            ref byte ioBuffer // Point to a buffer, place the data to be written from DOUT, and return the data read from DIN
            );

        [DllImport("ch341dll.DLL")]
        public extern static bool CH341Set_D5_D0(  // Set the I / O direction of the D5-D0 pin of CH341 and output data directly through the D5-D0 pin of CH341, which is higher than CH341SetOutput
        /* ***** Use this API with caution to prevent the I / O direction from changing the input pin into an output pin that causes a short circuit between the output pins and other output pins ***** */
        int iIndex,  // Specify the CH341 device serial number
        int iSetDirOut,  // Set the D5-D0 pin I / O direction, a clear 0 is the corresponding pin for the input, a position of the corresponding pin for the output, parallel port mode default value of 0x00 all input
        int iSetDataOut  // Set the output data of each pin of D5-D0. If the I / O direction is output, the corresponding pin output is low when a bit is cleared to 0, and the pin output is high when a bit is set
        );              // The bits 5 to 0 of the above data correspond to the D5-D0 pin of CH341, respectively


        [DllImport("ch341dll.DLL")]
        public extern static bool CH341BitStreamSPI(  // Processing the SPI bit data stream, 4 line / 5 line interface, the clock line for the DCK / D3 pin, the output data line DOUT / DOUT2 pin, the input data line for the DIN / DIN2 pin, chip select line D0 / D1 / D2, the speed of about 8K bit * 2
        int iIndex,  // Specify the CH341 device serial number
        int iLength,  // Ready to transfer the number of data bits, up to 896 at a time, it is recommended not to exceed 256
        ref byte ioBuffer  // Point to a buffer, place the data to be written from DOUT / DOUT2 / D2-D0, and return the data read from DIN / DIN2
        );
        /* SPI Timing: The DCK / D3 pin is clocked and defaults to the low level. The DOUT / D5 and DOUT2 / D4 pins are output during the low level before the rising edge of the clock. The DIN / D7 and DIN2 / D6 pins are clocked The falling edge of the previous high period is entered */
        /* A bit in the ioBuffer is 8 bits corresponding to the D7-D0 pin, bit 5 is output to DOUT, bit 4 is output to DOUT2, bit 2-bit 0 is output to D2-D0, bit 7 is input from DIN, bit 6 from DIN2 Input, bit 3 data ignored */
        /* Before calling the API, you should call CH341Set_D5_D0 to set the I / O direction of the D5-D0 pin of CH341 and set the default level of the pin */
        
        // ********* END SPI FUNCTIONS *********

        public static long Init(int index)
        {
            long open = CH341OpenDevice(index);
            if (open != 4294967295)   // if chip not connected open is Hex 00000000FFFFFFFF
            {
                Console.WriteLine("CH341a connected with handle: " + open.ToString("X16"));
                return open;
            }
            else Console.WriteLine("No Chip available");
            CH341CloseDevice(index);
            return -1;
        }

    }
}
