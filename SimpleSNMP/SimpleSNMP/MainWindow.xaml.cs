using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using SimpleSNMP.AdditionalWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        private Socket socket;
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
        private void SNMP_RECEIVE()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 162);
            EndPoint ep = (EndPoint)ipep;

            this.Dispatcher.Invoke((Action)(() =>
            {
                button4.IsEnabled = false;
            }));

            socket.Bind(ep);
            // Disable timeout processing. Just block until packet is received 
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 0);
            bool run = true;
            int inlen = -1;
            while (run)
            {
                byte[] indata = new byte[16 * 1024];
                // 16KB receive buffer int inlen = 0;
                IPEndPoint peer = new IPEndPoint(IPAddress.Any, 0);
                EndPoint inep = (EndPoint)peer;
                try
                {

                    inlen = socket.ReceiveFrom(indata, ref inep);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception {0}", ex.Message);
                    inlen = -1;
                }
                if (inlen > 0)
                {
                    // Check protocol version int 
                    int ver = SnmpSharpNet.SnmpPacket.GetProtocolVersion(indata, inlen);
                    if (ver == (int)SnmpSharpNet.SnmpVersion.Ver1)
                    {
                        // Parse SNMP Version 1 TRAP packet 

                        SnmpSharpNet.SnmpV1TrapPacket pkt = new SnmpSharpNet.SnmpV1TrapPacket();
                        pkt.decode(indata, inlen);

                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            
                      
                        consoleBox.Items.Add(string.Format("** SNMP Version 1 TRAP received from {0}:", inep.ToString()));
                        consoleBox.Items.Add(string.Format("*** Trap generic: {0}", translateReceiver(pkt.Pdu.Generic)));
                        consoleBox.Items.Add(string.Format("*** Trap specific: {0}", pkt.Pdu.Specific));
                        consoleBox.Items.Add(string.Format("*** Agent address: {0}", pkt.Pdu.AgentAddress.ToString()));
                        consoleBox.Items.Add(string.Format("*** Timestamp: {0}", pkt.Pdu.TimeStamp.ToString()));
                        consoleBox.Items.Add(string.Format("*** VarBind count: {0}", pkt.Pdu.VbList.Count));
                        consoleBox.Items.Add(string.Format("*** VarBind content:"));
                        }));

                        foreach (SnmpSharpNet.Vb v in pkt.Pdu.VbList)
                        {
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                consoleBox.Items.Add(string.Format("**** {0} {1}: {2}", v.Oid.ToString(), SnmpSharpNet.SnmpConstants.GetTypeName(v.Value.Type), v.Value.ToString()));
                            }));
                        }
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            consoleBox.Items.Add("** End of SNMP Version 1 TRAP data.");
                        }));
                    }
                    else
                    {
                        // Parse SNMP Version 2 TRAP packet 
                        SnmpSharpNet.SnmpV2Packet pkt = new SnmpSharpNet.SnmpV2Packet();
                        pkt.decode(indata, inlen);
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            consoleBox.Items.Add(string.Format("** SNMP Version 2 TRAP received from {0}:", inep.ToString()));
                        }));
                        if ((SnmpSharpNet.PduType)pkt.Pdu.Type != SnmpSharpNet.PduType.V2Trap)
                        {
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                consoleBox.Items.Add("*** NOT an SNMPv2 trap ****");
                            }));
                        }
                        else
                        {
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                consoleBox.Items.Add(string.Format("*** Community: {0}", pkt.Community.ToString()));
                            consoleBox.Items.Add(string.Format("*** VarBind count: {0}", pkt.Pdu.VbList.Count));
                            consoleBox.Items.Add("*** VarBind content:");
                            }));
                            foreach (SnmpSharpNet.Vb v in pkt.Pdu.VbList)
                            {
                                this.Dispatcher.Invoke((Action)(() =>
                                {
                                    consoleBox.Items.Add(string.Format("**** {0} {1}: {2}",
                                   v.Oid.ToString(), SnmpSharpNet.SnmpConstants.GetTypeName(v.Value.Type), v.Value.ToString()));
                                }));
                            }
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                consoleBox.Items.Add("** End of SNMP Version 2 TRAP data.");
                            }));
                        }
                    }
                }
                else
                {
                    if (inlen == 0)
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            consoleBox.Items.Add("Zero length packet received.");
                        }));
                }
            }
           
        }
        private void killSocket()
        {
            try
            {
                socket.Close();
            }catch(Exception e)
            {

            }
        }
        private string translateReceiver(int output)
        {
            string result = "";

            switch (output)
            {
                case 0:
                    return "ColdStart";
                case 1:
                    return "WarmStart";
                case 2:
                    return "LinkDown";
                case 3:
                    return "LinkUp";
                case 4:
                    return "AuthenticationFailure";
                case 5:
                    return "EGPNeighbourLoss";
                case 6:
                    return "Other (Enterprise specific)";
                default:
                    return result;
            }
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

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            Thread th = new Thread(SNMP_RECEIVE);
            th.Start();
        }

        private void killerButton_Click(object sender, RoutedEventArgs e)
        {
            killSocket();
        }
    }
}
