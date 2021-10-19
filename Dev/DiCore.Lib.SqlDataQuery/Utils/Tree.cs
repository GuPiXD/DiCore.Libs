using System;
using System.Collections.Generic;
using System.Linq;

namespace DiCore.Lib.SqlDataQuery.Utils
{
    public class Tree<T> where T : IComparable

    {
        public TreeNode<T> Root { get; set; }
        public List<TreeNode<T>> Nodes { get; set; }

        public void SetRoot(TreeNode<T> element)
        {
            Root = element;
        }

        public void AddChild(TreeNode<T> parent, TreeNode<T> element)
        {
            parent.ChildNodes.Add(element);
            Nodes.Add(element);
        }

        public void AddChilds(TreeNode<T> parent, List<TreeNode<T>> elements)
        {
            parent.ChildNodes.AddRange(elements);
            Nodes.AddRange(elements);
        }

        public TreeNode<T> GetNode(T element)
        {
            return Nodes.First(e => e.Node.CompareTo(element) == 0);
        }

        public List<T> GetPath(T element)
        {
            var result = new List<T>();
            var node = GetNode(element);
            while (node != null)
            {
                result.Add(node.Node);
                node = GetNode(node.Parent.Node);
            }
            return result;
        }
    }
}
