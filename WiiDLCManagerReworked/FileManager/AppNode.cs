using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiiDLCManagerReworked.FileManager
{
    public class AppNode : Node
    {
        private int _nameOffset;
        private int _dataOffset;
        private int _size;
        private int _appFilePath;

        public AppNode()
        {
            throw new System.NotImplementedException();
        }

        public override byte[] GetNodeByteArray()
        {
            throw new System.NotImplementedException();
        }
    }
}