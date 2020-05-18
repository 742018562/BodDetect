using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace BodDetect.UDP
{
    internal static class FinsDriver
    {
        //private const byte MemoryAreaData = 0x82;
        //private const byte MemoryWriteFlag = 0x31;
        //private const byte MemoryIO = 0x80;


        private const byte MemoryAreaWork = 0xB1;
        private static readonly byte[] CommandMemoryAreaRead = new byte[] { 0x01, 0x01 };
        private static readonly byte[] CommandMemoryAreaWrite = new byte[] { 0x01, 0x02 };

        public static byte[] ReadDataCommand(Header header, byte AreaCode , ushort startAddress, byte bitAddress, ushort readCount)
            => ReadCommand(AreaCode, header, startAddress, bitAddress, readCount);

        public static byte[] ReadWorkCommand(Header header, ushort startAddress, byte bitAddress, ushort readCount)
            => ReadCommand(MemoryAreaWork, header, startAddress, bitAddress, readCount);

        public static byte[] WriteDataCommand(Header header, byte AreaCode, ushort startAddress, byte bitAddress, ushort[] data)
            => WriteCommand(AreaCode, header, startAddress, bitAddress, data);

        public static byte[] WriteBitDataCommand(Header header, byte AreaCode, ushort startAddress, byte bitAddress, byte[] data)
            => WriteBitCommand(AreaCode, header, startAddress, bitAddress, data);

        public static byte[] WriteWorkCommand(Header header, ushort startAddress, byte bitAddress, ushort[] data)
            => WriteCommand(MemoryAreaWork, header, startAddress, bitAddress, data);

        public static void ProcessResponse(UdpReceiveResult received, FinsResponse[] responses)
        {
            var data = received.Buffer;
            byte finishCode1 = data[12],
                finishCode2 = data[13];

            var sid = data[9];
            if (finishCode1 != 0 || finishCode2 != 0)
            {
                responses[sid].CmdSuccess = false;
                throw new FinsException($"Failure code {finishCode1} {finishCode2}");
            }

            responses[sid].CmdSuccess = true;

            if (data.Length > 14)
            {
                responses[sid].PutValue(sid, data.Skip(14).ToArray());
            }
            else
            {
                responses[sid].PutValue(sid, true);
            }

        }

        private static byte[] ReadCommand(byte memoryArea, Header header, ushort startAddress, byte bitAddress, ushort readCount)
        {
            var ms = new BinaryWriter(new MemoryStream());
            header.WriteTo(ms);
            ms.Write(CommandMemoryAreaRead);
            ms.Write(memoryArea);
            ms.Write((byte)(startAddress >> 8));
            ms.Write((byte)startAddress);
            ms.Write(bitAddress); // Address Bit
            ms.Write((byte)(readCount >> 8));
            ms.Write((byte)readCount);
            return ((MemoryStream)ms.BaseStream).ToArray();
        }

        private static byte[] WriteCommand(byte memoryArea, Header header, ushort startAddress, byte bitAddress, ushort[] data)
        {
            var ms = new BinaryWriter(new MemoryStream());
            header.WriteTo(ms);
            ms.Write(CommandMemoryAreaWrite);
            ms.Write(memoryArea);
            ms.Write((byte)(startAddress >> 8));
            ms.Write((byte)startAddress);
            ms.Write(bitAddress); // Address Bit
            ms.Write((byte)(data.Length >> 8));
            ms.Write((byte)data.Length);
            ms.Write(Tool.ToBytes(data));
            return ((MemoryStream)ms.BaseStream).ToArray();
        }

        private static byte[] WriteBitCommand(byte memoryArea, Header header, ushort startAddress, byte bitAddress, byte[] data) 
        {
            var ms = new BinaryWriter(new MemoryStream());
            header.WriteTo(ms);
            ms.Write(CommandMemoryAreaWrite);
            ms.Write(memoryArea);
            ms.Write((byte)(startAddress >> 8));
            ms.Write((byte)startAddress);
            ms.Write(bitAddress); // Address Bit
            ms.Write((byte)(data.Length >> 8));
            ms.Write((byte)data.Length);
            ms.Write(data);
            return ((MemoryStream)ms.BaseStream).ToArray();
        }
    }

    public class Header
    {
        private readonly byte _icf, _rsv, _gct,
            _destinationNetworkAddress, _destinationAddress1, _destinationAddress2,
            _sourceNetworkAddress, _sourceAddress1, _sourceAddress2, _sid;

        public Header(byte sid, bool response)
        {
            _sid = sid; // Service id

            _icf = response ? (byte)0x80 : (byte)0x81;
            _rsv = 0;
            _gct = 0x02;
            _destinationNetworkAddress = 0;
            _destinationAddress1 = 0xAE;
            _destinationAddress2 = 0;
            _sourceNetworkAddress = 0;
            _sourceAddress1 = 0x21;
            _sourceAddress2 = 0;
        }

        public void WriteTo(BinaryWriter ms)
        {
            ms.Write(
                new[]
                {
                    _icf, _rsv, _gct,
                    _destinationNetworkAddress, _destinationAddress1, _destinationAddress2,
                    _sourceNetworkAddress, _sourceAddress1, _sourceAddress2,
                    _sid,
                });
        }
    }
}
