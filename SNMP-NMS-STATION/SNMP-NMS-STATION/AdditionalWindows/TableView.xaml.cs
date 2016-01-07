 using Lextm.SharpSnmpLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SNMP_NMS_STATION.AdditionalWindows
{
    /// <summary>
    /// Interaction logic for TableView.xaml
    /// </summary>
    public partial class TableView : Window
    {
        private string[] ipAddrTable =
        {
            "ipAdEntAddr",
            "ipAdEntNetIfIndex",
            "ipAdEntNetMask",
            "ipAdEntBcasAddr",
            "ipAdEntReasmMaxSize"

        };
        private string[] ipRouteTable =
        {
            "ipRouteDest",
            "ipRouteIfIndex",
            "ipRouteMetric1",
            "ipRouteMetric2",
            "ipRouteMetric3",
            "ipRouteMetric4",
            "ipRouteNextHop",
            "ipRouteType",
            "ipRouteProto",
            "ipRouteAge",
            "ipRouteMask",
            "ipRouteMetric5",
            "ipRouteInfo"
        };
        private string[] ipNetTable =
        {
            "ipNetToMediaIfIndex",
            "ipNetToMediaPhysAddress",
            "ipNetToMedia",
            "ipNetToMediaNetAddress",
            "ipNetToMediaType"
        };
        public TableView(Variable[,] table,int selector)
        {
            InitializeComponent();
            
            populateTable(table,selector);
        }

        private void populateTable(Variable[,] table,int selector)
        {
            
            int columns = table.GetLength(1);
            int rows = table.GetLength(0);
            string[,] tempTable = new string[rows, columns];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    string[] tmp = table[i, j].ToString().Split(';');
                    string[] tmp2 = tmp[1].Split(':');

                    tempTable[i, j] = tmp2[1];
                }
            }


            DataTable dt = new DataTable();
            int nbColumns = columns;
            int nbRows = rows;
            string[] temp;
            if (selector == 0)
            {
                temp = ipAddrTable;
            }
            else if (selector == 1)
            {
                temp = ipRouteTable;
            }
            else
            {
                temp = ipNetTable;
            }
            for (int i = 0; i < nbColumns; i++)
            {
                dt.Columns.Add(temp[i], typeof(string));
            }

            for (int row = 0; row < nbRows; row++)
            {
                DataRow dr = dt.NewRow();
                for (int col = 0; col < nbColumns; col++)
                {
                    dr[col] = tempTable[row, col];
                }
                dt.Rows.Add(dr);
            }

            dataTable.ItemsSource = dt.DefaultView;




        }
    }

}


