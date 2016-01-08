using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using SNMP_NMS_STATION.AdditionalWindows;
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

namespace SNMP_NMS_STATION
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Socket socket;
        private bool loop= false;
        private string[] commands =
        {
            "Pokaż tabelę",
            "Pokaż wartość obiektu skalarnego",
            "Nasłuchuj powiadomień",
            "Monitoruj stan obiektu skalarnego",
            "Zmień wartość obiektu skalarnego"
        };
        private List<string> scalarNames;
        private List<string> tablesNames;
        private SNMPCommandHandler handler;
        private Dictionary<string, string> tables;
        private Dictionary<string, string> scalars;

        public MainWindow()
        {
            InitializeComponent();
            initAll();

        }

        private void initAll()
        {
            initDictionaries();
            commandBox.ItemsSource = commands.ToList();
            handler = new SNMPCommandHandler();
        }

        private void initDictionaries()
        {
            tables = new Dictionary<string, string>() {
                {"ipAddrTable",".1.3.6.1.2.1.4.20" },
                {"ipRouteTable",".1.3.6.1.2.1.4.21" },
                {"ipNetToMediaTable",".1.3.6.1.2.1.4.22" }

            };

            tablesNames = tables.Keys.ToList();

            scalars = new Dictionary<string, string>()
            {
                {"ipForwarding",".1.3.6.1.2.1.4.1.0" },
                {"ipDefaultTTL",".1.3.6.1.2.1.4.2.0"},
                {"ipInReceives",".1.3.6.1.2.1.4.3.0" },
                {"ipInHdrErrors",".1.3.6.1.2.1.4.4.0" },
                {"ipInAddrErrors",".1.3.6.1.2.1.4.5.0" },
                {"ipForwDatagrams",".1.3.6.1.2.1.4.6.0" },
                {"ipInUnknownProtos",".1.3.6.1.2.1.4.7.0" },
                {"ipInDiscards",".1.3.6.1.2.1.4.8.0" },
                {"ipInDelivers",".1.3.6.1.2.1.4.9.0" },
                {"ipOutRequests",".1.3.6.1.2.1.4.10.0" },
                {"ipOutDiscards",".1.3.6.1.2.1.4.11.0" },
                {"ipOutNoRoutes",".1.3.6.1.2.1.4.12.0" },
                {"ipReasmTimeout",".1.3.6.1.2.1.4.13.0" },
                {"ipReasmReqds",".1.3.6.1.2.1.4.14.0" },
                {"ipReasmOKs",".1.3.6.1.2.1.4.15.0" },
                {"ipReasmFails",".1.3.6.1.2.1.4.16.0" },
                {"ipFragOKs",".1.3.6.1.2.1.4.17.0" },
                {"ipFragFails",".1.3.6.1.2.1.4.18.0" },
                {"ipFragCreates",".1.3.6.1.2.1.4.19.0" },
                {"ipRoutingDiscards",".1.3.6.1.2.1.4.23.0" }
            };
            scalarNames = scalars.Keys.ToList();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            console.Items.Clear();
        }
        private void add(Object value)
        {

            console.Items.Add(value.ToString());
        }

        private void commandBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selected = commandBox.SelectedIndex;

            switch (selected)
            {
                case 0:
                    selectBox.ItemsSource = tablesNames;
                    break;
                case 1:
                    selectBox.ItemsSource = scalarNames;
                    break;
                case 2:
                    selectBox.ItemsSource = scalarNames;
                    break;
                case 3:
                    selectBox.ItemsSource = scalarNames;
                    break;
                case 4:
                    selectBox.ItemsSource = scalarNames;
                    break;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            int command = commandBox.SelectedIndex;
            string obiekt;
            int obiektIndex = selectBox.SelectedIndex;
            string adres;
            switch (command)
            {
                case 0:
                    obiekt = selectBox.SelectedItem.ToString();
                    adres = tables[obiekt];
                    TableView tview = new TableView(handler.SNMP_GET_TABLE(adres), obiektIndex);
                    tview.ShowDialog();
                    break;
                case 1:
                    obiekt = selectBox.SelectedItem.ToString();
                    adres = scalars[obiekt];
                    add(obiekt+": "+handler.SNMP_GET(adres));
                    break;
                case 2:
                    Thread th = new Thread(SNMP_RECEIVE);
                    th.Start();
                    break;
                case 3:
                    obiekt = selectBox.SelectedItem.ToString();
                    adres = scalars[obiekt];
                    string time = valueBox.Text;
                    Thread thread = new Thread(() => SNMPMonit(adres, time));
                    thread.Start();
                    break;
                case 4:
                    obiekt = selectBox.SelectedItem.ToString();
                    adres = scalars[obiekt];
                    string value = valueBox.Text;
                    add(obiekt+": "+handler.SNMP_SET(adres, value));
                    break;
                default:
                    break;

            }
        }
        private int prepareResult(String result)
        {
            string tmp = result.Split(';')[1];
            string tmp2 = tmp.Split(':')[1];

            return int.Parse(tmp2);
        }
        public void SNMPMonit(string adres, string time)
        {
            // TODO
           // string time = valueBox.Text;
            
            var res1 = handler.SNMP_GET(adres);

            Thread.Sleep(int.Parse(time));

            var res2 = handler.SNMP_GET(adres);

            int result1 = prepareResult(res1.ToString());
            int result2 = prepareResult(res2.ToString());

            if (result1 == result2)
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    console.Items.Add(string.Format("Wartość nie uległa zmianie, wynosi {0}", result2));
                }));

            }
            else if (result1 > result2)
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    console.Items.Add(string.Format("Wartość zmniejszyła się. Wynosiła {0}, a teraz wynosi {1}.", result1, result2));
                }));
            }
            else
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    console.Items.Add(string.Format("Wartość zwiększyła się. Wynosiła {0}, a teraz wynosi {1}.", result1, result2));
                }));
            }



        }
        private void SNMP_RECEIVE()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 162);
            EndPoint ep = (EndPoint)ipep;


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


                            console.Items.Add(string.Format("** SNMP Version 1 TRAP received from {0}:", inep.ToString()));
                            console.Items.Add(string.Format("*** Trap generic: {0}", translateReceiver(pkt.Pdu.Generic)));
                            console.Items.Add(string.Format("*** Trap specific: {0}", pkt.Pdu.Specific));
                            console.Items.Add(string.Format("*** Agent address: {0}", pkt.Pdu.AgentAddress.ToString()));
                            console.Items.Add(string.Format("*** Timestamp: {0}", pkt.Pdu.TimeStamp.ToString()));
                            console.Items.Add(string.Format("*** VarBind count: {0}", pkt.Pdu.VbList.Count));
                            console.Items.Add(string.Format("*** VarBind content:"));
                        }));

                        foreach (SnmpSharpNet.Vb v in pkt.Pdu.VbList)
                        {
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                console.Items.Add(string.Format("**** {0} {1}: {2}", v.Oid.ToString(), SnmpSharpNet.SnmpConstants.GetTypeName(v.Value.Type), v.Value.ToString()));
                            }));
                        }
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            console.Items.Add("** End of SNMP Version 1 TRAP data.");
                        }));
                    }
                    else
                    {
                        // Parse SNMP Version 2 TRAP packet 
                        SnmpSharpNet.SnmpV2Packet pkt = new SnmpSharpNet.SnmpV2Packet();
                        pkt.decode(indata, inlen);
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            console.Items.Add(string.Format("** SNMP Version 2 TRAP received from {0}:", inep.ToString()));
                        }));
                        if ((SnmpSharpNet.PduType)pkt.Pdu.Type != SnmpSharpNet.PduType.V2Trap)
                        {
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                console.Items.Add("*** NOT an SNMPv2 trap ****");
                            }));
                        }
                        else
                        {
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                console.Items.Add(string.Format("*** Community: {0}", pkt.Community.ToString()));
                                console.Items.Add(string.Format("*** VarBind count: {0}", pkt.Pdu.VbList.Count));
                                console.Items.Add("*** VarBind content:");
                            }));
                            foreach (SnmpSharpNet.Vb v in pkt.Pdu.VbList)
                            {
                                this.Dispatcher.Invoke((Action)(() =>
                                {
                                    console.Items.Add(string.Format("**** {0} {1}: {2}",
                                   v.Oid.ToString(), SnmpSharpNet.SnmpConstants.GetTypeName(v.Value.Type), v.Value.ToString()));
                                }));
                            }
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                console.Items.Add("** End of SNMP Version 2 TRAP data.");
                            }));
                        }
                    }
                }
                else
                {
                    if (inlen == 0)
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            console.Items.Add("Zero length packet received.");
                        }));
                }
            }

        }
        private void killSocket()
        {
            try
            {
                socket.Close();
            }
            catch (Exception e)
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
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}