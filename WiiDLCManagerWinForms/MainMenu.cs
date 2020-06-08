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
using System.IO;

namespace WiiDLCManagerWinForms
{
    public partial class MainMenu : Form
    {
        SongVizManager sVizzer;
        PackVizManager pVizzer;
        AppManager appek = new AppManager();


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

                pVizzer.packsCount(selectedRowCount, ids.Remove((ids.Length - 1), 1).ToString());
            }
        }

        private void songsPacksListBox_DoubleClick(object sender, EventArgs e)
        {
            songGridView.ClearSelection();

            DataGridViewRow row = songGridView.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[0].Value.ToString().Equals(songsPacksListBox.SelectedValue.ToString())).First();
            songGridView.Rows[row.Index].Selected = true;
            tabsControl.SelectedIndex = 0;
        }

        private void testButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog oFD = new OpenFileDialog();

            if (oFD.ShowDialog() == DialogResult.OK)
            {
                AppManager appek = new AppManager();
                appek.LoadPack(oFD.FileName);
                appek.Unpack();
            }
        }

        private void testFolderButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if(fbd.ShowDialog() == DialogResult.OK)
            {
                AppManager appek = new AppManager();
                appek.LoadFolder(fbd.SelectedPath);

                //appek.Pack(Directory.GetParent(fbd.SelectedPath).FullName + "\\wyn.app");
                //appek.UpdateTreeView(packTreeView);
                UpdateTreeView(appek.GetNodesTree(0), 1);
            }
        }

        private void testLoadButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog oFD = new OpenFileDialog();

            if (oFD.ShowDialog() == DialogResult.OK)
            {
                appek.LoadPack(oFD.FileName);
                //appek.UpdateTreeView(packTreeView);
                UpdateTreeView(appek.GetNodesTree(0), 1);
                testDtaButton.Enabled = true;
            }
        }

        private void packTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            testTagBox.Text = e.Node.Tag.ToString();
        }

        private void packTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            unpackNodeButton.Enabled = true;
        }

        private void unpackNodeButton_Click(object sender, EventArgs e)
        {
            appek.UnpackNode((int)packTreeView.SelectedNode.Tag);
        }

        private void testDtaButton_Click(object sender, EventArgs e)
        {
            appek.UnpackNode(appek.FindNode("songs.dta"));
        }

        private void testDeleteButton_Click(object sender, EventArgs e)
        {
            appek.DeleteNode((int)packTreeView.SelectedNode.Tag);
            appek.UpdateNodes();

            //appek.UpdateTreeView(packTreeView);
            UpdateTreeView(appek.GetNodesTree(0), 1);
        }

        private void testUpdateButton_Click(object sender, EventArgs e)
        {
            appek.UpdateAppFile();
        }

        private void addFileTestButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog oFD = new OpenFileDialog();

            if (oFD.ShowDialog() == DialogResult.OK)
            {
                appek.AddNode((int)packTreeView.SelectedNode.Tag, oFD.FileName);
                appek.UpdateNodes();

                //appek.UpdateTreeView(packTreeView);
                UpdateTreeView(appek.GetNodesTree(0), 1);
            }
        }

        private void testAddFolderButton_Click(object sender, EventArgs e)
        {

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                appek.AddNode((int)packTreeView.SelectedNode.Tag, fbd.SelectedPath);
                appek.UpdateNodes();

                //appek.UpdateTreeView(packTreeView);
                UpdateTreeView(appek.GetNodesTree(0), 1);
            }
        }

        private void testTreeButton_Click(object sender, EventArgs e)
        {
            UpdateTreeView(appek.GetNodesTree((int)packTreeView.SelectedNode.Tag), (int)packTreeView.SelectedNode.Tag + 1);
        }

        private void UpdateTreeView(List<string> nodes, int startNodeId)
        {
            packTreeView.BeginUpdate();
            packTreeView.Nodes.Clear();

            List<TreeNodeCollection> parents = new List<TreeNodeCollection>();
            parents.Add(packTreeView.Nodes);

            List<string> req = new List<string>();
            req.Add(nodes[0]);
            List<int> foldnum = new List<int>();
            foldnum.Add(0);

            for (int i = 0; i < nodes.Count; ++i)
            {

                if (nodes[i].Contains("."))
                {
                    parents[parents.Count - 1].Add(nodes[i].Substring(nodes[i].LastIndexOf("\\") + 1));
                    parents[parents.Count - 1][foldnum[foldnum.Count - 1]].Tag = i + startNodeId;
                    ++foldnum[foldnum.Count - 1];
                }
                else
                {
                    parents[parents.Count - 1].Add(nodes[i].Substring(nodes[i].LastIndexOf("\\") + 1));
                    parents[parents.Count - 1][foldnum[foldnum.Count - 1]].Tag = i + startNodeId;
                    parents.Add(parents[parents.Count - 1][foldnum[foldnum.Count - 1]].Nodes);

                    ++foldnum[foldnum.Count - 1];
                    foldnum.Add(0);
                    req.Add(nodes[i]);
                }

                while (i + 1 < nodes.Count && req.Count > 1 && req[req.Count - 1].Count(x => x == '\\') >= nodes[i + 1].Count(x => x == '\\'))
                {
                    req.RemoveAt(req.Count - 1);
                    parents.RemoveAt(parents.Count - 1);
                    foldnum.RemoveAt(foldnum.Count - 1);
                }
            }

            packTreeView.EndUpdate();
        }
    }
}
