using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiiDLCManagerReworked.FileManager;
using NUnit.Framework;

namespace WiiDLCManager.Tests
{
    public class NodeTests
    {
        internal class NodeMock : Node
        {
            public NodeMock(string name)
            {
                Name = name;
                _parent = null;
                _children = new List<Node>();
                _type = NodeType.File;
            }

            public override byte[] GetNodeByteArray()
            {
                throw new NotImplementedException();
            }

            public void SetConnections(Node parent, List<Node> children)
            {
                _parent = parent;
                _children = children;
            }
        }

        [Test]
        public void AddNewNodeAsChild_CheckProperConnections()
        {
            /*NodeMock correctParentMock = new NodeMock("parent");
            NodeMock correctChildMock = new NodeMock("child");
            List<Node> correctChildren = new List<Node>
            {
                correctChildMock
            };

            correctParentMock.SetConnections(null, correctChildren);*/


            NodeMock parent = new NodeMock("parent");
            NodeMock child = new NodeMock("child");

            parent.AddChild(child);

            Assert.AreEqual(parent.Children.First(), child);
            Assert.AreEqual(parent, child.Parent);
        }

        [Test]
        public void ChangeParentByAddingChildToParent_CheckProperConnections()
        {
            NodeMock parent1 = new NodeMock("parent1");
            NodeMock parent2 = new NodeMock("parent2");
            NodeMock child = new NodeMock("child");

            parent1.AddChild(child);
            parent2.AddChild(child);

            Assert.AreEqual(parent2, child.Parent);
            Assert.AreEqual(0, parent1.Children.Count);
        }

        [Test]
        public void ChangeParentByChangingChildsParent_CheckProperConnections()
        {
            NodeMock parent1 = new NodeMock("parent1");
            NodeMock parent2 = new NodeMock("parent2");
            NodeMock child = new NodeMock("child");

            parent1.AddChild(child);

            child.Parent = parent2;

            Assert.AreEqual(parent2, child.Parent);
            Assert.AreEqual(0, parent1.Children.Count);
        }
    }
}
