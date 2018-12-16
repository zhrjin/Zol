using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zol.Common.DBHelper
{
    public class DBConnectionSetting
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string ConnName { set; get; }

        /// <summary>
        /// 数据库服务器IP
        /// </summary>
        public string ServerIp { set; get; }

        /// <summary>
        /// 数据库端口
        /// </summary>
        public int ServerPort { set; get; }

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DataSource { set; get; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserId { set; get; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { set; get; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public string DbType { set; get; }

    }
}
