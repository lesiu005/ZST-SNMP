using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using SimpleSNMP.AdditionalWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
        private string[] commands =
        {
            "GET",
            "GET-NEXT",
            "GET-BULK",
            "WALK"
        };

        public MainWindow()
        {
            InitializeComponent();
            actionBox.ItemsSource = commands.ToList();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string command = actionBox.SelectedItem.ToString();

            switch (command)
            {
               
                case "GET":
                    SNMP_GET();
                    break;
                case "GET-NEXT":
                    SNMP_GET_NEXT();
                    break;
                case "GET-BULK":
                    SNMP_GET_BULK();
                    break;
                case "WALK":
                    SNMP_WALK();
                    break;
            }
            
        }

        #region SNMP_Commands
        private void SNMP_GET()
        {
            string adres = addressBox.Text;
            var result = Messenger.Get(VersionCode.V1,
                           new IPEndPoint(IPAddress.Parse("127.0.0.1"), 161),
                           new OctetString("public"),
                           new List<Variable> { new Variable(new ObjectIdentifier(adres)) },
                           60000);
            foreach (var v in result)
            {
                consoleBox.Items.Add(v.ToString());
            }
        }
        private void SNMP_GET_NEXT()
        {
            string adres = addressBox.Text;
            GetNextRequestMessage message = new GetNextRequestMessage(0,
                              VersionCode.V1,
                              new OctetString("public"),
                              new List<Variable> { new Variable(new ObjectIdentifier(adres)) });
            ISnmpMessage response = message.GetResponse(60000, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 161));
            if (response.Pdu().ErrorStatus.ToInt32() != 0)
            {
                throw new Exception();
            }

            var result = response.Pdu().Variables;
            foreach (var v in result)
            {
                consoleBox.Items.Add(v.ToString());
            }
        }
        private void SNMP_GET_BULK()
        {
            string adres = addressBox.Text;
            GetBulkRequestMessage message = new GetBulkRequestMessage(0,
                      VersionCode.V2,
                      new OctetString("public"),
                      0,
                      10,
                      new List<Variable> { new Variable(new ObjectIdentifier(adres)) });
            ISnmpMessage response = message.GetResponse(60000, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 161));
            if (response.Pdu().ErrorStatus.ToInt32() != 0)
            {
                throw new Exception();
            }

            var result = response.Pdu().Variables;
            foreach (var v in result)
            {
                consoleBox.Items.Add(v.ToString());
            }
        }
        private void SNMP_WALK()
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
            foreach (var v in result)
            {
                consoleBox.Items.Add(v.ToString());
            }
        }
        private void SNMP_SET(string value,string adres)
        {
            
            var result = Messenger.Set(VersionCode.V1,
                           new IPEndPoint(IPAddress.Parse("127.0.0.1"), 161),
                           new OctetString("public"),
                           new List<Variable> { new Variable(new ObjectIdentifier(adres), new OctetString(value)) },
                           60000);
        }
        #endregion
        
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

            TableView tView = new TableView(result);
            tView.ShowDialog();

        }

        private void setButton_Click(object sender, RoutedEventArgs e)
        {
            string adres = setAdresBox.Text;
            string value = setValueBox.Text;

            SNMP_SET(value, adres);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            Thread th = new Thread(monit);
            th.Start();
        }
        private void monit()
        {
            string adres="";
            string time="";
            this.Dispatcher.Invoke((Action)(() =>
            {
            adres = monitorAdresBox.Text;
                time = timeBox.Text;
            }));
          

            var res1 = Messenger.Get(VersionCode.V1,
                           new IPEndPoint(IPAddress.Parse("127.0.0.1"), 161),
                           new OctetString("public"),
                           new List<Variable> { new Variable(new ObjectIdentifier(adres)) },
                           60000);

            System.Threading.Thread.Sleep(int.Parse(time));

            var res2 = Messenger.Get(VersionCode.V1,
                           new IPEndPoint(IPAddress.Parse("127.0.0.1"), 161),
                           new OctetString("public"),
                           new List<Variable> { new Variable(new ObjectIdentifier(adres)) },
                           60000);

            int result1 = prepareResult(res1[0].ToString());
            int result2 = prepareResult(res2[0].ToString());

            if (result1 == result2)
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    consoleBox.Items.Add(string.Format("Wartość nie uległa zmianie, wynosi {0}", result2));
                }));

            }
            else if (result1 > result2)
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    consoleBox.Items.Add(string.Format("Wartość zmniejszyła się. Wynosiła {0}, a teraz wynosi {1}.", result1, result2));
                }));
            }
            else
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    consoleBox.Items.Add(string.Format("Wartość zwiększyła się. Wynosiła {0}, a teraz wynosi {1}.", result1, result2));
                }));
            }
        }
        private int prepareResult(String result)
        {
            string tmp = result.Split(';')[1];
            string tmp2 = tmp.Split(':')[1];

            return int.Parse(tmp2);
        }
    }
}
