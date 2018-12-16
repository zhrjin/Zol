using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Zol.OrleansService
{
    public class SettingHelper : IDisposable
    {
        /// <summary> 
        /// 初始化服务配置帮助类 
        /// </summary> 
        public SettingHelper()
        {
            InitSettings();
        }

        /// <summary> 
        /// 系统用于标志此服务的名称 
        /// </summary> 
        public string ServiceName { get; private set; }

        /// <summary> 
        /// 初始化服务配置信息 
        /// </summary> 
        private void InitSettings()
        {
            string root = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string xmlfile = root.Remove(root.LastIndexOf('\\') + 1) + "DaHua.OrleansService.exe.config";
            if (File.Exists(xmlfile))
            {
                //系统用于标志此服务名称(唯一性)
                ServiceName = "DaHua.Orleans.Service." + Get_ConfigValue(xmlfile, "SiloPort");
            }
            else
            {
                throw new FileNotFoundException("未能找到服务名称配置文件 DaHua.OrleansService.exe.config！路径:" + xmlfile);
            }

        }
        /// <summary>
        /// 读取 XML中指定节点值
        /// </summary>
        /// <param name="configpath">配置文件路径</param>
        /// <param name="strKeyName">键值</param>        
        /// <returns></returns>
        protected static string Get_ConfigValue(string configpath, string strKeyName)
        {
            using (XmlTextReader tr = new XmlTextReader(configpath))
            {
                while (tr.Read())
                {
                    if (tr.NodeType == XmlNodeType.Element)
                    {
                        if (tr.Name == "add" && tr.GetAttribute("key") == strKeyName)
                        {
                            return tr.GetAttribute("value");
                        }
                    }
                }
            }
            return "";
        }

        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    ServiceName = null;
                }
            }
            disposed = true;
        }
        ~SettingHelper()
        {
            Dispose(false);
        }
    }
}
