using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace BodDetect.Event
{
    public class ProcessInfoEventArgs : EventArgs
    {
        private int uid;
        private int data;

        public int Uid { get => uid; set => uid = value; }
        public int Data { get => data; set => data = value; }

        public ProcessInfoEventArgs(int id, int data)
        {
            Uid = id;
            Data = data;
        }

    }
}
