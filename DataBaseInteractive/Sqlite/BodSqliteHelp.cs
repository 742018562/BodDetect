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
                conn.Open();
                string sql = "SELECT * FROM HisDataBase";

                SQLiteDataAdapter ap = new SQLiteDataAdapter(sql, conn);

                DataSet ds = new DataSet();
                ap.Fill(ds);
                ap.Dispose();
                DataTable dt = ds.Tables[0];

                DataRow dataRow = dt.NewRow();
                dt.Rows.Add(dataRow);

                sql = "INSERT INTO HisDataBase(id,PH,DO,Temperature,Turbidity,Uv254,AirTemperature,Humidity,RunNum,Cod,Bod,BodElePot,BodElePotDrop,CreateDate,CreateTime) " +
                    "VALUES(@id,@PH,@DO,@Temperature,@Turbidity,@Uv254,@AirTemperature,@Humidity,@RunNum,@Cod,@Bod,@BodElePot,@BodElePotDrop,@CreateDate,@CreateTime)";

                object[] paramList = hisDatabasesModel.GetObjectList();

                SQLiteHelper.ExecuteNonQuery(conn, sql, paramList);

                conn.Close();
                conn.Dispose();
            }
        }

        public static List<HisDataBaseModel> SelectHisData() 
        {
            List<HisDataBaseModel> hisDataBaseModelList = new List<HisDataBaseModel>();
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
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
                    hisDataBaseModel.id = (int)item["id"];
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

                conn.Close();
                conn.Dispose();
            }

            return hisDataBaseModelList;
        }

        public void InsertHisBodData(List<HisDataBaseModel> hisDatabasesModel)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
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

                    sql = "INSERT INTO HisDataBase(id,PH,DO,Temperature,Turbidity,Uv254,AirTemperature,Humidity,RunNum,Cod,Bod,CreateDate,CreateTime) " +
                        "VALUES(@id,@PH,@DO,@Temperature,@Turbidity,@Uv254,@AirTemperature,@Humidity,@RunNum,@Cod,@Bod,@CreateDate,@CreateTime)";

                    object[] paramList = item.GetObjectList();

                    SQLiteHelper.ExecuteNonQuery(conn, sql, paramList);
                }

                conn.Close();
                conn.Dispose();
            }
        }

    }
}
