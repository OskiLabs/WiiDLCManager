using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiiDLCManagerWinForms
{
    class PackManager
    {
        int PackId;
        DTAReader pReader = new DTAReader();
        public List<SongData> Songs;

        public bool GetSongsFromDTA()
        {
            return false;
        }

        public bool InsertSongs()
        {
            return false;
        }

        public bool DeleteSongs()
        {
            return false;
        }
    }
}
