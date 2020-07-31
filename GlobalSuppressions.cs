// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Globalization", "CA1305:指定 IFormatProvider", Justification = "<挂起>", Scope = "member", Target = "~M:BodDetect.MainWindow.MetroButton_Click_1(System.Object,System.Windows.RoutedEventArgs)")]
[assembly: SuppressMessage("Globalization", "CA1305:指定 IFormatProvider", Justification = "<挂起>", Scope = "member", Target = "~M:BodDetect.UDP.FinsClient.#ctor(System.Net.IPEndPoint,System.String)")]
[assembly: SuppressMessage("Globalization", "CA2101:指定对 P/Invoke 字符串参数进行封送处理", Justification = "<挂起>", Scope = "member", Target = "~M:BodDetect.Tool.FindWindow(System.String,System.String)~System.IntPtr")]
[assembly: SuppressMessage("Globalization", "CA2101:指定对 P/Invoke 字符串参数进行封送处理", Justification = "<挂起>", Scope = "member", Target = "~M:BodDetect.Tool.RegisterWindowMessage(System.String)~System.Int32")]
[assembly: SuppressMessage("Design", "CA1063:正确实现 IDisposable", Justification = "<挂起>", Scope = "type", Target = "~T:BodDetect.SerialPortHelp")]
[assembly: SuppressMessage("Design", "CA1063:正确实现 IDisposable", Justification = "<挂起>", Scope = "type", Target = "~T:BodDetect.UDP.FinsClient")]
[assembly: SuppressMessage("Design", "CA1060:将 pinvoke 移到本机方法类", Justification = "<挂起>", Scope = "type", Target = "~T:BodDetect.Tool")]
[assembly: SuppressMessage("Security", "CA2100:检查 SQL 查询是否存在安全漏洞", Justification = "<挂起>", Scope = "member", Target = "~M:BodDetect.DataBaseInteractive.Sqlite.SQLiteHelper.CreateCommand(System.Data.SQLite.SQLiteConnection,System.String,System.Data.SQLite.SQLiteParameter[])~System.Data.SQLite.SQLiteCommand")]
