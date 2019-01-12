
namespace WaveRGB
{
    class KeyboardInfo  // this file contains data relevant to the US G910 keyboard.
                        // edit just this file to adapt the application to any other keyboard layout.
    {
        public const int designWidth = 963;   // size of the original pixelmap the coordinates below are based upon
        public const int designHeight = 352;

        public const int key_HIDcode = 0;
        public const int key_WinCode = 1;
        public const int key_X = 2;
        public const int key_Y = 3;

        public const int specialLogiKey = 999;
        public const int mediaKey = 0xace;

        readonly static int row1 = 57;
        readonly static int row2 = 107;
        readonly static int row3 = 145;
        readonly static int row4 = 185;
        readonly static int row5 = 224;
        readonly static int row6 = 263;

        public readonly static int[,] keyLocations = new int[,] {
            //HID_code, WindowsKeyCode, x, y of key center
            {0x29,27,80, row1},//esc
            {0x3A,112,146,row1},//f1
            {0x3B,113,185,row1},//f2
            {0x3C,114,222,row1},//f3
            {0x3D,115,256,row1},//f4
            {0x3E,116,330,row1},//f5
            {0x3F,117,370,row1},//f6
            {0x40,118,408,row1},//f7
            {0x41,119,447,row1},//f8
            {0x42,120,512,row1},//f9
            {0x43,121,552,row1},//f10
            {0x44,122,591,row1},//f11
            {0x45,123,631,row1},//f12
            {0x46,44,683,row1},//print screen
            {0x47,145,722,row1},//scroll lock
            {0x48,19,761,row1},//pause
                
            {0x35,192,80,row2},//~
            {0x1E,49,120,row2},//1
            {0x1F,50,159,row2},//2
            {0x20,51,198,row2},//3
            {0x21,52,238,row2},//4
            {0x22,53,276,row2},//5
            {0x23,54,316,row2},//6
            {0x24,55,355,row2},//7
            {0x25,56,396,row2},//8
            {0x26,57,434,row2},//9
            {0x27,48,474,row2},//0
            {0x2D,189,512,row2},//-
            {0x2E,187,552,row2},//=
            {0x2A,8,610,row2},//<-
            {0x49,45,683,row2},//INS
            {0x4A,36,722,row2},//HOME
            {0x4B,33,761,row2},//PAGEUP
            {0x53,144,815,row2},//NUMLOCK
            {0x54,111,854,row2},// /
            {0x55,106,893,row2},// *
            {0x56,109,933,row2},//-
                
            {0x2B,9,90,row3},//Tab
            {0x14,81,139,row3},//q
            {0x1A,87,179,row3},//w
            {0x08,69,218,row3},//e
            {0x15,82,257,row3},//r
            {0x17,84,297,row3},//t
            {0x1C,89,337,row3},//y
            {0x18,85,375,row3},//u
            {0x0C,73,415,row3},//i
            {0x12,79,454,row3},//o
            {0x13,80,492,row3},//p
            {0x2F,219,532,row3},//[
            {0x30,221,571,row3},//]
            {0x31,220,620,row3},// \
            {0x4C,46,684,row3},//Del
            {0x4D,35,721,row3},//End
            {0x4E,34,762,row3},//PageD
            {0x5F,103,815,row3},//7
            {0x60,104,854,row3},//8
            {0x61,105,893,row3},//9
            {0x57,107,933,167},//+

            {0x39,20,90,row4},//Caps
            {0x04,65,149,row4},//a
            {0x16,83,190,row4},//s
            {0x07,68,227,row4},//d
            {0x09,70,268,row4},//f
            {0x0A,71,306,row4},//g
            {0x0B,72,346,row4},//h
            {0x0D,74,385,row4},//j
            {0x0E,75,424,row4},//k
            {0x0F,76,463,row4},//l
            {0x33,186,502,row4},//;
            {0x34,222,542,row4},//'
            //{0x??,???,582,row4},//     European extra key
            {0x28,13,606,row4},//enter
            {0x5C,100,815,row4},//4
            {0x5D,101,854,row4},//5
            {0x5D,12,854,row4},//keypad 5 w/o NUM LOCK
            {0x5E,102,893,row4},//6
                
            {0xE1,160,107,row5},//LShift
            //{0x??,???,130,row5},//     European extra key
            {0x1D,90,170,row5},//z
            {0x1B,88,208,row5},//x
            {0x06,67,248,row5},//c
            {0x19,86,287,row5},//v
            {0x05,66,326,row5},//b
            {0x11,78,366,row5},//n
            {0x10,77,405,row5},//m
            {0x36,188,444,row5},//,
            {0x37,190,483,row5},//.
            {0x38,191,522,row5},// /
            {0xE5,161,597,row5},//RShift
            {0x52,38,721,row5},//up
            {0x59,97,815,row5},//1
            {0x5A,98,854,row5},//2
            {0x5B,99,893,row5},//3
            {0x58,13,933,245},//kp_enter   Windows sends 13 for both enter keys but included here for lighting

            {0xE0,162,90,row6},//Lctrl
            {0xE3,91,147,row6},//Lwin
            {0xE2,164,194,row6},//Lalt
            {0x2C,32,332,row6},//space
            {0xE6,165,468,row6},//Ralt
            {0xE7,92,517,row6},//Rwin
            {0x65,93,566,row6},//context
            {0xE4,163,618,row6},//Rctrl
            {0x50,37,684,row6},//left
            {0x51,40,721,row6},//down
            {0x4F,39,762,row6},//right
            {0x62,96,833,row6},//0
            {0x63,110,893,row6},//.

            {0xFFFF1,specialLogiKey,32,row1},//"G"     Windows does not detect these keys but included here for lighting
            {0xFFFF2,specialLogiKey,226,331},//"G910"
            {0xFFF1,specialLogiKey,32,row2},//g1
            {0xFFF2,specialLogiKey,32,row3},//g2
            {0xFFF3,specialLogiKey,32,row4},//g3
            {0xFFF4,specialLogiKey,32,row5},//g4
            {0xFFF5,specialLogiKey,32,row6},//g5
            {0xFFF6,specialLogiKey,146,20},//g6
            {0xFFF7,specialLogiKey,185,20},//g7
            {0xFFF8,specialLogiKey,222,20},//g8
            {0xFFF9,specialLogiKey,256,20},//g9

            {mediaKey,0xAD,893,row1},//media   Windows sends codes when media keys pressed but no logitech API to change media key light colors
            {mediaKey,0xAE,893,row1},//media            These x,y coordinates make lights eminate from above the keypad
            {mediaKey,0xAF,893,row1},//media
            {mediaKey,0xB0,893,row1},//media
            {mediaKey,0xB1,893,row1},//media
            {mediaKey,0xB2,893,row1},//media
            {mediaKey,0xB3,893,row1}//media
                
            };

        public readonly static int keysCount = keyLocations.Length / 4;  // div 4 because .Length counts elements in all dimensions.

    }
}
