using System;
using System.Collections.Generic;
using System.Text;

namespace BodDetect
{
    class BodData
    {
        private byte[] revicePLCData;

        private byte[] sendPLCData;

        private byte[] reciveBodData;

        private byte[] sendBodData;

        float[] DoData;
        float[] PHData;
        uint[] TurbidityData;


        private Dictionary<string, int> sensorDataDic = new Dictionary<string, int>();

        public Dictionary<string, int> ValveCmdDic = new Dictionary<string, int>();

        public byte[] RevicePLCData { get => revicePLCData; set => revicePLCData = value; }
        public byte[] SendPLCData { get => sendPLCData; set => sendPLCData = value; }
        public byte[] ReciveBodData { get => reciveBodData; set => reciveBodData = value; }
        public byte[] SendBodData { get => sendBodData; set => sendBodData = value; }
    }
}
 