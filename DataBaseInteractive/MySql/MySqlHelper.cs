using BodDetect.DataBaseInteractive.Sqlite;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace BodDetect.DataBaseInteractive
{
    public class MySqlHelper
    {
        public const string connRemoteStr = "Database=ddh_deep_tunne;datasource=192.168.10.253;port=3306;user=plc_user;pwd=20200904;";
        public static void InsertBodData(HisDataBaseModel hisDataBaseModel) 
        {
            string sql = "INSERT INTO HisDataBase(PH,DO,Temperature,Turbidity,Uv254,AirTemperature,Humidity,RunNum,Cod,Bod,BodElePot,BodElePotDrop,CreateDate,CreateTime) " +
    "VALUES(@PH,@DO,@Temperature,@Turbidity,@Uv254,@AirTemperature,@Humidity,@RunNum,@Cod,@Bod,@BodElePot,@BodElePotDrop,@CreateDate,@CreateTime)";

            object[] paramList = hisDataBaseModel.GetObjectList();
            ExecuteNonQuery(sql, paramList);
        }

        public static int ExecuteNonQuery( string commandText, params object[] paramList)
        {
            //  MySqlCommand cmd = cn.CreateCommand();
            MySqlConnection connRemote = new MySqlConnection(connRemoteStr);
            connRemote.Open();
            MySqlCommand cmd = new MySqlCommand(commandText, connRemote);
            int result = 0;
            try
            {
                cmd.CommandText = commandText;
                AttachParameters(cmd, commandText, paramList);
                result = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

                LogUtil.LogError(ex);
            }

            cmd.Dispose();

            return result;
        }


        public static MySqlParameterCollection AttachParameters(MySqlCommand cmd, string commandText, params object[] paramList) 
        {
            if (paramList == null || paramList.Length == 0) return null;

            MySqlParameterCollection coll = cmd.Parameters;
            string parmString = commandText.Substring(commandText.IndexOf("@"));
            // pre-process the string so always at least 1 space after a comma.
            parmString = parmString.Replace(",", " ,");
            // get the named parameters into a match collection
            string pattern = @"(@)\S*(.*?)\b";
            Regex ex = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection mc = ex.Matches(parmString);
            string[] paramNames = new string[mc.Count];
            int i = 0;
            foreach (Match m in mc)
            {
                paramNames[i] = m.Value;
                i++;
            }

            // now let's type the parameters
            int j = 0;
            Type t = null;
            foreach (object o in paramList)
            {
                t = o.GetType();

                MySqlParameter parm = new MySqlParameter();
                switch (t.ToString())
                {

                    case ("DBNull"):
                    case ("Char"):
                    case ("SByte"):
                    case ("UInt16"):
                    case ("UInt32"):
                    case ("UInt64"):
                        throw new SystemException("Invalid data type");


                    case ("System.String"):
                        parm.DbType = DbType.String;
                        parm.ParameterName = paramNames[j];
                        parm.Value = (string)paramList[j];
                        coll.Add(parm);
                        break;

                    case ("System.Byte[]"):
                        parm.DbType = DbType.Binary;
                        parm.ParameterName = paramNames[j];
                        parm.Value = (byte[])paramList[j];
                        coll.Add(parm);
                        break;

                    case ("System.Int32"):
                        parm.DbType = DbType.Int32;
                        parm.ParameterName = paramNames[j];
                        parm.Value = (int)paramList[j];
                        coll.Add(parm);
                        break;

                    case ("System.Boolean"):
                        parm.DbType = DbType.Boolean;
                        parm.ParameterName = paramNames[j];
                        parm.Value = (bool)paramList[j];
                        coll.Add(parm);
                        break;

                    case ("System.DateTime"):
                        parm.DbType = DbType.DateTime;
                        parm.ParameterName = paramNames[j];
                        parm.Value = Convert.ToDateTime(paramList[j]);
                        coll.Add(parm);
                        break;

                    case ("System.Double"):
                        parm.DbType = DbType.Double;
                        parm.ParameterName = paramNames[j];
                        parm.Value = Convert.ToDouble(paramList[j]);
                        coll.Add(parm);
                        break;

                    case ("System.Decimal"):
                        parm.DbType = DbType.Decimal;
                        parm.ParameterName = paramNames[j];
                        parm.Value = Convert.ToDecimal(paramList[j]);
                        break;

                    case ("System.Guid"):
                        parm.DbType = DbType.Guid;
                        parm.ParameterName = paramNames[j];
                        parm.Value = (System.Guid)(paramList[j]);
                        break;

                    case ("System.Object"):

                        parm.DbType = DbType.Object;
                        parm.ParameterName = paramNames[j];
                        parm.Value = paramList[j];
                        coll.Add(parm);
                        break;

                    default:
                        throw new SystemException("Value is of unknown data type");

                } // end switch

                j++;
            }
            return coll;
        }

    }
}
