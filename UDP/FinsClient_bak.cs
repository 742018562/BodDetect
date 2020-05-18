//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;

//namespace BodDetect.UDP
//{
//    class FinsClient_bak : IDisposable
//    {
//        private string ip;
//        private int locaLport;
//        private int remotePort;
//        IPAddress localIp;
//        IPAddress pLCIp;
//        IPEndPoint locatePoint;
//        IPEndPoint remotePoint;


//        public int LocaLport { get => locaLport; set => locaLport = value; }
//        public string Ip { get => ip; set => ip = value; }
//        public int RemotePort { get => remotePort; set => remotePort = value; }
//        public IPAddress LocalIp { get => localIp; set => localIp = value; }
//        public IPAddress PLCIp { get => pLCIp; set => pLCIp = value; }
//        public IPEndPoint LocatePoint { get => locatePoint; set => locatePoint = value; }
//        public IPEndPoint RemotePoint { get => remotePoint; set => remotePoint = value; }


//         UdpClient _udpClient;

//        public bool IsConnect = false;


//        /// <summary>
//        /// 
//        /// </summary>
//        /// <returns></returns>
//        public bool connect()
//        {
//            try
//            {
//                if (IsConnect == true && _udpClient != null)
//                {
//                    return true;
//                }

//                string LcoIp = GetLocalIP();
//                if (string.IsNullOrEmpty(LcoIp))
//                {
//                    return false;
//                }

//                LocalIp = IPAddress.Parse(LcoIp);
//                LocatePoint = new IPEndPoint(LocalIp, LocaLport);
//                _udpClient = new UdpClient(LocatePoint);

//                PLCIp = IPAddress.Parse(Ip);
//                remotePoint = new IPEndPoint(PLCIp, remotePort);
//                ////监听创建好后，就开始接收信息，并创建一个线程
//                //Thread th = new Thread(Receive);
//                //th.IsBackground = true;
//                //th.Start();

//                return true;
//            }
//            catch (Exception ex)
//            {
//                return false;
//            }

//        }

//        public bool Receive(ref byte[] data)
//        {
//            //远端IP
//            //       udpClient.Connect(remotePoint);

//            try
//            {
//                byte[] recBuffer;

//                recBuffer = udpClient.Receive(ref remotePoint);

//                data = recBuffer;
//                return true;

//                //if (cBoxHex.Checked)
//                //{
//                //    rtxtRec.AppendText(StringtoHex(recBuffer) + "\r\n");

//                //}
//                //else
//                //{
//                //    int count = Convert.ToInt32(RtxtCount.Text);

//                //    if (recBuffer != null)
//                //    {
//                //        while (count > 0)
//                //        {

//                //            byte[] Num = new byte[2] { recBuffer[recBuffer.Length - count * 2], recBuffer[recBuffer.Length - count * 2 - 1] };
//                //            string zas = Tool.StringtoHex(Num);
//                //            Int32 xx = Convert.ToInt32(zas, 16);
//                //            rtxtRec.AppendText(xx + "\r\n");

//                //            count--;
//                //        }

//                //    }
//                //}
//            }
//            catch (Exception e)
//            {
//                return false;
//            }
//        }


//        public bool SendData()
    

//        /// <summary>
//        /// 获取本机ip
//        /// </summary>
//        /// <returns></returns>
//        public static string GetLocalIP()
//        {
//            try
//            {
//                string HostName = Dns.GetHostName(); //得到主机名
//                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
//                for (int i = 0; i < IpEntry.AddressList.Length; i++)
//                {
//                    //从IP地址列表中筛选出IPv4类型的IP地址
//                    //AddressFamily.InterNetwork表示此IP为IPv4,
//                    //AddressFamily.InterNetworkV6表示此地址为IPv6类型
//                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
//                    {
//                        string tempip = "";
//                        tempip = IpEntry.AddressList[i].ToString();
//                        return IpEntry.AddressList[i].ToString();
//                    }
//                }
//                return string.Empty;
//            }
//            catch (Exception ex)
//            {
//                return string.Empty;
//            }
//        }

//        public void Dispose()
//        {
//            _udpClient?.Dispose();
//        }
//    }
//}
