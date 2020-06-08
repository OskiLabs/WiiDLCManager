using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WiiDLCManagerWinForms
{
    class AppManager
    {
        u8header header;
        List<u8node> nodes;
        string filename;

        ushort upd_nameOffset;
        string datapath;

        //Variables for exporting - Remember to export them when refactoring
        u8node curNode;
        u8node copyNode;

        public AppManager()
        {
            header = null;
            nodes = null;
            filename = null;

            curNode = null;

            upd_nameOffset = 0;
            datapath = "";
        }

        /// <summary>
        /// Creates App file in specified directory
        /// </summary>
        /// <param name="filen"></param>
        public void Pack(string filen)
        {
            filename = filen;
            FileStream app = File.Create(filename);

            app.Write(BitConverter.GetBytes(Be32(header.tag)), 0, 4);
            app.Write(BitConverter.GetBytes(Be32(header.rootnode_offset)), 0, 4);
            app.Write(BitConverter.GetBytes(Be32(header.header_size)), 0, 4);
            app.Write(BitConverter.GetBytes(Be32(header.data_offset)), 0, 4);
            app.Write(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, 16);
            app.Flush();

            foreach (u8node node in nodes)
            {
                app.Write(BitConverter.GetBytes(Be16(node.type)), 0, 2);
                app.Write(BitConverter.GetBytes(Be16(node.name_offset)), 0, 2);
                app.Write(BitConverter.GetBytes(Be32(node.data_offset)), 0, 4);
                app.Write(BitConverter.GetBytes(Be32(node.size)), 0, 4);
                app.Flush();
            }

            int stringsize = 0;
            foreach (u8node node in nodes)
            {
                app.Write(Encoding.Default.GetBytes(node.name), 0, node.name.Length);
                app.Write(new byte[] { 0x00 }, 0, 1);
                stringsize += node.name.Length + 1;
                app.Flush();
            }

            int paddingsize = (int)header.data_offset - 0x20 - (12 * nodes.Count) - stringsize;
            for (int i = 0; i < paddingsize; ++i)
            {
                app.Write(new byte[] { 0x00 }, 0, 1);
            }
            app.Flush();

            foreach (u8node node in nodes)
            {
                if (node.type == 0x0)
                {
                    FileStream cur_file = File.OpenRead(node.fullname);
                    byte[] cur_file_buffer = new byte[cur_file.Length];

                    cur_file.Read(cur_file_buffer, 0, (int)cur_file.Length);
                    app.Write(cur_file_buffer, 0, (int)cur_file.Length);

                    paddingsize = (int)Align((uint)cur_file.Length, 32) - (int)cur_file.Length;
                    for (int i = 0; i < paddingsize; ++i)
                    {
                        app.Write(new byte[] { 0x00 }, 0, 1);
                    }

                    app.Flush();
                }
            }

            app.Close();
        }

        /// <summary>
        /// Loads content from specified folder and parses it to App form
        /// </summary>
        /// <param name="foldername">Absolute path to folder from where content is loaded</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public void LoadFolder(string foldername)
        {
            nodes = new List<u8node>();
            offsets offs = new offsets();
            offs.names_offset = 1;
            offs.data_offset = 0;

            try
            {
                GetNodes(foldername, -1, offs, nodes);
            }
            catch
            {
                ClearAppManager();
                throw;
            }

            header = new u8header
            {
                tag = 0x55AA382D,
                rootnode_offset = 0x20,
                header_size = 12 * (uint)nodes.Count
            };

            foreach (u8node node in nodes)
            {
                header.header_size += (uint)node.name.Length + 1;
            }

            header.data_offset = Align(0x20 + header.header_size, 0x40);

            foreach (u8node node in nodes)
            {
                if (node.type == 0x0) node.data_offset += header.data_offset;
            }

            curNode = nodes[0];
            Console.WriteLine("End!");
        }

        /// <summary>
        /// Recursive function that creates nodes from specified directory
        /// </summary>
        /// <param name="dir">Directory from nodes are readed</param>
        /// <param name="recurs">Initial recursion value</param>
        /// <param name="offs">Initial names offset</param>
        /// <param name="cur_nodes">Reference to "u8node" List where nodes are saved</param>
        /// 
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        private void GetNodes(string dir, int recurs, offsets offs, List<u8node> cur_nodes)
        {
            string[] entries = Directory.GetFileSystemEntries(dir);
            u8node fdcur = new u8node();
            int fold_point;

            int last = dir.LastIndexOf("\\", dir.Length);
            fdcur.type = 0x0100;
            fdcur.children = new SortedDictionary<string,u8node>();
            if (cur_nodes.Count != 0) fdcur.name = dir.Substring(last + 1, dir.Length - last - 1);
            else fdcur.name = "";
            if (cur_nodes.Count != 0) fdcur.parent = cur_nodes[cur_nodes.Count - 1];
            if (cur_nodes.Count != 0) fdcur.parent.children.Add(fdcur.name,fdcur);
            if (cur_nodes.Count != 0) fdcur.data_offset = (uint)recurs;
            if (cur_nodes.Count != 0) fdcur.name_offset = (ushort)offs.names_offset;
            if (cur_nodes.Count != 0) offs.names_offset += fdcur.name.Length + 1;

            cur_nodes.Add(fdcur);
            fold_point = cur_nodes.Count - 1;

            foreach (string entry in entries)
            {
                if (entry.Contains("."))
                {
                    u8node flcur = new u8node();
                    last = entry.LastIndexOf("\\", entry.Length);
                    flcur.name = entry.Substring(last + 1, entry.Length - last - 1);
                    flcur.fullname = entry;
                    flcur.type = 0x0;
                    flcur.size = (uint)File.OpenRead(entry).Length;
                    flcur.name_offset = (ushort)offs.names_offset;
                    flcur.data_offset = (uint)offs.data_offset;
                    flcur.parent = fdcur;
                    fdcur.children.Add(flcur.name,flcur);

                    offs.names_offset += flcur.name.Length + 1;
                    offs.data_offset += (int)Align(flcur.size, 32);
                    cur_nodes.Add(flcur);
                }
                else
                {
                    GetNodes(entry, recurs + 1, offs, cur_nodes);
                }
            }

            cur_nodes[fold_point].size = (uint)cur_nodes.Count;
        }
        /// <summary>
        /// Loads content info from specified App file
        /// </summary>
        /// <param name="filen">Absolute path to App file, from which content info will be loaded</param>
        /// 
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="NotAppFileException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="UncompleteAppFileException"></exception>
        public void LoadPack(string filen)
        {
            header = new u8header();
            filename = filen;
            FileStream file = File.OpenRead(filename);
            datapath = Directory.GetParent(filename).FullName + "\\data.app";

            byte[] tempInt = new byte[4];
            byte[] tempShort = new byte[2];

            file.Read(tempInt, 0, 4);
            header.tag = BitConverter.ToUInt32(tempInt, 0);

            if (header.tag != 0x55AA382D)
            {
                throw new NotAppFileException();
            }

            try
            {
                file.Read(tempInt, 0, 4);
                header.rootnode_offset = Be32(BitConverter.ToUInt32(tempInt, 0));
                file.Read(tempInt, 0, 4);
                header.header_size = Be32(BitConverter.ToUInt32(tempInt, 0));
                file.Read(tempInt, 0, 4);
                header.data_offset = Be32(BitConverter.ToUInt32(tempInt, 0));

                Console.WriteLine(header.tag.ToString("X"));
                Console.WriteLine(header.rootnode_offset.ToString());
                Console.WriteLine(header.header_size.ToString());
                Console.WriteLine(header.data_offset.ToString());

                file.Seek(16, SeekOrigin.Current);

                ////Reading Root Node
                file.Seek(8, SeekOrigin.Current);
                file.Read(tempInt, 0, 4);
                uint numnodes = Be32(BitConverter.ToUInt32(tempInt, 0));
                nodes = new List<u8node>();

                Console.WriteLine(numnodes.ToString());
                uint stringsize = header.data_offset - header.rootnode_offset - numnodes * 12;
                Console.WriteLine(stringsize.ToString());
                uint datasize = (uint)file.Length - header.data_offset;
                Console.WriteLine(datasize.ToString());

                file.Seek(header.rootnode_offset, SeekOrigin.Begin);

                for (int i = 0; i < numnodes; ++i)
                {
                    u8node node = new u8node();
                    nodes.Add(node);

                    file.Read(tempShort, 0, 2);
                    nodes[i].type = Be16(BitConverter.ToUInt16(tempShort, 0));
                    file.Read(tempShort, 0, 2);
                    nodes[i].name_offset = Be16(BitConverter.ToUInt16(tempShort, 0));
                    file.Read(tempInt, 0, 4);
                    nodes[i].data_offset = Be32(BitConverter.ToUInt32(tempInt, 0));
                    file.Read(tempInt, 0, 4);
                    nodes[i].size = Be32(BitConverter.ToUInt32(tempInt, 0));
                    nodes[i].name = "";

                    if (nodes[i].type == 0x0100)
                    {
                        nodes[i].children = new SortedDictionary<string,u8node>();
                    }
                    else
                    {
                        nodes[i].children = null;
                    }
                }

                file.Seek(1, SeekOrigin.Current);

                byte[] chb = new byte[1];
                List<u8node> req = new List<u8node>
                {
                    nodes[0]
                };

                for (int i = 1; i < numnodes; ++i)
                {
                    List<byte> buff = new List<byte>();
                    file.Read(chb, 0, 1);
                    while (chb[0] != 0x0)
                    {
                        buff.Add(chb[0]);
                        file.Read(chb, 0, 1);
                    }

                    byte[] res = new byte[buff.Count];
                    for (int j = 0; j < buff.Count; ++j)
                    {
                        res[j] = buff[j];
                    }

                    nodes[i].name = Encoding.Default.GetString(res);
                    nodes[i].parent = req[req.Count - 1];
                    nodes[i].parent.children.Add(nodes[i].name,nodes[i]);

                    Console.WriteLine("Nodes " + (i + 1) + ":");
                    Console.WriteLine(nodes[i].type.ToString("X"));
                    Console.WriteLine(nodes[i].name_offset.ToString());
                    Console.WriteLine(nodes[i].data_offset.ToString());
                    Console.WriteLine(nodes[i].size.ToString());
                    Console.WriteLine(nodes[i].name);

                    if (nodes[i].type == 0x0100) req.Add(nodes[i]);

                    while (req.Count > 1 && req[req.Count - 1].size == i + 1)
                    {
                        req.RemoveAt(req.Count - 1);
                    }
                }

                file.Close();
                curNode = nodes[0];
                Console.WriteLine("End!");
            }
            catch(ArgumentOutOfRangeException)
            {
                ClearAppManager();
                throw new UncompleteAppFileException();
            }
        }

        /// <summary>
        /// Returns id of first found node specified by correspoding file or directory name. 
        /// </summary>
        /// <param name="nodename"></param>
        /// <returns></returns>
        public int FindNode(string nodename)
        {
            int res = -1;

            for (int i = 0; i < nodes.Count; ++i)
            {
                if (nodes[i].name == nodename)
                {
                    res = i;
                    break;
                }
            }

            return res;
        }

        private u8node GetNodeById(int nodeId)
        {
            return nodes[nodeId];
        }

        public void DeleteNode(int nodeId)
        {
            DeleteNode(GetNodeById(nodeId));
        }

        private void DeleteNode(u8node node)
        {
            node.parent.children.Remove(node.name);
        }

        /// <summary>
        /// Adds specified directory or file to App structure as child of specified parent node
        /// </summary>
        /// <param name="nodeId">Id of parent node</param>
        /// <param name="path">Absolute path to file or directory</param>
        public void AddNode(int nodeId, string path)
        {
            AddNode(GetNodeById(nodeId), path);
        }

        private void AddNode(u8node node, string path)
        {
            u8node newbie = new u8node();

            if (Directory.Exists(path))
            {
                newbie.type = 0x0100;
                newbie.name = path.Substring(path.LastIndexOf("\\") + 1);
                newbie.fullname = path;
                newbie.data_offset = node.size + 1;
                newbie.children = new SortedDictionary<string,u8node>();
                newbie.parent = node;


                newbie.parent.children.Add(newbie.name,newbie);

                foreach (string childpath in Directory.GetFileSystemEntries(path))
                {
                    AddNode(newbie, childpath);
                }

            }
            else if (File.Exists(path))
            {
                newbie.type = 0x0;
                newbie.name = path.Substring(path.LastIndexOf("\\") + 1);
                newbie.fullname = path;
                newbie.size = (uint)File.OpenRead(path).Length;
                newbie.children = null;

                newbie.parent = node;

                newbie.parent.children.Add(newbie.name,newbie);
            }
        }

        public byte[] GetNodeBytes(int nodeId)
        {
            return GetNodeBytes(nodes[nodeId]);
        }

        private byte[] GetNodeBytes(u8node node)
        {
            byte[] data = new byte[node.size];

            if (node.fullname == null)
            {
                FileStream app = File.OpenRead(filename);
                app.Seek(node.data_offset, SeekOrigin.Begin);
                app.Read(data, 0, (int)node.size);
            }
            else
            {
                FileStream app = File.OpenRead(node.fullname);
                app.Read(data, 0, (int)node.size);
            }

            return data;
        }

        public void UpdateNodes()
        {
            curNode = nodes[0];

            nodes.Clear();
            upd_nameOffset = 0;

            UpdateNext(curNode);
        }

        private void UpdateNext(u8node node)
        {
            node.name_offset = upd_nameOffset;
            upd_nameOffset += (ushort)(node.name.Length + 1);

            nodes.Add(node);

            if (node.type == 0x0100)
            {
                foreach (u8node child in node.children.Values)
                {
                    UpdateNext(child);
                }

                node.size = (uint)nodes.Count;
            }
        }

        /// <summary>
        /// Creates temporary file, where data for updated App file will be saved
        /// </summary>
        /// <exception cref="UnauthorizedAccessException"></exception>
        private void PrepareData()
        {
            FileStream datafile = File.Create(datapath);                            //Creating file where data will be saved

            foreach (u8node node in nodes)
            {
                if (node.type == 0x0)                                               //Only file type nodes
                {
                    if (node.fullname != null)                                      //Handle with new files (outside original App file)
                    {
                        FileStream curfile = File.OpenRead(node.fullname);          //Get filestream of current file
                        byte[] filebytes = new byte[curfile.Length];                //Byte array, where bytes of current data will be added
                        curfile.Read(filebytes, 0, filebytes.Length);               //Load bytes of current file to array
                        datafile.Write(filebytes, 0, filebytes.Length);             //Write bytes of current file to data file
                        curfile.Close();                                            //Close current file
                    }
                    else                                                            //Handle with old files (inside original App file)   
                    {
                        FileStream curfile = File.OpenRead(filename);               //Open original App file
                        curfile.Seek(node.data_offset, SeekOrigin.Begin);           //Set position to start of file data
                        byte[] filebytes = new byte[node.size];                     //Byte array, where bytes of current data will be added
                        curfile.Read(filebytes, 0, filebytes.Length);               //Load bytes of current file to array
                        datafile.Write(filebytes, 0, filebytes.Length);             //Write bytes of current file to data filestream

                        curfile.Close();                                            //Close original App file
                    }

                    datafile.Flush();                                               //Flush filestream to data file
                }
            }

            datafile.Close();                                                       //Close created data file
        }

        /// <summary>
        /// Updates App file
        /// </summary>
        /// <param name="backupFlag">Set true, if backup file of original App file must be created</param>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public void UpdateAppFile(bool backupFlag)
        {
            PrepareData();                                                          //Create temporary data file

            FileStream app = File.Create(filename + "_new");                        //Overwrite App File
            FileStream datafile = File.OpenRead(datapath);

            try
            {
                app.Write(BitConverter.GetBytes(header.tag), 0, 4);                 //Begin writing header to file with tag
                app.Write(BitConverter.GetBytes(Be32(header.rootnode_offset)), 0, 4);   //Write root node offset
                                                                                    //Update header size:
                header.header_size = 12 * (uint)nodes.Count;                        //Initial value
                foreach (u8node node in nodes)
                {
                    header.header_size += ((uint)node.name.Length + 1);             //Add each node's length of name + 1
                }
                //Update data offset value:
                header.data_offset = Align(0x20 + header.header_size, 0x40);
                foreach (u8node node in nodes)
                {
                    if (node.type == 0x0) node.data_offset += header.data_offset;
                }

                app.Write(BitConverter.GetBytes(Be32(header.header_size)), 0, 4);
                app.Write(BitConverter.GetBytes(Be32(header.data_offset)), 0, 4);

                app.Write(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, 16);
                app.Flush();

                uint dataoffset = Align(0x20 + header.header_size, 0x40);

                foreach (u8node node in nodes)                                      //Write nodes to file
                {
                    app.Write(BitConverter.GetBytes(Be16(node.type)), 0, 2);
                    app.Write(BitConverter.GetBytes(Be16(node.name_offset)), 0, 2);

                    if (node.type == 0x0)                                           //Count new data_offset
                    {
                        node.data_offset = dataoffset;
                        dataoffset += Align(node.size, 32);
                    }
                    app.Write(BitConverter.GetBytes(Be32(node.data_offset)), 0, 4);
                    app.Write(BitConverter.GetBytes(Be32(node.size)), 0, 4);
                    app.Flush();
                }

                int stringsize = 0;
                foreach (u8node node in nodes)
                {
                    app.Write(Encoding.Default.GetBytes(node.name), 0, node.name.Length);
                    app.Write(new byte[] { 0x00 }, 0, 1);
                    stringsize += node.name.Length + 1;
                    app.Flush();
                }

                int paddingsize = (int)header.data_offset - 0x20 - (12 * nodes.Count) - stringsize;
                for (int i = 0; i < paddingsize; ++i)
                {
                    app.Write(new byte[] { 0x00 }, 0, 1);
                }
                app.Flush();

                foreach (u8node node in nodes)
                {
                    if (node.type == 0x0)
                    {
                        byte[] cur_file_buffer = new byte[node.size];

                        datafile.Read(cur_file_buffer, 0, (int)node.size);
                        app.Write(cur_file_buffer, 0, (int)node.size);

                        paddingsize = (int)(Align(node.size, 32) - node.size);
                        for (int i = 0; i < paddingsize; ++i)
                        {
                            app.WriteByte(0x00);
                        }

                        app.Flush();
                    }
                }

                datafile.Close();
                File.Delete(datapath);
                app.Close();

                File.Replace(filename + "_new", filename, filename + "_original");

                if(!backupFlag)
                {
                    File.Delete(filename + "_original");
                }
            }
            catch
            {
                datafile.Close();
                File.Delete(datapath);

                app.Close();
                File.Delete(filename + "_new");

                throw;
            }
        }

        /// <summary>
        /// Unpacks node specified by nodeId, to same directory, where App file is
        /// </summary>
        /// <param name="nodeId">Id of App node, which corresponding file will be unpacked</param>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public void UnpackNode(int nodeId)
        {
            UnpackNode(nodes[nodeId], Directory.GetParent(filename).FullName);
        }

        /// <summary>
        /// Unpacks node specified by nodeId, to destination directory
        /// </summary>
        /// <param name="nodeId">Id of App node, which corresponding file will be unpacked</param>
        /// <param name="destination">Destination directory where file will be unpacked</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public void UnpackNode(int nodeId, string destination)
        {
            UnpackNode(nodes[nodeId], destination);
        }

        /// <summary>
        /// Recursive method that unpacks file or directory (with corresponding files and/or subdirectories) to specified directory
        /// </summary>
        /// <param name="node">Actually </param>
        /// <param name="curDir">Directory, where files or subdirectories will be created</param>
        private void UnpackNode(u8node node, string curDir)
        {

            if (node.type == 0x100)
            {
                curDir = curDir + "\\" + node.name;

                Directory.CreateDirectory(curDir);

                foreach (u8node child in node.children.Values)
                {
                    UnpackNode(child, curDir);
                }
            }
            else
            {

                FileStream file = File.OpenRead(filename);
                file.Seek(node.data_offset, SeekOrigin.Begin);

                FileStream newfile = File.Create(curDir + "\\" + node.name);
                Console.WriteLine(newfile.Name);

                byte[] fb = new byte[node.size];
                file.Read(fb, 0, (int)node.size);
                newfile.Write(fb, 0, fb.Length);
                newfile.Close();
            }
        }

        public void Unpack()
        {
            FileStream file = File.OpenRead(filename);
            Directory.CreateDirectory(Directory.GetParent(file.Name).FullName + "\\NewDepacked");
            String curDir = Directory.GetParent(file.Name).FullName + "\\NewDepacked\\";
            List<u8node> req = new List<u8node>
            {
                nodes[0]
            };
             
            for (int i = 1; i < nodes.Count; ++i)
            {

                if (nodes[i].type == 0x0)
                {
                    long cur = file.Position;
                    file.Seek(nodes[i].data_offset, SeekOrigin.Begin);
                    FileStream newfile = File.Create(curDir + "\\" + nodes[i].name);
                    Console.WriteLine(newfile.Name);

                    byte[] fb = new byte[nodes[i].size];
                    file.Read(fb, 0, (int)nodes[i].size);
                    newfile.Write(fb, 0, fb.Length);
                    newfile.Close();

                    file.Position = cur;
                }
                else if (nodes[i].type == 0x0100)
                {
                    Directory.CreateDirectory(curDir + "\\" + nodes[i].name);
                    curDir = curDir + "\\" + nodes[i].name;
                    req.Add(nodes[i]);
                }

                while (req.Count > 1 && req[req.Count - 1].size == i + 1)
                {
                    curDir = Directory.GetParent(curDir).FullName;
                    req.RemoveAt(req.Count - 1);
                }
            }

            file.Close();
        }

        public List<String> GetNodesTree(int nodeId)
        {
            return GetNodesTree(GetNodeById(nodeId), "");
        }

        private List<String> GetNodesTree(u8node node, string parents)
        {
            List<String> nodesTree = new List<string>();

            foreach (u8node child in node.children.Values)
            {
                if (child.type == 0x0)
                {
                    nodesTree.Add(parents + "\\" + child.name);
                }
                else
                {
                    nodesTree.Add(parents + "\\" + child.name);
                    nodesTree.AddRange(GetNodesTree(child, parents + "\\" + child.name));
                }
            }

            return nodesTree;
        }

        /// <summary>
        /// Clears all parameters of this App Manager object
        /// </summary>
        private void ClearAppManager()
        {
            header = null;
            nodes = null;
            filename = null;

            upd_nameOffset = 0;
            datapath = "";
        }

        #region Command Line Like Methods
        //These methods allow to move through App structure and edit it just like in normal file structure via command line.
        //They use "curNode" reference, that saves current directory node, where user actualy is.  
        //All of these methods have prefix "Cl", acronim for "Command Line".  

        /// <summary>
        /// Set current node parent as current node
        /// </summary>
        public void ClCurDir()
        {
            if (curNode.parent != null) curNode = curNode.parent;
        }

        /// <summary>
        /// Set node specified by the path as current node
        /// </summary>
        /// <param name="nodePath">Relative or absolute path to node</param>
        /// <exception cref="ArgumentException"></exception>
        public void ClCurNod(string nodePath)
        {
            int str = 0;
            String[] pathNodes = nodePath.Split('\\');

            u8node oldCurNode = curNode;

            if (pathNodes[0] == "root")
            {
                curNode = nodes[0];
                str = 1;
            }

            for(int i = str; i < pathNodes.Length; ++i)
            {
                curNode = curNode.children[pathNodes[i]];
            }

        }

        /// <summary>
        /// Returns string array of all children nodes of current node
        /// </summary>
        /// <returns>String array of names of current node children</returns>
        public string[] ClDir()
        {
            string[] children = new string[curNode.children.Count];
            int n = 0;

            foreach(string childName in curNode.children.Keys)
            {
                children[n++] = childName;
            }

            return children;
        }

        /// <summary>
        /// Adds specified file or directory to App structure as child of current node
        /// </summary>
        /// <param name="path">Absolute path to file or directory</param>
        public void ClAdd(string path)
        {
            AddNode(curNode, path);
        }

        /// <summary>
        /// Deletes specified node
        /// </summary>
        /// <param name="path">Relative or absolute path to node that will be deleted</param>
        public void ClDelete(string path)
        {
            u8node nextNode = curNode;
            ClCurNod(path);
            DeleteNode(curNode);
            curNode = nextNode;
        }

        /// <summary>
        /// Changes parent of source node to destination node
        /// </summary>
        /// <param name="sourcePath">Relative or absolute path to moved node</param>
        /// <param name="destinationPath">Relative or absolute path to new parent node</param>
        public void ClMove(string sourcePath, string destinationPath)
        {
            u8node sourceNode = ClGetNod(destinationPath);
            u8node destinationNode = ClGetNod(sourcePath);

            sourceNode.parent.children.Remove(sourceNode.name);
            sourceNode.parent = destinationNode;
            destinationNode.children.Add(sourceNode.name, sourceNode);
        }

        /// <summary>
        /// Saves specified node for future pasting or cutting
        /// </summary>
        /// <param name="nodePath">Relative or absolute path to copied node</param>
        public void ClCopy(string nodePath)
        {
            copyNode = ClGetNod(nodePath);
        }

        /// <summary>
        /// Copies previolusy saved node to specified directory node
        /// </summary>
        /// <param name="nodePath">Relative or absolute path to directory node to whom the node will be pasted</param>
        public void ClPaste(string nodePath)
        {
            u8node newParent = ClGetNod(nodePath);
            newParent.children.Add(copyNode.name, ClCopyNode(copyNode,newParent));
        }

        /// <summary>
        /// Cuts previoulusy saved node to specified directory node
        /// </summary>
        /// <param name="nodePath">Relative or absolute path to directory node to whom the node will be cutted</param>
        public void ClCut(string nodePath)
        {
            u8node newParent = ClGetNod(nodePath);
            newParent.children.Add(copyNode.name,copyNode);
            copyNode.parent.children.Remove(copyNode.name);
            copyNode.parent = newParent;
        }

        /// <summary>
        /// Returns node specified by path
        /// </summary>
        /// <param name="nodePath">Absolute or relative path to node</param>
        /// <returns>Reference to specified node</returns>
        private u8node ClGetNod(string nodePath)
        {
            int str = 0;
            String[] pathNodes = nodePath.Split('\\');

            u8node seekNode = curNode;

            if (pathNodes[0] == "root")
            {
                seekNode = nodes[0];
                str = 1;
            }

            for (int i = str; i < pathNodes.Length; ++i)
            {
                seekNode = seekNode.children[pathNodes[i]];
            }

            return seekNode;
        }

        /// <summary>
        /// Creates new node based on specified node
        /// </summary>
        /// <param name="copiedNode">Reference to copied node</param>
        /// <param name="newParentNode">Refernce to parent for created node</param>
        /// <returns>Reference to created node</returns>
        private u8node ClCopyNode(u8node copiedNode, u8node newParentNode)
        {
            u8node newbie = new u8node
            {
                name = copiedNode.name,
                fullname = copiedNode.name,
                name_offset = copiedNode.name_offset,
                parent = newParentNode
            };

            foreach (KeyValuePair<string,u8node> child in copiedNode.children)
            {
                newbie.children.Add(child.Key, ClCopyNode(child.Value, newbie));
            }

            return newbie;
        }

        #endregion

        #region File Stuff

        private ushort Be16(ushort x)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] bs = BitConverter.GetBytes(x);
                Array.Reverse(bs, 0, bs.Length);
                return BitConverter.ToUInt16(bs, 0);
            }
            else
            {
                return x;
            }
        }

        private uint Be32(uint x)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] bs = BitConverter.GetBytes(x);
                Array.Reverse(bs, 0, bs.Length);
                return BitConverter.ToUInt32(bs, 0);
            }
            else
            {
                return x;
            }
        }

        private uint Align(uint x, uint boundary)
        {
            if (x % boundary == 0)
                return x;
            else
                return x + boundary - (x % boundary);
        }

        #endregion
    }

    #region Classes of used Structures

    class offsets
    {
        public int names_offset { get; set; }
        public int data_offset { get; set; }
    }

    class u8node
    {
        public ushort type { get; set; } //this is 0x0000 for files and 0x0100 for folders
        public ushort name_offset { get; set; } //offset into the string table from the start of the string table
        public uint data_offset { get; set; }   // absolute offset from U.8- header but it is recursion for directories
        public uint size { get; set; }   // last included file num for directories

        public string name { get; set; }
        public string fullname { get; set; }

        public u8node parent { get; set; }
        public SortedDictionary<string,u8node> children { get; set; }

    }


    class u8header
    {
        public uint tag { get; set; } // 0x55AA382D "U.8-"
        public uint rootnode_offset { get; set; } // offset to root_node, always 0x20.
        public uint header_size { get; set; } // size of header from root_node to end of string table.
        public uint data_offset { get; set; } // offset to data -- this is rootnode_offset + header_size, aligned to 0x40.
    }

    #endregion

    #region Exceptions

    class NotAppFileException : Exception
    {

    }

    class UncompleteAppFileException : Exception
    {

    }

    #endregion
}