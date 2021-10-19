using System;
using System.Collections;

namespace DiCore.Lib.TankValidate
{
    internal class Variable
    {
        internal string Name { get; }
        internal double Value;

        internal Variable(string name)
        {
            Name = name;
        }

    }

    internal class VariablesList
    {
        internal ArrayList List { get; }

        internal VariablesList()
        {
            List = new ArrayList();
        }

        internal Variable Add(string name)
        {
            bool exist = false;
            int i;
            for (i = 0; (i <List.Count) && (exist == false); i++)
                if (((Variable)List[i]).Name == name)
                    exist = true;
            if (exist == false)
            {
                Variable newVar = new Variable(name);
                List.Add(newVar);
                return newVar;
            }
            else
                return (Variable)List[i - 1];
        }

        internal void Clear()
        {
            List.Clear();
        }

        internal int IndexOf(string name)
        {
            int res = -1;
            for (int i = 0; (i < List.Count) && (res < 0); i++)
                if (((Variable)(List[i])).Name == name)
                    res = i;
            return res;
        }
  


    }

    internal class CellAddr
    {
        internal string Sheet;
        internal int Row;
        internal int Column;
        internal int Offset;

        internal CellAddr()
        {
            Sheet = "";
            Row = Column = Offset = 0;
        }

        internal void CopyAddr(CellAddr source)
        {
            Sheet = source.Sheet;
            Column = source.Column;
            Row = source.Row;
            Offset = source.Offset;
        }

        internal void DecodeAddr(string str)
        {
            int colonPos = str.IndexOf(':');
            Sheet = str.Substring(1, colonPos - 1);
            Sheet = Sheet.Trim();
            string cell = str.Substring(colonPos + 1, str.Length - colonPos - 2);
            cell = cell.Trim();
            Row = Convert.ToInt32(cell.Substring(cell.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }),
                    cell.Length - cell.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' })));
            string colStr = cell.Substring(0, cell.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }));
            if (colStr.Length == 1)
                Column = colStr.ToUpper()[0] - 'A' + 1;
            else
                Column = (colStr.ToUpper()[0] - 'A' + 1) * 26 + colStr.ToUpper()[1] - 'A' + 1;
        }
    }

    internal class Pointer
    {
        internal string Name { get; }
        internal CellAddr Cell { get; }
        //internal int Offset;

        internal Pointer(string name)
        {
            Name = name;
            Cell = new CellAddr();
            //Offset = 0;
        }
    }

    internal class PointersList
    {
        internal ArrayList List { get; }

        internal PointersList()
        {
            List = new ArrayList();
        }

        internal Pointer Add(string name)
        {
            bool exist = false;
            int i;
            for (i = 0; (i < List.Count) && (exist == false); i++)
                if (((Pointer)List[i]).Name == name)
                    exist = true;
            if (exist == false)
            {
                Pointer newPointer = new Pointer(name);
                List.Add(newPointer);
                return newPointer;
            }
            else
            {
                return (Pointer)List[i - 1];
            }
        }

        internal void Clear()
        {
            List.Clear();
        }

        internal int IndexOf(string name)
        {
            int res = -1;
            for (int i = 0; (i < List.Count) && (res < 0); i++)
                if (((Pointer)(List[i])).Name == name)
                    res = i;
            return res;
        }
    }

    internal class StringVar
    {
        internal string Name { get; }
        internal string Value;

        internal StringVar(string str)
        {
            Name = str;
        }
    }

    internal class StringsList
    {

        internal ArrayList List { get; }

        internal StringsList()
        {
            List = new ArrayList();
        }

        internal StringVar Add(string name)
        {
            bool exist = false;
            int i;
            for (i = 0; (i < List.Count) && (exist == false); i++)
                if (((StringVar)List[i]).Name == name)
                    exist = true;
            if (exist == false)
            {
                StringVar newStr = new StringVar(name);
                //newStr.Value = value;
                List.Add(newStr);
                return newStr;
            }
            else
                return (StringVar)List[i - 1];
        }

        internal void Clear()
        {
            List.Clear();
        }

        internal int IndexOf(string varName)
        {
            int res = -1;
            for (int i = 0; (i < List.Count) && (res < 0); i++)
                if (((StringVar)(List[i])).Name == varName)
                    res = i;
            return res;
        }
    }

    internal class StringArray
    {
        internal string Name { get; }
        internal ArrayList Array { get; }
        internal StringArray(string name)
        {
            Name = name;
            Array = new ArrayList();
        }
    }

    internal class StringArrayList
    {
        internal ArrayList List { get; }

        internal StringArrayList()
        {
            List = new ArrayList();
        }

        internal StringArray Add(string name)
        {
            bool exist = false;
            int i;
            for (i = 0; (i < List.Count) && (exist == false); i++)
                if (((StringArray)List[i]).Name == name)
                    exist = true;
            if (exist == false)
            {
                StringArray newVar = new StringArray(name);
                List.Add(newVar);
                return newVar;
            }
            else
                return (StringArray)List[i - 1];
        }

        internal void Clear()
        {
            List.Clear();
        }

        internal int IndexOf(string varName)
        {
            int res = -1;
            for (int i = 0; (i < List.Count) && (res < 0); i++)
                if (((StringArray)(List[i])).Name == varName)
                    res = i;
            return res;
        }

        internal void ClearItem(int index)
        {
            ((StringArray)List[index]).Array.Clear();
        }


    }

    public class ReportRecord
    {
        public int Type { get; }
        public string Sheet { get; }
        public int Column { get; }
        public int Row { get; }
        public string Value { get; }
        public string Defect { get; }
        public string Text { get; }

        public string TypeName
        {
            get { return ((EnErrorType) Type).ToText(); }
        }

        internal ReportRecord(int type, string sheet, int column, int row, string value, string defect, string text)
        {
            Type = type;
            Sheet = sheet;
            Column = column;
            Row = row;
            Value = value;
            Defect = defect;
            Text = text;
        }

    }

    public static class Helper
    {
        public static string ToText(this ReportRecord error)
        {
            return
                $"Тип: {((EnErrorType) error.Type).ToText()}; Лист: {error.Sheet}; Строка: {error.Row}; Столбец: {error.Column}; Значение: {error.Value}; № дефекта: {error.Defect}; Описание: {error.Text}";
        }
        public static string ToText(this SyntaxError syntaxError)
        {
            return
                $"Наименование: {syntaxError.Name}; Текст: {syntaxError.Text}; Значение: {syntaxError.Value}; Строка: {syntaxError.Line}";
        }
        
    }

    public class SyntaxError
    {
        public string Name { get; }
        public int? Line { get; }
        public string Text { get; }
        public string Value { get; }

        internal SyntaxError(string name, int? line, string text, string value)
        {
            Name = name;
            Line = line;
            Text = text;
            Value = value;
        }
    }

    internal class SyntaxErrorsList
    {
        internal ArrayList List { get; }

        internal SyntaxErrorsList()
        {
            List = new ArrayList();
        }

        internal void Add(string name, int? line, string text, string value)
        {
            SyntaxError newErr = new SyntaxError(name, line, text, value);
            List.Add(newErr);
        }
    }

}
