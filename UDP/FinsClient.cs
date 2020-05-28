using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BodDetect.UDP
{
    /// <summary>
    /// Allows to write and read PLC memory
    /// </summary>
    public class FinsClient : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;
        private readonly UdpClient _udpClient;
        private readonly FinsResponse[] _responses = new FinsResponse[256];
        private readonly Thread _readerThread;
        private readonly object _lockObject = new object();
        private byte _sid;

        public static byte destinationAddress1;

        public static byte sourceAddress1;


        private string remoteIp { get; set; }

        public FinsClient(IPEndPoint remoteIpEndPoint,string RemoteIp)
        {
            _udpClient = new UdpClient();
            _udpClient.Connect(remoteIpEndPoint);

            remoteIp = RemoteIp;

            string[] value = remoteIp.Split('.');
            if (value.Length < 4)
            {
                return;
            }

            destinationAddress1 = Convert.ToByte(value[3]);

            string localeIp = Tool.GetLocalIP();

            value = localeIp.Split('.');
            if (value.Length < 4)
            {
                return;
            }

            sourceAddress1 = Convert.ToByte(value[3]);

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            _readerThread = new Thread(ReadWorker);
            _readerThread.Start();

            for (int i = 0; i < _responses.
                Length; i++)
                _responses[i] = new FinsResponse((byte)i, null,null);

            Timeout = TimeSpan.FromSeconds(2);
        }

        /// <summary>
        /// Gets or sets response timeout
        /// </summary>
        public TimeSpan Timeout { get; set; }

        public void Close()
        {
            _cancellationTokenSource.Cancel();
            _readerThread.Join();
        }

        /// <summary>
        /// Syncroniously reads specified number of ushorts starting from specified address in data memory
        /// </summary>
        /// <param name="startAddress">Address to start to read from</param>
        /// <param name="count">Number of ushorts to read</param>
        /// <returns>Read data</returns>
        public ushort[] ReadData(ushort startAddress, byte bitAddress,ushort count, byte memoryAreaCode)
        {
            var sid = IncrementSid();

           var cmd = FinsDriver.ReadDataCommand(new Header(sid, true), memoryAreaCode, startAddress, bitAddress, count);
            _responses[sid].RecType = RecDataType._uint16;
            return Read(sid, cmd).Data;
        }

        public float[] ReadBigFloatData(ushort startAddress, byte bitAddress ,ushort count, byte memoryAreaCode)
        {
            var sid = IncrementSid();
            var cmd = FinsDriver.ReadDataCommand(new Header(sid, true), memoryAreaCode, startAddress, bitAddress,count);
            _responses[sid].RecType = RecDataType.Big_float;
            return Read(sid, cmd).FloatData;
        }

        public float[] ReadLitFloatData(ushort startAddress, byte bitAddress,ushort count, byte memoryAreaCode)
        {
            var sid = IncrementSid();
            var cmd = FinsDriver.ReadDataCommand(new Header(sid, true), memoryAreaCode, startAddress, bitAddress, count);
            _responses[sid].RecType = RecDataType.Little_float;
            return Read(sid, cmd).FloatData;
        }

        public uint[] ReadIntData(ushort startAddress, byte bitAddress, ushort count, byte memoryAreaCode)
        {
            var sid = IncrementSid();
            var cmd = FinsDriver.ReadDataCommand(new Header(sid, true), memoryAreaCode, startAddress, bitAddress, count);
            _responses[sid].RecType = RecDataType._uint32;
            return Read(sid, cmd).Int32Data;
        }

        public float[] ReadDOFloatData(ushort startAddress, byte bitAddress, ushort count, byte memoryAreaCode)
        {
            var sid = IncrementSid();
            var cmd = FinsDriver.ReadDataCommand(new Header(sid, true), memoryAreaCode, startAddress, bitAddress, count);
            _responses[sid].RecType = RecDataType.DO_float;
            return Read(sid, cmd).FloatData;
        }

        /// <summary>
        /// Syncroniously reads specified number of ushorts starting from specified address in work memory
        /// </summary>
        /// <param name="startAddress">Address to start to read from</param>
        /// <param name="count">Number of ushorts to read</param>
        /// <returns>Read data</returns>
        public ushort[] ReadWork(ushort startAddress, byte bitAddress, ushort count)
        {
            var sid = IncrementSid();
            var cmd = FinsDriver.ReadWorkCommand(new Header(sid, true), startAddress, bitAddress, count);
            _responses[sid].RecType = RecDataType._uint16;
            return Read(sid, cmd).Data;
        }

        /// <summary>
        /// Syncroniously writes specified data to specified address of data memory
        /// </summary>
        /// <param name="startAddress">Address to start write to</param>
        /// <param name="data">Data to write</param>
        public bool WriteData(ushort startAddress, byte bitAddress, ushort[] data, byte memoryAreaCode)
        {
            try
            {
                var sid = IncrementSid();
                var cmd = FinsDriver.WriteDataCommand(new Header(sid, true), memoryAreaCode, startAddress, bitAddress, data);
                Write(sid, cmd);
                return true;
            }
            catch (TimeoutException)
            {

                return false;
            }
            catch (Exception)
            {
                return false;
            }


        }

        public bool WriteBitData(ushort startAddress, byte bitAddress, byte[] data, byte memoryAreaCode) 
        {
            try
            {
                var sid = IncrementSid();
                var cmd = FinsDriver.WriteBitDataCommand(new Header(sid, true), memoryAreaCode, startAddress, bitAddress, data);
                Write(sid, cmd);
                return true;
            }
            catch (TimeoutException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }

        }


        /// <summary>
        /// Syncroniously writes specified data to specified address of work memory
        /// </summary>
        /// <param name="startAddress">Address to start write to</param>
        /// <param name="data">Data to write</param>
        public void WriteWork(ushort startAddress, byte bitAddress, ushort[] data)
        {
            var sid = IncrementSid();
            var cmd = FinsDriver.WriteWorkCommand(new Header(sid, true), startAddress, bitAddress, data);
            Write(sid, cmd);
        }

        /// <summary>
        /// Asynchronously reads specified number of ushorts starting from specified address in data memory
        /// </summary>
        /// <param name="startAddress">Address to start to read from</param>
        /// <param name="count">Number of ushorts to read</param>
        /// <returns>Read data</returns>
        public async Task<ushort[]> ReadDataAsync(ushort startAddress, byte bitAddress, ushort count, byte memoryAreaCode)
        {
            var sid = IncrementSid();
            var cmd = FinsDriver.ReadDataCommand(new Header(sid, true), memoryAreaCode, startAddress, bitAddress, count);
            _responses[sid].RecType = RecDataType._uint16;
            return (await CommandAsync(sid, cmd)).Data;
        }

        public async Task<uint[]> ReadInd32DataAsync(ushort startAddress, byte bitAddress, ushort count, byte memoryAreaCode)
        {
            var sid = IncrementSid();
            var cmd = FinsDriver.ReadDataCommand(new Header(sid, true), memoryAreaCode, startAddress, bitAddress, count);
            _responses[sid].RecType = RecDataType._uint32;
            return (await CommandAsync(sid, cmd)).Int32Data;
        }

        public async Task<float[]> ReadLigFlaotDataAsync(ushort startAddress, byte bitAddress, ushort count, byte memoryAreaCode)
        {
            var sid = IncrementSid();
            var cmd = FinsDriver.ReadDataCommand(new Header(sid, true), memoryAreaCode, startAddress, bitAddress, count);
            _responses[sid].RecType = RecDataType.Little_float;
            return (await CommandAsync(sid, cmd)).FloatData;
        }

        public async Task<float[]> ReadBigFloatDataAsync(ushort startAddress, byte bitAddress, ushort count, byte memoryAreaCode)
        {
            var sid = IncrementSid();
            var cmd = FinsDriver.ReadDataCommand(new Header(sid, true), memoryAreaCode, startAddress, bitAddress, count);
            _responses[sid].RecType = RecDataType.Big_float;
            return (await CommandAsync(sid, cmd)).FloatData;
        }

        /// <summary>
        /// Asynchronously writes specified data to specified address of data memory
        /// </summary>
        /// <param name="startAddress">Address to start to write to</param>
        /// <param name="data">Data to write</param>
        public async Task WriteDataAsync(ushort startAddress, byte bitAddress, ushort[] data, byte memoryAreaCode)
        {
            var sid = IncrementSid();
            var cmd = FinsDriver.WriteDataCommand(new Header(sid, true), memoryAreaCode, startAddress, bitAddress, data);
            _responses[sid].RecType = RecDataType._uint16;
            await CommandAsync(sid, cmd);
        }

        /// <summary>
        /// Writes specified data to specified address of data memory without
        /// </summary>
        /// <param name="startAddress">Address to start to read from</param>
        /// <param name="count">Number of ushorts to read</param>
        public void WriteDataNoResponse(ushort startAddress, byte bitAddress, ushort[] data, byte memoryAreaCode)
        {
            var sid = IncrementSid();
            var cmd = FinsDriver.WriteDataCommand(new Header(sid, false), memoryAreaCode, startAddress, bitAddress, data);
            _udpClient.SendAsync(cmd, cmd.Length);
        }

        private byte IncrementSid()
        {
            byte sid;
            lock (_lockObject)
            {
                _sid++;
                sid = _sid;
            }

            _responses[sid].Reset();
            return sid;
        }

        private FinsResponse Read(byte sid, byte[] cmd)
        {

            if (_udpClient.Send(cmd, cmd.Length) != cmd.Length)
                throw new Exception();
            if (!_responses[sid].WaitEvent.WaitOne(Timeout))
                throw new TimeoutException();
            return _responses[sid];
        }

        private void Write(byte sid, byte[] cmd)
        {
            if (_udpClient.Send(cmd, cmd.Length) != cmd.Length)
                throw new Exception();
            if (!_responses[sid].WaitEvent.WaitOne(Timeout))
                throw new TimeoutException();
        }

        private async Task<FinsResponse> CommandAsync(byte sid, byte[] cmd)
        {
            if (await _udpClient.SendAsync(cmd, cmd.Length) != cmd.Length)
                throw new Exception();
            if (!_responses[sid].WaitEvent.WaitOne(Timeout))
                throw new TimeoutException();
            return _responses[sid];
        }

        private void ReadWorker()
        {
            try
            {
                while (true)
                {
                    var task = _udpClient.ReceiveAsync();
                    task.Wait(_cancellationToken);

                    if (task.IsFaulted)
                        throw new AggregateException(task.Exception);

                    FinsDriver.ProcessResponse(task.Result, _responses);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _readerThread?.Join();
            _udpClient?.Dispose();
        }
    }
}
