using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zol.Common.Config
{
    public class AppConfigHelper
    {
        static AppConfigHelper()
        {
            InitSettings();
        }

        /// <summary> 
        /// 初始化服务配置信息 
        /// </summary> 
        private static void InitSettings()
        {
            ConnectionString = System.Configuration.ConfigurationManager.AppSettings["ConnectionString"];
            Invariant = System.Configuration.ConfigurationManager.AppSettings["Invariant"];
            ClusterId = System.Configuration.ConfigurationManager.AppSettings["ClusterId"];
            ServiceId = System.Configuration.ConfigurationManager.AppSettings["ServiceId"];

            string sSiloPort = System.Configuration.ConfigurationManager.AppSettings["SiloPort"];
            if (!string.IsNullOrEmpty(sSiloPort))
            {
                SiloPort = int.Parse(sSiloPort);
            }

            string sGatewayPort = System.Configuration.ConfigurationManager.AppSettings["GatewayPort"];
            if (!string.IsNullOrEmpty(sSiloPort))
            {
                GatewayPort = int.Parse(sGatewayPort);
            }
        }

        /// <summary>
        /// MQ连接字符串
        /// </summary>
        public static string ConnectionString { get; private set; }

        /// <summary>
        /// Invariant
        /// </summary>
        public static string Invariant { get; private set; }

        /// <summary>
        /// ClusterId
        /// </summary>
        public static string ClusterId { get; private set; }

        /// <summary>
        /// ServiceId
        /// </summary>
        public static string ServiceId { get; private set; }

        /// <summary>
        /// SiloPort
        /// </summary>
        public static int SiloPort { get; private set; }

        /// <summary>
        /// GatewayPort
        /// </summary>
        public static int GatewayPort { get; private set; }

    }
}
