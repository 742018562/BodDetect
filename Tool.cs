using System;
using System.Collections.Generic;
using System.Text;

namespace BodDetect
{



    public enum RecDataType
    {
        error = -1,
        _uint16,
        _uint32,
        Little_float,
        Big_float,
        DO_float
    }


    public enum MemoryAreaCode 
    {
        WR = 0X31,
        DR = 0X80,
        IO = 0X81
    }

    class Tool
    {
       public static string  StringtoHex(byte[] myBytes)
       {
            string result = null;
            foreach (byte b in myBytes)
            {
                string val = b.ToString("X");
                result += (val.Length == 2 ? val : "0" + val);
            }
            return result;
        }

        public static ushort[] ToShorts(byte[] data)
        {
            ushort[] r = new ushort[data.Length / 2];
            for (int i = 0; i < r.Length; i++)
            {
                r[i] = data[i * 2 + 1];
                r[i] = (ushort)(r[i] | data[i * 2] << 8);
            }
            return r;
        }

        public static float[] BigToFloats(byte[] data) 
        {
            float[] r = new float[data.Length / 4];
            for (int i = 0; i < r.Length; i++)
            {
                byte[] floats = new byte[4];
                floats[0] = data[i*4];
                floats[1] = data[i*4 + 1];
                floats[2] = data[i*4 + 2];
                floats[3] = data[i*4 + 3];

                r[i] = BitConverter.ToSingle(floats,0);
                //r[i] = data[i * 2 + 1]; 
                //r[i] = (ushort)(r[i] | data[i * 2] << 8);
            }
            return r;
        }

        public static float[] LitToFloats(byte[] data) 
        {
            float[] r = new float[data.Length / 4];
            for (int i = 0; i < r.Length; i++)
            {
                byte[] floats = new byte[4];
                floats[3] = data[i * 4];
                floats[2] = data[i * 4 + 1];
                floats[1] = data[i * 4 + 2];
                floats[0] = data[i * 4 + 3];

                r[i] = BitConverter.ToSingle(floats,0);
                //r[i] = data[i * 2 + 1]; 
                //r[i] = (ushort)(r[i] | data[i * 2] << 8);
            }
            return r;
        }


        public static float[] DoFloats(byte[] data) 
        {
            float[] r = new float[data.Length / 4];
            for (int i = 0; i < r.Length; i++)
            {
                byte[] floats = new byte[4];
                floats[0] = data[i * 4 + 1];
                floats[1] = data[i * 4];
                floats[2] = data[i * 4 + 3];
                floats[3] = data[i * 4 + 2];

                r[i] = BitConverter.ToSingle(floats, 0);

            }
            return r;
        }

        public static uint[] ToInt32(byte[] data) 
        {
            uint[] r = new uint[data.Length / 4];
            for (int i = 0; i < r.Length; i++)
            {
                byte[] int32s = new byte[4];
                int32s[3] = data[i*4];
                int32s[2] = data[i * 4 + 1];
                int32s[1] = data[i * 4 + 2];
                int32s[0] = data[i * 4 + 3];
                r[i] = BitConverter.ToUInt32(int32s,0);
            }
            return r;

        }

        public static byte[] ToBytes(ushort[] shorts)
        {
            byte[] r = new byte[shorts.Length * 2];
            for (int i = 0; i < shorts.Length; i++)
            {
                r[i * 2] = (byte)(shorts[i] >> 8);
                r[i * 2 + 1] = (byte)shorts[i];
            }

            return r;
        }

       

    }
}
