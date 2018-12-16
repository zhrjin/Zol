using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zol.Common
{
    public class Logger
    {
        public static readonly log4net.ILog Loginfo = log4net.LogManager.GetLogger("loginfo");
        public static readonly log4net.ILog Logerror = log4net.LogManager.GetLogger("logerror");
        public static readonly log4net.ILog Logwarning = log4net.LogManager.GetLogger("logwarn");

        public static void SetConfig()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static void SetConfig(FileInfo configFile)
        {
            log4net.Config.XmlConfigurator.Configure(configFile);
        }

        public static void Debug(string info)
        {
            if (Loginfo.IsInfoEnabled)
            {
                Loginfo.Info(info);
            }
        }

        public static void Error(string info, Exception se)
        {
            if (Logerror.IsErrorEnabled)
            {
                Logerror.Error(info, se);
            }
        }

        /// <summary>
        /// Warn
        /// </summary>
        /// <param name="info"></param>
        public static void Warn(string info)
        {
            if (Logwarning.IsWarnEnabled)
            {
                Logwarning.Warn(info);
            }
        }
    }
}
