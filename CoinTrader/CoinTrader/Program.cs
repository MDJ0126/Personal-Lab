using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoinTrader
{
    static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 이미 실행 중인지 확인
            Mutex mutex = new Mutex(false, "F2D98EF4-4736-4DE4-BD7B-F8267D914387", out bool createNew);
            if (createNew)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            else
            {
                MessageBox.Show("이미 실행 중입니다.", "CoinTrader", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
