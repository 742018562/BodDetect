using System;
using System.Collections.Generic;
using System.Text;

namespace BodDetect.BodDataManage
{
    public class DelegateParam
    {
        private byte uid;
        private int data;
        private ProcessState state;
        private UiType uiType;


        public int Data { get => data; set => data = value; }
        public UiType UiType { get => uiType; set => uiType = value; }
        public ProcessState State { get => state; set => state = value; }
        public byte Uid { get => uid; set => uid = value; }

        public DelegateParam(byte id, int data, ProcessState state, UiType uiType) 
        {
            Uid = id;
            Data = data;
            State = state;
            UiType = uiType;
        }
    }
}
