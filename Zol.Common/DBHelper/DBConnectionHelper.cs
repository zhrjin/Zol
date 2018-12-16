using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zol.Common.DBHelper
{
    /// <summary>
    /// 
    /// </summary>
    public class DBConnectionHelper
    {
        //JSON格式=[{ConnName:'',ServerIp:'',ServerPort:'',DataSource:'',UserId:'',Password:'',DbType:''}]

        /// <summary>
        /// 根据名称获取连接字符串
        /// </summary>
        /// <param name="dbName"></param>
        public static string GetDBConnectionSetting(string dbName)
        {
            string sDbType;
            return GetDBConnectionSetting(dbName, out sDbType);
        }

        /// <summary>
        /// 根据名称获取连接字符串
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="DbType">数据库类型 SqlServer </param>
        /// <returns></returns>
        public static string GetDBConnectionSetting(string dbName, out string DbType)
        {
            DbType = "";
            string sConnectionStr = "";
            List<DBConnectionSetting> modelList = new List<DBConnectionSetting>();
            string sSql = string.Format("SELECT t.FCONTENT FROM T_SYSTEM_SET t WHERE t.FFLAG = 'DbConnection'");
            string result = new DapperHelper().QueryFirst<string>(sSql);
            if (!string.IsNullOrEmpty(result))
            {
                modelList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DBConnectionSetting>>(result);
            }

            if (modelList != null && modelList.Count > 0 && modelList.Any(m => m.ConnName == dbName))
            {
                DBConnectionSetting model = modelList.First(m => m.ConnName == dbName);
                if (model != null)
                {
                    DbType = model.DbType;
                    if (model.DbType == "SqlServer")
                    {
                        sConnectionStr = string.Format("DATA SOURCE = {0};Initial Catalog={1}; PASSWORD={2};PERSIST SECURITY INFO=True;POOLING=False;USER ID = {3}",
                                               model.ServerIp, model.DataSource, model.Password, model.UserId);
                    }
                    else
                    {
                        sConnectionStr = string.Format("DATA SOURCE = {0}:{1}/{2};PASSWORD={3};PERSIST SECURITY INFO=True;POOLING=False;USER ID = {4}",
                            model.ServerIp, model.ServerPort, model.DataSource, model.Password, model.UserId);
                    }
                }
            }

            return sConnectionStr;
        }

    }
}
