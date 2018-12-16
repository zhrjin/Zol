using Zol.Common;
using Zol.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Zol.OrleansService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                var bRet = Task.Run(SiloWrapper.GetInstance().StartAsync).GetAwaiter().GetResult();
                Logger.Debug("Silo started successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Silo started fail", ex);
                throw ex;
            }
        }

        protected override void OnStop()
        {
            try
            {
                Task.Run(SiloWrapper.GetInstance().StopAsync);
                Logger.Debug("Silo stop successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Silo stop fail", ex);
                throw ex;
            }
        }
    }
}
