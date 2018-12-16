using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zol.Common.DBHelper
{
    public class SqlServerHelper : DataBase
    {
        public SqlServerHelper() : base()
        {

        }

        public SqlServerHelper(string strConn) : base(strConn)
        {
        }

        /// <summary>
        /// 获取连接
        /// </summary>
        /// <returns></returns>
        public override IDbConnection GetConnection()
        {
            SqlConnection conn = new SqlConnection(ConnString);
            conn.Open();
            return conn;
        }
    }
}
