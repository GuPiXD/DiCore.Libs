using System;

namespace DiCore.Lib.TankValidate
{
    internal class NodeClass
    {
        internal string Name { get; }
        internal string Text;
        //internal string Attribute;
        internal NodeClass Next;
        internal NodeClass Parent;
        internal NodeClass Child;
        internal bool Readed;
        internal int LineNumber { get; }

        internal Object Operand_1;
        internal Object Operand_2;

        //E.R. 20.11.2017 Извлечение подстроки из строки в рамках нового кода дефекта (дополнительные операнды)  см. ProcessorClass.DecodeOperands
        public Object Operand_3;
        public Object Operand_4;
        internal bool Result;
        //internal int MessType;

        internal NodeClass(string str, int lineN)
        {
            Name = str;
            Next = Parent = Child = null;
            Readed = false;
            Result = false;
            //MessType = 0;
            LineNumber = lineN;
            //Attribute = "";
        }
    }

    internal class TagsTree
    {
        private static NodeClass currentNode;
        internal NodeClass ReadedNode;
        internal NodeClass RootNode;

        internal TagsTree()
        {
            currentNode = null;
        }

        internal void Add(string str, int lineNumber)
        {
            NodeClass newNode = new NodeClass(str, lineNumber);
            if ((currentNode == null) || (currentNode.Readed == true))
            {
                if (currentNode != null)
                {
                    currentNode.Next = newNode;
                    newNode.Parent = currentNode.Parent;
                }
                else
                    RootNode = newNode;
                currentNode = newNode;
            }
            else
            {
                currentNode.Child = newNode;
                newNode.Parent = currentNode;
                currentNode = newNode;
            }
        }

        internal void EndElement(string str)
        {
            if (currentNode.Readed == false)
            {
                if (currentNode.Name == str)
                    currentNode.Readed = true;
                else
                    ; //error
            }
            else
            {
                if (currentNode.Parent != null)
                {
                    currentNode = currentNode.Parent;
                    EndElement(str);
                }
                else
                    ; //error
            }
        }

        internal void SetText(string str)
        {
            currentNode.Text = DeleteSpecChars(str);
        }

        private string DeleteSpecChars(string str)
        {
            return str.Replace("\n", "").Replace("\r", "").Replace("\t", "");
        }

        internal bool Read()
        {
            if ((currentNode.Child != null) && (currentNode.Child.Readed == false))
                while (currentNode.Child != null)
                    currentNode = currentNode.Child;
            ReadedNode = currentNode;
            currentNode.Readed = true;
            if (currentNode.Next != null)
                currentNode = currentNode.Next;
            else
            {
                if (currentNode.Parent != null)
                    currentNode = currentNode.Parent;
            }
            if (ReadedNode.Parent == null)
                return false;
            else
                return true;
        }

        internal void RefreshData(NodeClass startNode)
        {
            NodeClass tmpNode = startNode;
            while ((tmpNode.Next != null) || (tmpNode.Readed == true))
            {
                tmpNode.Readed = false;
                if (tmpNode.Child != null)
                    RefreshData(tmpNode.Child);
                if (tmpNode.Next != null)
                    tmpNode = tmpNode.Next;
            }
        }

        internal void Clear()
        {
            RootNode = null;
            currentNode = null;
        }

        internal void SetCurrentNode(NodeClass node)
        {
            currentNode = node;
        }
    }
}
