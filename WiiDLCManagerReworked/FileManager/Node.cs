using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiiDLCManagerReworked.FileManager
{
    public abstract class Node
    {

        public int Children
        {
            get => default;
            set
            {
            }
        }

        public int Parrent
        {
            get => default;
            set
            {
            }
        }

        public int Name
        {
            get => default;
            set
            {
            }
        }

        public int FullName
        {
            get => default;
            set
            {
            }
        }

        public int Type
        {
            get => default;
            set
            {
            }
        }

        public abstract byte[] GetNodeByteArray();
    }
}