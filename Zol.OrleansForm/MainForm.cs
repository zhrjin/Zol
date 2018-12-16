using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zol.Common;
using Zol.Core;

namespace Zol.OrleansForm
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                var bRet = Task.Run(SiloWrapper.GetInstance().StartAsync).GetAwaiter().GetResult();
                btnStart.Enabled = false; btnStop.Enabled = true;
                Logger.Debug("Silo started successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Silo started fail", ex);
                MessageBox.Show("启动失败!");
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                Task.Run(SiloWrapper.GetInstance().StopAsync);
                btnStart.Enabled = true;
                btnStop.Enabled = false;
                Logger.Debug("Silo stop successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Silo stop fail", ex);
                MessageBox.Show("启动失败!");
            }
        }
    }
}
