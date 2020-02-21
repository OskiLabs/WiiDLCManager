using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Forms;
using System.Data.SQLite;

namespace WiiDLCManagerWinForms
{
    class SongVizManager
    {
        public PictureBox AlbumImage { get; set; }

        public Label Name { get; set; }
        public Label Artist { get; set; }
        public Label Album { get; set; }
        public Label Time { get; set; }
        public Label Year { get; set; }
        public Label Genre { get; set; }

        public PictureBox GuitarDiff { get; set; }
        public PictureBox BassDiff { get; set; }
        public PictureBox DrumsDiff { get; set; }
        public PictureBox KeysDiff { get; set; }
        public PictureBox VocalsDiff { get; set; }
        public PictureBox ProGuitarDiff { get; set; }
        public PictureBox ProBassDiff { get; set; }
        public PictureBox ProDrumsDiff { get; set; }
        public PictureBox ProKeysDiff { get; set; }
        public PictureBox HarmDiff { get; set; }

        public PictureBox GuitarLogo { get; set; }
        public PictureBox BassLogo { get; set; }
        public PictureBox DrumsLogo { get; set; }
        public PictureBox KeysLogo { get; set; }
        public PictureBox VocalsLogo { get; set; }
        public PictureBox ProGuitarLogo { get; set; }
        public PictureBox ProBassLogo { get; set; }
        public PictureBox ProDrumsLogo { get; set; }
        public PictureBox ProKeysLogo { get; set; }
        public PictureBox HarmLogo { get; set; }

        public PictureBox Rating { get; set; }
        public Button RatingLess { get; set; }
        public Button RatingMore { get; set; }

        private SQLiteConnection dbconn = new SQLiteConnection("Data Source=songlist.db");

        public SongVizManager() { }

        private void changeDiff(PictureBox instr, int diff)
        {
            switch (diff)
            {
                case 0:
                    instr.Image = WiiDLCManagerWinForms.Properties.Resources.diff0;
                    break;
                case 1:
                    instr.Image = WiiDLCManagerWinForms.Properties.Resources.diff1;
                    break;
                case 2:
                    instr.Image = WiiDLCManagerWinForms.Properties.Resources.diff2;
                    break;
                case 3:
                    instr.Image = WiiDLCManagerWinForms.Properties.Resources.diff3;
                    break;
                case 4:
                    instr.Image = WiiDLCManagerWinForms.Properties.Resources.diff4;
                    break;
                case 5:
                    instr.Image = WiiDLCManagerWinForms.Properties.Resources.diff5;
                    break;
                case 6:
                    instr.Image = WiiDLCManagerWinForms.Properties.Resources.diff6;
                    break;
                default:
                    instr.Image = WiiDLCManagerWinForms.Properties.Resources.nodiff;
                    break;
            }
        }

        private void changeVocals(int num, int diff)
        {
            switch (num)
            {
                case 1:
                    changeDiff(VocalsDiff, diff);
                    changeDiff(HarmDiff, -1);
                    HarmLogo.Image = WiiDLCManagerWinForms.Properties.Resources.harm3;
                    break;
                case 2:
                    changeDiff(VocalsDiff, diff);
                    changeDiff(HarmDiff, diff);
                    HarmLogo.Image = WiiDLCManagerWinForms.Properties.Resources.harm2;
                    break;
                case 3:
                    changeDiff(VocalsDiff, diff);
                    changeDiff(HarmDiff, diff);
                    HarmLogo.Image = WiiDLCManagerWinForms.Properties.Resources.harm3;
                    break;
                default:
                    changeDiff(VocalsDiff, 0);
                    changeDiff(HarmDiff, 0);
                    HarmLogo.Image = WiiDLCManagerWinForms.Properties.Resources.harm3;
                    break;
            }
        }
        private void updateRating(int rat)
        {
            switch (rat)
            {
                case 1:
                    Rating.Image = WiiDLCManagerWinForms.Properties.Resources.rev1;
                    break;
                case 2:
                    Rating.Image = WiiDLCManagerWinForms.Properties.Resources.rev2;
                    break;
                case 3:
                    Rating.Image = WiiDLCManagerWinForms.Properties.Resources.rev3;
                    break;
                case 4:
                    Rating.Image = WiiDLCManagerWinForms.Properties.Resources.rev4;
                    break;
                case 5:
                    Rating.Image = WiiDLCManagerWinForms.Properties.Resources.rev5;
                    break;
                default:
                    Rating.Image = WiiDLCManagerWinForms.Properties.Resources.rev0;
                    break;
            }
        }

        public void addRating(string songId)
        {
            dbconn.Open();
            SQLiteCommand dbcomm = new SQLiteCommand(dbconn)
            {
                CommandText = "SELECT My_Rating FROM Songs WHERE Id = " + songId
            };

            SQLiteDataReader dbreader = dbcomm.ExecuteReader();
            dbreader.Read();

            int actRat = dbreader.GetInt32(0);

            if (dbreader.GetInt32(0) < 5)
            {
                updateRating(++actRat);
            }

            dbreader.Close();

            dbcomm.CommandText = "UPDATE Songs SET My_Rating = " + actRat.ToString() + " WHERE Id = " + songId;
            dbcomm.ExecuteReader();

            dbconn.Close();
        }

        public void subRating(string songId)
        {
            dbconn.Open();
            SQLiteCommand dbcomm = new SQLiteCommand(dbconn)
            {
                CommandText = "SELECT My_Rating FROM Songs WHERE Id = " + songId
            };

            SQLiteDataReader dbreader = dbcomm.ExecuteReader();
            dbreader.Read();

            int actRat = dbreader.GetInt32(0);

            if (dbreader.GetInt32(0) > 0)
            {
                updateRating(--actRat);
            }

            dbreader.Close();

            dbcomm.CommandText = "UPDATE Songs SET My_Rating = " + actRat.ToString() + " WHERE Id = " + songId;
            dbcomm.ExecuteReader();

            dbconn.Close();
        }

        public void updateSongInfo(string songId)
        {
            dbconn.Open();
            SQLiteCommand dbcomm = new SQLiteCommand(dbconn)
            {
                CommandText = "Select Name, Artist, Year, Genre, Album, Length, Guitar_Difficulty, Bass_Difficulty, Drums_Difficulty, Keys_Difficulty, Vocals_Difficulty, Pro_Guitar_Difficulty, Pro_Bass_Difficulty, Pro_Keys_Difficulty, Vocals_Number, My_Rating From Songs Where Id = " + songId
            };

            SQLiteDataReader dbreader = dbcomm.ExecuteReader();
            dbreader.Read();

            Name.Text = dbreader.GetString(0);
            Artist.Text = dbreader.GetString(1);
            Year.Text = dbreader.GetInt32(2).ToString();
            Genre.Text = dbreader.GetString(3);
            Album.Text = dbreader.GetString(4);

            int timeMin = dbreader.GetInt32(5) / 60;
            int timeSec = dbreader.GetInt32(5) - (60 * timeMin);

            Time.Text = timeMin.ToString() + ":" + timeSec.ToString("D2");

            GuitarLogo.Image = WiiDLCManagerWinForms.Properties.Resources.guitar;
            BassLogo.Image = WiiDLCManagerWinForms.Properties.Resources.bass;
            DrumsLogo.Image = WiiDLCManagerWinForms.Properties.Resources.drums;
            KeysLogo.Image = WiiDLCManagerWinForms.Properties.Resources.keys;
            VocalsLogo.Image = WiiDLCManagerWinForms.Properties.Resources.vocals;
            ProGuitarLogo.Image = WiiDLCManagerWinForms.Properties.Resources.proguitar;
            ProBassLogo.Image = WiiDLCManagerWinForms.Properties.Resources.probass;
            ProDrumsLogo.Image = WiiDLCManagerWinForms.Properties.Resources.prodrums;
            ProKeysLogo.Image = WiiDLCManagerWinForms.Properties.Resources.prokeys;

            RatingLess.Visible = true;
            RatingMore.Visible = true;

            changeDiff(GuitarDiff, dbreader.GetInt32(6));
            changeDiff(BassDiff, dbreader.GetInt32(7));
            changeDiff(DrumsDiff, dbreader.GetInt32(8));
            changeDiff(ProDrumsDiff, dbreader.GetInt32(8));
            changeDiff(KeysDiff, dbreader.GetInt32(9));
            changeVocals(dbreader.GetInt32(14), dbreader.GetInt32(10));
            changeDiff(ProGuitarDiff, dbreader.GetInt32(11));
            changeDiff(ProBassDiff, dbreader.GetInt32(12));
            changeDiff(ProKeysDiff, dbreader.GetInt32(13));

            updateRating(dbreader.GetInt32(15));

            dbreader.Close();
            dbconn.Close();
        }

        public void songsCount(int num)
        {
            Name.Text = "Songs: " + num.ToString();
            AlbumImage.Visible = false;

            Artist.Text = "";
            Year.Text = "";
            Genre.Text = "";
            Album.Text = "";
            Time.Text = "";

            Rating.Image = null;
            RatingLess.Visible = false;
            RatingMore.Visible = false;

            GuitarDiff.Image = null;
            BassDiff.Image = null;
            DrumsDiff.Image = null;
            KeysDiff.Image = null;
            VocalsDiff.Image = null;
            ProGuitarDiff.Image = null;
            ProBassDiff.Image = null;
            ProDrumsDiff.Image = null;
            ProKeysDiff.Image = null;
            HarmDiff.Image = null;

            GuitarLogo.Image = null;
            BassLogo.Image = null;
            DrumsLogo.Image = null;
            KeysLogo.Image = null;
            VocalsLogo.Image = null;
            ProGuitarLogo.Image = null;
            ProBassLogo.Image = null;
            ProDrumsLogo.Image = null;
            ProKeysLogo.Image = null;
            HarmLogo.Image = null;
        }
    }
}
