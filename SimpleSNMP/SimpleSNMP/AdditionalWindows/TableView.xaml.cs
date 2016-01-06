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

namespace SimpleSNMP.AdditionalWindows
{
    /// <summary>
    /// Interaction logic for TableView.xaml
    /// </summary>
    public partial class TableView : Window
    {
        public TableView(Variable[,] table)
        {
            InitializeComponent();
            populateTable(table);
        }

        private void populateTable(Variable[,] table)
        {
            int columns = table.GetLength(1);
            int rows = table.GetLength(0);
            string[,] tempTable = new string[rows, columns];

            for(int i = 0; i < rows; i++)
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
            for (int i = 0; i < nbColumns; i++)
            {
                dt.Columns.Add(i.ToString(), typeof(string));
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
    

