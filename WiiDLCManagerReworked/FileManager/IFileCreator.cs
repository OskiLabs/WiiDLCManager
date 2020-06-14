using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiiDLCManagerReworked.FileManager
{
    interface IFileCreator
    {
        FileStruct Load();
        void Pack(FileStruct packStruct);
        void Repack();
    }
}
