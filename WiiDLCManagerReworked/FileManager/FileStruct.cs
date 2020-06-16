using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiiDLCManagerReworked.FileManager
{
    public struct FileStruct
    {
        private FileStructExplorer Explorer;
        private System.Collections.Generic.SortedList<int, WiiDLCManagerReworked.FileManager.Node> nodes;
        private string _filename;
        private string _datapath;
    }
}