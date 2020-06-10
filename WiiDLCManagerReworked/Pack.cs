using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WiiDLCManagerReworked.FileManager;

namespace WiiDLCManagerReworked
{
    public class Pack
    {
        private IFileCreator FileCreator;
        private Dictionary<String, Song> Songs;
        private FileStruct PackStruct;
    }
}