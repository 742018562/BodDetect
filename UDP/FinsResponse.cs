using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Media;

namespace BodDetect.UDP
{
   

    internal class FinsResponse
    {
        public FinsResponse(byte sid, ushort[] data, float[] floatData)
        {
            Sid = sid;
            Data = data;
            FloatData = floatData;
            WaitEvent = new ManualResetEvent(false);
        }

        public byte Sid { get; private set; }
        public ushort[] Data { get; private set; }
        public ManualResetEvent WaitEvent { get; private set; }

        public float[] FloatData { get; private set; }

        public uint[] Int32Data { get; private set; }

        public RecDataType RecType { get; set; }

        public bool CmdSuccess = false;

        public void Reset()
        {
            Sid = 0;
            if (Data != null) 
            {
                Array.Clear(Data, 0, Data.Length);
            }

            Data = null;

            if (FloatData != null) 
            {
                Array.Clear(FloatData, 0, FloatData.Length);
            }

            if (Int32Data != null) 
            {
                Array.Clear(Int32Data, 0, Int32Data.Length);
            }

            FloatData = null;

            CmdSuccess = false;
            RecType = RecDataType.error;

            WaitEvent.Reset();
        }

        public void PutValue(byte sid, byte[] data)
        {
            Sid = sid;

            switch (RecType)
            {
                case RecDataType.error:
                    CmdSuccess = false;
                    break;

                case RecDataType._uint16:
                    Data = Tool.ToShorts(data);
                    break;

                case RecDataType._uint32:
                    Int32Data = Tool.ToInt32(data);
                    break;

                case RecDataType.Little_float:
                    FloatData = Tool.LitToFloats(data);
                    break;

                case RecDataType.Big_float:
                    FloatData = Tool.BigToFloats(data);
                    break;

                case RecDataType.DO_float:
                    FloatData = Tool.DoFloats(data);
                    break;
                default:
                    break;
            }


            WaitEvent.Set();
        }


        public void PutValue(byte sid, bool cmdStatu) 
        {
            Sid = sid;
            CmdSuccess = cmdStatu;

            WaitEvent.Set();

        }
    }
}
