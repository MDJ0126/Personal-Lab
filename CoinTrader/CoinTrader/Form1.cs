using Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoinTrader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //ProtocolManager.GetHandler<HandlerAccount>().Request();
            //ProtocolManager.GetHandler<HandlerApiKey>().Request();
            ProtocolManager.GetHandler<HandlerTicker>().Request();
        }
    }
}
