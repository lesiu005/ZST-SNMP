using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleSNMP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string adres = addressBox.Text;
            var result = new List<Variable>();
            Messenger.Walk(VersionCode.V1,
                           new IPEndPoint(IPAddress.Parse("127.0.0.1"), 161),
                           new OctetString("public"),
                           new ObjectIdentifier(adres),
                           result,
                           30000,
                           WalkMode.WithinSubtree);
            
            foreach(var v in result)
            {
                consoleBox.Items.Add(v.ToString());
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            consoleBox.Items.Clear();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            string address = tableAddressBox.Text;

            var result = Messenger.GetTable(VersionCode.V2,
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), 161),
                new OctetString("public"),
                new ObjectIdentifier(address),
                60000,
                1,
                null);
            int a = 1;

        }
    }
}
