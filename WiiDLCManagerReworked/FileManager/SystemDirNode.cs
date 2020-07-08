using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiiDLCManagerReworked.FileManager
{
    public class SystemDirNode : Node
    {
        private int _fullPath;

        public SystemDirNode(string path)
        {
            _children = null;
            _parent = null;
        }

        public override byte[] GetNodeByteArray()
        {
            throw new System.NotImplementedException();
        }
    }
}
