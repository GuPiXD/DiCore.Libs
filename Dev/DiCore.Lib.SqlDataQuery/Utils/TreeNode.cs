using System;
using System.Collections.Generic;

namespace DiCore.Lib.SqlDataQuery.Utils
{
    public class TreeNode<TNode> where TNode: IComparable
    {
        public TreeNode(TNode nodeValue, TreeNode<TNode> parentNode)
        {
            ChildNodes = new List<TreeNode<TNode>>();
            Node = nodeValue;
            Parent = parentNode;
        }

        public TNode Node { get; set; }
        public TreeNode<TNode> Parent { get; set; }
        public List<TreeNode<TNode>> ChildNodes { get; set; }
    }
}