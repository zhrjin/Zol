using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zol.Common.DBHelper
{
    public class DBHelper
    {
        public static DataBase DBInstance(string dbConnectionStr)
        {
            string dbType;
            string sDBConnStr = DBConnectionHelper.GetDBConnectionSetting(dbConnectionStr, out dbType);
            if (dbType == "SqlServer")
            {
                return new SqlServerHelper(sDBConnStr);
            }
            else
            {
                return new DapperHelper(sDBConnStr);
            }
        }
    }
}
