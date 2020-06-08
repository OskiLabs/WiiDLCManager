using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace WiiDLCManagerWinForms
{
    class SongsManager
    {
        AppManager apper = new AppManager();
        DTAReader reader = new DTAReader();

        string dataSource;

        public SongsManager(string dbfile)
        {
            dataSource = "Data Source=" + dbfile;
        }

        /// <summary>
        /// Add songs and pack meta data from pack file to database. 
        /// </summary>
        /// <param name="path">Absolute path to pack file (app or bin) from songs will be imported</param>
        public void AddSongs(string path)
        {
            apper.LoadPack(path);
            int id = apper.FindNode("songs.dta");

            if (id > 0)
            {
                SQLiteConnection dbconn = new SQLiteConnection(dataSource);

                dbconn.Open();

                SQLiteCommand dbcomm = new SQLiteCommand(dbconn)
                {
                    CommandText = "INSERT INTO "
                };

                reader.ReadDTA(apper.GetNodeBytes(id));
                foreach (List<string> songMeta in reader.DTAEntries)
                {

                }
            }

        }
    }
}
