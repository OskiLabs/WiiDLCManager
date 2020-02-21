using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiiDLCManagerWinForms
{
    public partial class MainMenu : Form
    {
        SongVizManager sVizzer;
        PackVizManager pVizzer;

        public MainMenu()
        {
            InitializeComponent();

            sVizzer = new SongVizManager()
            {
                AlbumImage = albumPictureBox,
                Name = songNameLabel,
                Artist = artistLabel,
                Album = albumNameLabel,
                Time = timeLabel,
                Year = yearLabel,
                Genre = genreLabel,

                GuitarDiff = guitarDiffPictureBox,
                BassDiff = bassDiffPictureBox,
                DrumsDiff = drumsDiffPictureBox,
                KeysDiff = keysDiffPictureBox,
                VocalsDiff = vocalsDiffPictureBox,
                ProGuitarDiff = proGuitarDiffPictureBox,
                ProBassDiff = proBassDiffPictureBox,
                ProDrumsDiff = proDrumsDiffPictureBox,
                ProKeysDiff = proKeysDiffPictureBox,
                HarmDiff = harmDiffPictureBox,

                GuitarLogo = guitarPictureBox,
                BassLogo = bassPictureBox,
                DrumsLogo = drumsPictureBox,
                KeysLogo = keysPictureBox,
                VocalsLogo = vocalsPictureBox,
                ProGuitarLogo = proGuitarPictureBox,
                ProBassLogo = proBassPictureBox,
                ProDrumsLogo = proDrumsPictureBox,
                ProKeysLogo = proKeysPictureBox,
                HarmLogo = harmPictureBox,

                Rating = ratingPictureBox,
                RatingLess = ratingLessButton,
                RatingMore = ratingMoreButton
            };

            pVizzer = new PackVizManager()
            {
                Name = namePacksLabel,
                Gen = genPacksLabel,
                Path = pathPacksLabel,
                Type = typePacksLabel,
                Size = sizePacksLabel,
                Count = countPacksLabel,
                Songs = songsPacksListBox
            };
        }


        private void MainMenu_Load(object sender, EventArgs e)
        {
            SQLiteConnection dbconn = new SQLiteConnection("Data Source=songlist.db");

            dbconn.Open();

            SQLiteCommand dbcomm = new SQLiteCommand(dbconn)
            {
                CommandText = "Select * From Songs"
            };

            SQLiteDataReader dbreader = dbcomm.ExecuteReader();
            DataTable dbtable = new DataTable();
            dbtable.Load(dbreader);
            songGridView.DataSource = dbtable;
            dbreader.Close();

            dbcomm.CommandText = "Select * From Apps";
            dbreader = dbcomm.ExecuteReader();
            dbtable = new DataTable();
            dbtable.Load(dbreader);
            packsGridView.DataSource = dbtable;
            dbreader.Close();
            dbconn.Close();

            songGridView.Columns[0].Visible = false;
            packsGridView.Columns[0].Visible = false;
        }

        private void songlistGridView_SelectionChanged(object sender, EventArgs e)
        {
            Int32 selectedRowCount = songGridView.Rows.GetRowCount(DataGridViewElementStates.Selected);
            if (selectedRowCount == 1)
            {
                string songid = songGridView.SelectedRows[0].Cells[0].Value.ToString();
                sVizzer.updateSongInfo(songid);
            }
            else
            {
                sVizzer.songsCount(selectedRowCount);
            }
        }

        private void ratingLessButton_Click(object sender, EventArgs e)
        {
            Int32 selectedRowCount = songGridView.Rows.GetRowCount(DataGridViewElementStates.Selected);
            if (selectedRowCount == 1)
            {
                string songid = songGridView.SelectedRows[0].Cells[0].Value.ToString();
                sVizzer.subRating(songid);
            }
        }

        private void ratingMoreButton_Click(object sender, EventArgs e)
        {
            Int32 selectedRowCount = songGridView.Rows.GetRowCount(DataGridViewElementStates.Selected);
            if (selectedRowCount == 1)
            {
                string songid = songGridView.SelectedRows[0].Cells[0].Value.ToString();
                sVizzer.addRating(songid);
            }
        }

        private void packsGridView_SelectionChanged(object sender, EventArgs e)
        {
            Int32 selectedRowCount = packsGridView.Rows.GetRowCount(DataGridViewElementStates.Selected);
            if (selectedRowCount == 1)
            {
                string packId = packsGridView.SelectedRows[0].Cells[0].Value.ToString();
                pVizzer.changePack(packId);
            }
            else if (selectedRowCount == 0)
            {

            }
            else
            {
                StringBuilder ids = new StringBuilder("");

                foreach (DataGridViewRow row in packsGridView.SelectedRows)
                {
                    ids.Append(row.Cells[0].Value.ToString() + ",");
                }

                pVizzer.packsCount(selectedRowCount,ids.Remove((ids.Length-1),1).ToString());
            }
        }

        private void songsPacksListBox_DoubleClick(object sender, EventArgs e)
        {
            songGridView.ClearSelection();

            DataGridViewRow row = songGridView.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[0].Value.ToString().Equals(songsPacksListBox.SelectedValue.ToString())).First();
            songGridView.Rows[row.Index].Selected = true;
            tabsControl.SelectedIndex = 0;
        }
    }
}
