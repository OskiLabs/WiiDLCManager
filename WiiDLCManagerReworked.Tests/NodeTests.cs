using NUnit.Framework;
using System;
using WiiDLCManagerReworked.FileManager;

namespace WiiDLCManagerReworked.Tests
{
    public class NodeTests
    {
        internal class NodeMock : Node
        {
            public NodeMock(string name)
            {
                Name = name;
                Parent = null;
                _children = null;
                _type = NodeType.File;
            }

            public override byte[] GetNodeByteArray()
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void AddNewNodeAsChild_CheckProperConnections()
        {
            NodeMock parent = new NodeMock("parent");
            NodeMock child = new NodeMock("child");

            parent.AddChild(child);

            Assert.AreEqual(child, parent.Children[0]);
            Assert.AreEqual(parent, child.Parent);
        }
    }
}
