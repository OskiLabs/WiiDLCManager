using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Data;


namespace WiiDLCManagerWinForms
{
    class PackVizManager
    {
        public Label Name { get; set; }
        public Label Gen { get; set; }
        public Label Path { get; set; }
        public Label Type { get; set; }
        public Label Size { get; set; }
        public Label Count { get; set; }
        public ListBox Songs { get; set; }

        private SQLiteConnection dbconn = new SQLiteConnection("Data Source=songlist.db");

        public void changePack(string packId)
        {
            dbconn.Open();
            SQLiteCommand dbcomm = new SQLiteCommand(dbconn)
            {
                CommandText = "Select Name, Gen, Path, Type, Size From Apps Where Id = " + packId
            };

            SQLiteDataReader dbreader = dbcomm.ExecuteReader();
            dbreader.Read();

            Name.Text = dbreader.GetString(0);
            Gen.Text = dbreader.GetString(1);
            Path.Text = dbreader.GetString(2);
            Type.Text = dbreader.GetString(3);

            Size.Text = ((dbreader.GetInt32(4) / 1000) > 0 ? (dbreader.GetInt32(4) / 1000).ToString() + " MB" : dbreader.GetInt32(4).ToString() + " KB");
            dbreader.Close();

            dbcomm.CommandText = "SELECT Id, (Name || ' - ' || Artist) AS Name  FROM Songs WHERE Meta_App = " + packId + " OR Song_App = " + packId;
            dbreader = dbcomm.ExecuteReader();
            DataTable dbtable = new DataTable();
            dbtable.Load(dbreader);

            Songs.DataSource = dbtable;
            Songs.DisplayMember = "Name";
            Songs.ValueMember = "ID";
            Count.Text = "Songs: " + Songs.Items.Count.ToString();

            dbreader.Close();
            dbconn.Close();
        }

        public void packsCount(int num, string ids)
        {
            Name.Text = "Packs: " + num.ToString();
            Gen.Text = null;
            Path.Text = null;
            Type.Text = null;
            Size.Text = null;
            Count.Text = null;

            dbconn.Open();
            SQLiteCommand dbcomm = new SQLiteCommand(dbconn)
            {
                CommandText = "Select SUM(Size) From Apps Where Id IN (" + ids + ")"
            };

            SQLiteDataReader dbreader = dbcomm.ExecuteReader();
            dbreader.Read();

            Size.Text = "Total Size: " + ((dbreader.GetInt32(0) / 1000) > 0 ? (dbreader.GetInt32(0) / 1000).ToString() + " MB" : dbreader.GetInt32(0).ToString() + " KB");

            dbreader.Close();
            dbconn.Close();

            Songs.DataSource = null;
        }
    }
}
