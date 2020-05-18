using System;
using System.Collections.Generic;
using System.Text;

namespace BodDetect.BodDataManage
{
    public class PLCParam
    {
        public ushort address;
        public List<byte> data = new List<byte>();
        public byte bitAddress;
        public byte memoryAreaCode;

        public PunpCapType punpCapType;
    }
}
