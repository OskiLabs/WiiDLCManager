using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiiDLCManagerReworked.FileManager
{
    public abstract class Node
    {
        protected List<Node> _children;
        protected string _name;
        protected Node _parent;
        protected NodeType _type;

        public List<Node> Children
        {
            get
            {
                return _children;
            }
        }

        public Node Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if(value == this)
                {
                    throw new SelfReferenceException();
                }
                else
                {
                    if (value.Parent != null) value.Parent.RemoveChild(value);
                    _parent = value;
                    if (value.Children.Contains(this)) value.AddChild(this);
                }
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public NodeType Type
        {
            get
            {
                return _type;
            }
        }

        public abstract byte[] GetNodeByteArray();

        /// <summary>
        /// Adds specified node to children list
        /// </summary>
        /// <param name="newbie">Added node</param>
        public void AddChild(Node newbie)
        {
            _children.Add(newbie);
            //if (newbie.Parent != this) newbie.Parent = this;
        }

        /// <summary>
        /// Removes specified node from children list
        /// </summary>
        /// <param name="removed">Removed node</param>
        public void RemoveChild(Node removed)
        {
            _children.Remove(removed);
            removed.Parent = null;
        }
    }

    class SelfReferenceException : Exception
    {

    }
}