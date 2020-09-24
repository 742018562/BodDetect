using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;

namespace BodDetect.DataBaseInteractive.Sqlite
{
    public class BodSqliteHelp
    {

        public static string connStr = @"Data Source=" + @"C:\sqlite\Test.db;Initial Catalog=sqlite;Integrated Security=True;Max Pool Size=10";

        public DataSet dataSet;

        public DataTable dataTable;



        public BodSqliteHelp()
        {

        }

        public static void InsertHisBodData(HisDataBaseModel hisDatabasesModel)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM HisDataBase";

                    SQLiteDataAdapter ap = new SQLiteDataAdapter(sql, conn);

                    DataSet ds = new DataSet();
                    ap.Fill(ds);
                    ap.Dispose();
                    DataTable dt = ds.Tables[0];

                    DataRow dataRow = dt.NewRow();
                    dt.Rows.Add(dataRow);

                    sql = "INSERT INTO HisDataBase(PH,DO,Temperature,Turbidity,Uv254,AirTemperature,Humidity,RunNum,Cod,Bod,BodElePot,BodElePotDrop,CreateDate,CreateTime) " +
                        "VALUES(@PH,@DO,@Temperature,@Turbidity,@Uv254,@AirTemperature,@Humidity,@RunNum,@Cod,@Bod,@BodElePot,@BodElePotDrop,@CreateDate,@CreateTime)";

                    object[] paramList = hisDatabasesModel.GetObjectList();

                    SQLiteHelper.ExecuteNonQuery(conn, sql, paramList);
                }
                catch (Exception ex)
                {
                    LogUtil.LogError(ex);
                }
                finally 
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        public static List<HisDataBaseModel> SelectHisData()
        {
            List<HisDataBaseModel> hisDataBaseModelList = new List<HisDataBaseModel>();
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM HisDataBase";

                    SQLiteDataAdapter ap = new SQLiteDataAdapter(sql, conn);

                    DataSet ds = new DataSet();
                    ap.Fill(ds);
                    ap.Dispose();


                    DataTable dt = ds.Tables[0];
                    foreach (DataRow item in dt.Rows)
                    {
                        HisDataBaseModel hisDataBaseModel = new HisDataBaseModel();
                        hisDataBaseModel.id =Convert.ToInt32( item["id"]);
                        hisDataBaseModel.Humidity = (double)item["Humidity"];
                        hisDataBaseModel.PH = (double)item["PH"];
                        hisDataBaseModel.RunNum = (int)item["RunNum"];
                        hisDataBaseModel.Temperature = (double)item["Temperature"];
                        hisDataBaseModel.Turbidity = (double)item["Turbidity"];
                        hisDataBaseModel.Uv254 = (double)item["Uv254"];
                        hisDataBaseModel.AirTemperature = (double)item["AirTemperature"];
                        hisDataBaseModel.Bod = (double)item["Bod"];
                        hisDataBaseModel.BodElePot = (double)item["BodElePot"];
                        hisDataBaseModel.BodElePotDrop = (double)item["BodElePotDrop"];
                        hisDataBaseModel.Cod = (double)item["Cod"];
                        hisDataBaseModel.CreateDate = (string)item["CreateDate"];
                        hisDataBaseModel.CreateTime = (string)item["CreateTime"];
                        hisDataBaseModel.DO = (double)item["DO"];
                        hisDataBaseModelList.Add(hisDataBaseModel);
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.LogError(ex);
                }
                finally 
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return hisDataBaseModelList;
        }


        public static List<SysStatusInfoModel> SelectSysStatusData()
        {
            List<SysStatusInfoModel> sysStatusInfoModelList = new List<SysStatusInfoModel>();
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM SysStatusInfo";

                    SQLiteDataAdapter ap = new SQLiteDataAdapter(sql, conn);

                    DataSet ds = new DataSet();
                    ap.Fill(ds);
                    ap.Dispose();


                    DataTable dt = ds.Tables[0];
                    foreach (DataRow item in dt.Rows)
                    {
                        SysStatusInfoModel sysStatusInfoModel = new SysStatusInfoModel();
                        sysStatusInfoModel.id = Convert.ToInt32(item["id"]);
                        sysStatusInfoModel.num = (int)item["num"];
                        sysStatusInfoModel.SysStatus = (string)item["sysStatus"];
                        sysStatusInfoModel.CreateDate = (string)item["CreateDate"];
                        sysStatusInfoModel.CreateTime = (string)item["CreateTime"];

                        sysStatusInfoModelList.Add(sysStatusInfoModel);
                    }
                }
                catch (Exception ex)
                {

                    LogUtil.LogError(ex);
                }
                finally 
                {
                    conn.Close();
                    conn.Dispose();
                }

            }

            return sysStatusInfoModelList;
        }

        public static void InsertHisBodData(List<HisDataBaseModel> hisDatabasesModel)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM HisDataBase";

                    SQLiteDataAdapter ap = new SQLiteDataAdapter(sql, conn);
                    DataSet ds = new DataSet();
                    ap.Fill(ds);
                    ap.Dispose();
                    DataTable dt = ds.Tables[0];

                    foreach (var item in hisDatabasesModel)
                    {
                        DataRow dataRow = dt.NewRow();
                        dt.Rows.Add(dataRow);

                        sql = "INSERT INTO HisDataBase(PH,DO,Temperature,Turbidity,Uv254,AirTemperature,Humidity,RunNum,Cod,Bod,CreateDate,CreateTime) " +
                            "VALUES(@PH,@DO,@Temperature,@Turbidity,@Uv254,@AirTemperature,@Humidity,@RunNum,@Cod,@Bod,@CreateDate,@CreateTime)";

                        object[] paramList = item.GetObjectList();

                        SQLiteHelper.ExecuteNonQuery(conn, sql, paramList);
                    }
                }
                catch (Exception ex)
                {

                    LogUtil.LogError(ex);
                }
                finally 
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        public static void InsertSysStatusData(List<SysStatusInfoModel> sysStatusInfoModel)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM SysStatusInfo";

                    SQLiteDataAdapter ap = new SQLiteDataAdapter(sql, conn);
                    DataSet ds = new DataSet();
                    ap.Fill(ds);
                    ap.Dispose();
                    DataTable dt = ds.Tables[0];

                    foreach (var item in sysStatusInfoModel)
                    {
                        DataRow dataRow = dt.NewRow();
                        dt.Rows.Add(dataRow);

                        sql = "INSERT INTO SysStatusInfo(num,sysStatus,CreateDate,CreateTime) " +
                            "VALUES(@num,@SysStatus,@CreateDate,@CreateTime)";

                        object[] paramList = item.GetObjectList();

                        SQLiteHelper.ExecuteNonQuery(conn, sql, paramList);
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
                finally 
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        public static void InsertSysStatusData(SysStatusInfoModel sysStatusInfoModel)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM SysStatusInfo";

                    SQLiteDataAdapter ap = new SQLiteDataAdapter(sql, conn);
                    DataSet ds = new DataSet();
                    ap.Fill(ds);
                    ap.Dispose();
                    DataTable dt = ds.Tables[0];


                    DataRow dataRow = dt.NewRow();
                    dt.Rows.Add(dataRow);

                    sql = "INSERT INTO SysStatusInfo(num,sysStatus,CreateDate,CreateTime) " +
                        "VALUES(@num,@SysStatus,@CreateDate,@CreateTime)";

                    object[] paramList = sysStatusInfoModel.GetObjectList();

                    SQLiteHelper.ExecuteNonQuery(conn, sql, paramList);
                }
                catch (Exception ex)
                {

                    LogUtil.LogError(ex);
                }
                finally 
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }


        public static void InsertAlramInfo(AlramInfoModel alramInfoModel)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM AlramInfo";

                    SQLiteDataAdapter ap = new SQLiteDataAdapter(sql, conn);

                    DataSet ds = new DataSet();
                    ap.Fill(ds);
                    ap.Dispose();
                    DataTable dt = ds.Tables[0];

                    DataRow dataRow = dt.NewRow();
                    dt.Rows.Add(dataRow);

                    sql = "INSERT INTO AlramInfo(DeviceInfo,ErrorCode,ErrorDes,HasHandle,CreateDate,CreateTime) " +
                        "VALUES(@DeviceInfo,@ErrorCode,@ErrorDes,@HasHandle,@CreateDate,@CreateTime)";

                    object[] paramList = alramInfoModel.GetObjectList();

                    SQLiteHelper.ExecuteNonQuery(conn, sql, paramList);
                }
                catch (Exception ex)
                {

                    LogUtil.LogError(ex);
                }
                finally 
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        public static void InsertAlramInfo(List<AlramInfoModel> alramInfoModelList)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM AlramInfo";

                    SQLiteDataAdapter ap = new SQLiteDataAdapter(sql, conn);

                    DataSet ds = new DataSet();
                    ap.Fill(ds);
                    ap.Dispose();
                    DataTable dt = ds.Tables[0];

                    foreach (var item in alramInfoModelList)
                    {
                        DataRow dataRow = dt.NewRow();
                        dt.Rows.Add(dataRow);

                        sql = "INSERT INTO AlramInfo(DeviceInfo,ErrorCode,ErrorDes,HasHandle,CreateDate,CreateTime) " +
                            "VALUES(@DeviceInfo,@ErrorCode,@ErrorDes,@HasHandle,@CreateDate,@CreateTime)";

                        object[] paramList = item.GetObjectList();

                        SQLiteHelper.ExecuteNonQuery(conn, sql, paramList);
                    }
                }
                catch (Exception ex)
                {

                    LogUtil.LogError(ex);
                }
                finally 
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        public static List<AlramInfoModel> SelectAlramInfo()
        {
            List<AlramInfoModel> alramInfoModelList = new List<AlramInfoModel>();
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM AlramInfo";

                    SQLiteDataAdapter ap = new SQLiteDataAdapter(sql, conn);

                    DataSet ds = new DataSet();
                    ap.Fill(ds);
                    ap.Dispose();


                    DataTable dt = ds.Tables[0];
                    foreach (DataRow item in dt.Rows)
                    {
                        AlramInfoModel alramInfoModel = new AlramInfoModel();
                        alramInfoModel.id = Convert.ToInt32(item["id"]);
                        alramInfoModel.DeviceInfo = (int)item["DeviceInfo"];
                        alramInfoModel.ErrorCode = (int)item["ErrorCode"];
                        alramInfoModel.ErrorDes = (string)item["ErrorDes"];
                        alramInfoModel.HasHandle = (Boolean)item["HasHandle"];
                        alramInfoModel.CreateDate = (string)item["CreateDate"];
                        alramInfoModel.CreateTime = (string)item["CreateTime"];

                        alramInfoModelList.Add(alramInfoModel);
                    }
                }
                catch (Exception ex)
                {

                    LogUtil.LogError(ex);
                }
                finally 
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return alramInfoModelList;
        }

        public static void InsertMaintainInfo(MaintainInfoModel maintainInfoModel)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM MaintainInfo";

                    SQLiteDataAdapter ap = new SQLiteDataAdapter(sql, conn);

                    DataSet ds = new DataSet();
                    ap.Fill(ds);
                    ap.Dispose();
                    DataTable dt = ds.Tables[0];

                    DataRow dataRow = dt.NewRow();
                    dt.Rows.Add(dataRow);

                    sql = "INSERT INTO MaintainInfo(Name,AlramId,Info,CreateDate,CreateTime) " +
                        "VALUES(@Name,@AlramId,@Info,@CreateDate,@CreateTime)";

                    object[] paramList = maintainInfoModel.GetObjectList();

                    SQLiteHelper.ExecuteNonQuery(conn, sql, paramList);
                }
                catch (Exception ex)
                {

                    LogUtil.LogError(ex);
                }
                finally 
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        public static List<MaintainInfoModel> SelectMaintainInfo()
        {
            List<MaintainInfoModel> maintainInfoModelList = new List<MaintainInfoModel>();
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM MaintainInfo";

                    SQLiteDataAdapter ap = new SQLiteDataAdapter(sql, conn);

                    DataSet ds = new DataSet();
                    ap.Fill(ds);
                    ap.Dispose();


                    DataTable dt = ds.Tables[0];
                    foreach (DataRow item in dt.Rows)
                    {
                        MaintainInfoModel maintainInfoModel = new MaintainInfoModel();
                        maintainInfoModel.id = Convert.ToInt32(item["id"]);
                        maintainInfoModel.Name = (string)item["Name"];
                        maintainInfoModel.AlramId = (string)item["AlramId"];
                        maintainInfoModel.Info = (string)item["Info"];
                        maintainInfoModel.CreateDate = (string)item["CreateDate"];
                        maintainInfoModel.CreateTime = (string)item["CreateTime"];

                        maintainInfoModelList.Add(maintainInfoModel);
                    }
                }
                catch (Exception ex)
                {

                    LogUtil.LogError(ex);
                }
                finally 
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return maintainInfoModelList;
        }


    }
}
