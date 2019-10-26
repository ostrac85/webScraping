using System;
using System.Configuration;
using System.Diagnostics;
using System.Windows.Forms;

namespace CGB
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string processName = ConfigurationManager.AppSettings["BankName"];
            if (Process.GetProcessesByName(processName).Length > 1)
            {
                MessageBox.Show($"Another {processName} process is already running.\r\nPlease close other {processName} process first.", processName);
                Application.Exit();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmSetParameter());
        }
    }
}
