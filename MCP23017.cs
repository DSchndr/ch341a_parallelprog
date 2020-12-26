/* TODO: Add SPI support for faster R/W
 * 
 * 
 */
namespace CH341a_i2c_par_eeprom
{
    class MCP23017
    {
        private int device = 0;
        private byte pexaddr;
        private bool retval;


        static public class Reg
        {
            //IOCON.BANK = 0
            /* GPIO DIRECTION
             * 1 = input
             * 0 = output
             */
            static public byte IODIRA = 0x00;
            static public byte IODIRB = 0x01;

            /* GPIO POLARITY
             * 1 = invert gpio register bit
             */
            static public byte IPOLA = 0x02;
            static public byte IPOLB = 0x03;

            /* GPIO PULLUP
             * 1 + set as input = 100k pullup
             */
            static public byte GPPUA = 0x0C;
            static public byte GPPUB = 0x0D;

            /* GPIO REGISTER
             */
            static public byte GPIOA = 0x12;
            static public byte GPIOB = 0x13;
            /* OUTPUT LATCH REGISTER
             * READ: Last written value to port
             * WRITE: Writes Value to Port
             */
            static public byte OLATA = 0x14;
            static public byte OLATB = 0x15;

        }


        public MCP23017(int pdevice, byte pPortexpanderaddress)
        {
            device = pdevice;
            pexaddr = pPortexpanderaddress;
        }

        public void SetPullup(int pRegister, bool state)
        {
            do
            {
                retval = CH341a.CH341WriteI2C(device, pexaddr, (pRegister == 0) ? Reg.GPIOA : Reg.GPIOA, (state == true) ? (byte)0xff : (byte)0x00);
            } while (retval == false);
        }

        //Reads value from expander and register A/B
        public byte GPIORead(int pRegister)
        {
            byte register = 0;
            do
            {
                retval = CH341a.CH341ReadI2C(device, pexaddr, (pRegister == 0) ? Reg.GPIOA : Reg.GPIOB, ref register);
            } while (retval == false);
            return register;
        }
        //Writes value to output latch
        public void GPIOWrite(int pRegister, byte pValue)
        {
            //Try again until the shitty library returns true.
            do
            {
                retval = CH341a.CH341WriteI2C(device, pexaddr, (pRegister == 0) ? Reg.OLATA : Reg.OLATB, pValue);
            }
            while (retval == false);
        }

        public void SetDirection(int pRegister, byte pValue)
        {
            do { retval = CH341a.CH341WriteI2C(device, pexaddr, (pRegister == 0) ? Reg.IODIRA : Reg.IODIRB, pValue);}
            while (retval == false);
        }
    }
}
