using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BodDetect
{
    public class LogUtil
    {
        #region 字段
        private static object _lock = new object();
        private static string _path = null;
        private static int _fileSize = 10 * 1024 * 1024; //日志分隔文件大小
        #endregion

        #region 写文件
        /// <summary>
        /// 写文件
        /// </summary>
        private static void WriteFile(string log, string path)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

                using (FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        #region 日志内容
                        string value = string.Format(@"{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), log);
                        #endregion

                        sw.WriteLine(value);
                        sw.Flush();
                    }
                    fs.Close();
                }
            }
            catch { }
        }
        #endregion

        #region 生成日志文件路径
        /// <summary>
        /// 生成日志文件路径
        /// </summary>
        private static string CreateLogPath(string folder)
        {
            if (_path == null)
            {
                UriBuilder uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
                _path = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
            }

            int index = 0;
            string logPath;
            bool bl = true;
            do
            {
                index++;
                logPath = Path.Combine(_path, folder + DateTime.Now.ToString("yyyyMMdd") + (index == 1 ? "" : "_" + index.ToString()) + ".txt");
                if (File.Exists(logPath))
                {
                    FileInfo fileInfo = new FileInfo(logPath);
                    if (fileInfo.Length < _fileSize)
                    {
                        bl = false;
                    }
                }
                else
                {
                    bl = false;
                }
            } while (bl);

            return logPath;
        }
        #endregion

        #region 写错误日志
        public static void LogError(Exception ex, string log = null)
        {
            LogError(string.IsNullOrEmpty(log) ? ex.Message + "\r\n" + ex.StackTrace : (log + "：") + ex.Message + "\r\n" + ex.StackTrace);
        }
        /// <summary>
        /// 写错误日志
        /// </summary>
        public static void LogError(string log)
        {
            Task.Factory.StartNew(() =>
            {
                lock (_lock)
                {
                    WriteFile("[Error] " + log, CreateLogPath("Log\\Error\\"));
                }
            });
        }
        #endregion

        #region 写操作日志
        /// <summary>
        /// 写操作日志
        /// </summary>
        public static void Log(string log)
        {
            Task.Factory.StartNew(() =>
            {
                lock (_lock)
                {
                    WriteFile("[Info]  " + log, CreateLogPath("Log\\Info\\"));
                }
            });
        }
        #endregion

    }
}
