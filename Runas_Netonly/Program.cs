using System;
using System.Threading;
using System.Windows.Forms;

namespace Runas_Netonly
{
    static class Program
    {
        static private Mutex _mutexObject = null;

        [STAThread]
        static void Main()
        {
            const string appName = "RUNAS_NETONLY_APP";
            _mutexObject = new Mutex(true, appName, out bool createdNew);
            if (!createdNew)
            {
                return;
            }

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
