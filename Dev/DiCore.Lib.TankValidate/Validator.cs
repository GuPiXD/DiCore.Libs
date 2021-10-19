using System;
using System.Collections;
using System.Text.RegularExpressions;
using Aspose.Cells;

namespace DiCore.Lib.TankValidate
{
    //проверка файла Excel
    public partial class Validator : IValidator
    {
        private ArrayList Report = new ArrayList();
        private Worksheet CurrentSheet { get; set; }
        
        public ReportRecord[] Execute()
        {
            if (!Run())
            {
                throw new Exception("Ошибка входных данных");
            }
            return GetListErrors();
        }


        private bool Run()
        {
            if (ExistSyntaxErrors())
            {
                return false;
            }


            while (Tags.Read())
            {
                switch (Tags.ReadedNode.Name)
                {
                    case "MOV": OperationMOV(Tags.ReadedNode); break;
                    case "ADD":
                    case "SUB":
                    case "MUL":
                    case "DIV": MathOperations(Tags.ReadedNode); break;
                    case "CE":
                    case "CNE":
                    case "CL":
                    case "CLE":
                    case "CG":
                    case "CGE": CompareNumbers(Tags.ReadedNode); break;
                    case "CPE":
                    case "CPNE": ComparePointers(Tags.ReadedNode); break;
                    case "CMP": CompareStrings(Tags.ReadedNode, false); break;
                    case "CMPRE": CompareStrings(Tags.ReadedNode, true); break;
                    //E.R. 20.11.2017 Извлечение подстроки из строки в рамках нового кода дефекта
                    case "SUBSTRING": ExtractSubString(Tags.ReadedNode); break;
                    case "SUBNUMBER": ExtractSubNumber(Tags.ReadedNode); break;

                    case "NEXTR": ((Pointer)Pointers.List[Pointers.IndexOf(Tags.ReadedNode.Text)]).Cell.Row++; break;
                    case "NEXTC": ((Pointer)Pointers.List[Pointers.IndexOf(Tags.ReadedNode.Text)]).Cell.Column++; break;
                    case "NOT": Tags.ReadedNode.Result = !Tags.ReadedNode.Child.Result; break;
                    case "AND": OperationAND(Tags.ReadedNode); break;
                    case "OR": OperationOR(Tags.ReadedNode); break;
                    case "ISDATA": CheckIsData(Tags.ReadedNode); break;
                    case "EMPTY":
                        CheckIsData(Tags.ReadedNode);
                        Tags.ReadedNode.Result = !Tags.ReadedNode.Result; break;
                    case "CRITERION": CheckCriterion(Tags.ReadedNode); break;
                    case "THEN":
                    case "ELSE": Tags.SetCurrentNode(Tags.ReadedNode.Parent); break;
                    case "IF": break;
                    case "WHILE":
                        NodeClass tmpNode = Tags.ReadedNode.Child;
                        while (tmpNode.Name != "CRITERION")
                            tmpNode = tmpNode.Next;
                        if (tmpNode.Result == true)
                        {
                            Tags.RefreshData(Tags.ReadedNode);
                            Tags.SetCurrentNode(Tags.ReadedNode);
                        }
                        break;
                    case "LOAD": LoadArray(Tags.ReadedNode, false); break;
                    case "LOADU": LoadArray(Tags.ReadedNode, true); break;
                    case "INARRAY": SearchInArray(Tags.ReadedNode, false); break;
                    case "INARRAYRE": SearchInArray(Tags.ReadedNode, true); break;
                    case "OUTPUT": SetOutput(Tags.ReadedNode); break;
                    case "REQUIRED":
                        CheckIsData(Tags.ReadedNode);
                        if (Tags.ReadedNode.Result == false)
                        {
                            string defect = "";
                            if (((CellAddr)Tags.ReadedNode.Operand_1).Sheet == "Дефекты")
                            {
                                CellAddr tmpCell = new CellAddr();
                                tmpCell.CopyAddr(((CellAddr)Tags.ReadedNode.Operand_1));
                                tmpCell.Column = 5;
                                tmpCell.Offset = 0;
                                ReadString(tmpCell, out defect);
                            }
                            AddReportRecord(1, ((CellAddr)Tags.ReadedNode.Operand_1).Sheet,
                                ((CellAddr)Tags.ReadedNode.Operand_1).Column +
                                ((CellAddr)Tags.ReadedNode.Operand_1).Offset,
                                ((CellAddr)Tags.ReadedNode.Operand_1).Row, "", defect,
                                "Отсутствуют обязательные данные");
                        }
                        break;
                    case "DATEDIFF":
                        if (Tags.ReadedNode.Text.Substring(0, 1) == "#")
                        {
                            string tmpStr;
                            DateTime dt1, dt2;
                            Vars.Add(Tags.ReadedNode.Text);
                            CellAddr tmpAddr = new CellAddr();
                            tmpAddr.Sheet = "Обследования";
                            tmpAddr.Column = 4;
                            tmpAddr.Row = 5;
                            ReadString(tmpAddr, out tmpStr);
                            if (DateTime.TryParse(tmpStr, out dt1) == true)
                            {
                                tmpAddr.Sheet = "Общие";
                                tmpAddr.Column = 3;
                                tmpAddr.Row = 30;
                                ReadString(tmpAddr, out tmpStr);
                                if (DateTime.TryParse(tmpStr, out dt2) == true)
                                {
                                    TimeSpan diff = dt1 - dt2;
                                    DateTime res = new DateTime(diff.Ticks);
                                    double tmp = res.Year + (double)res.DayOfYear / 365;
                                    ((Variable)Vars.List[Vars.IndexOf(Tags.ReadedNode.Text)]).Value = tmp;
                                }
                                else
                                    AddReportRecord(0, tmpAddr.Sheet, tmpAddr.Column + tmpAddr.Offset, tmpAddr.Row,
                                        "Ошибка исполнения. Тег: <" + Tags.ReadedNode.Name + ">, Строка: " + Tags.ReadedNode.LineNumber.ToString(),
                                        "", "Значение в ячейке не является датой. Невозможна корректная проверка дефектовки дефектов геометрии.");
                            }
                            else
                                AddReportRecord(0, tmpAddr.Sheet, tmpAddr.Column + tmpAddr.Offset, tmpAddr.Row,
                                    "Ошибка исполнения. Тег: <" + Tags.ReadedNode.Name + ">, Строка: " + Tags.ReadedNode.LineNumber.ToString(),
                                    "", "Значение в ячейке не является датой. Невозможна корректная проверка дефектовки дефектов геометрии.");
                        }
                        break;
                    case "ARRAY":
                        InitArray(Tags.ReadedNode);
                        break;
                }
                //Application.DoEvents();
            }
            Tags.RefreshData(Tags.RootNode);
            Vars.Clear();
            Pointers.Clear();
            Strings.Clear();
            StrArrays.Clear();
            LastSheet = "";
            return true;
        }

        //E.R. 20.11.2017 Извлечение подстроки из строки в рамках нового кода дефекта. Параметры:
        // откуда взять (строковая переменная), сколько отступить от начала строки, сколько взять символов, 
        // куда положить (строковая переменная)
        private void ExtractSubString(NodeClass node)
        {

            DecodeOperands(node, node.Text);
            //Если в node Operand_3 и Operand_4 не пустые
            if ((node.Operand_3 != "") && (node.Operand_4) != "")
            {
                // В операнде 2 - отступ от начала строки.
                int Offs = Convert.ToInt16(node.Operand_3);
                // В операнде 3 - количество цифр
                int Amount = Convert.ToInt16(node.Operand_4);
                string Substr = ((StringVar)node.Operand_2).Value;
                if ((Substr == "") || (Substr == string.Empty) || ((Amount + Offs - 1) > Substr.Length))   // пустая строка - извлечь невозможно
                {
                    AddReportRecord(0, "", 0, 0,
                    "Ошибка исполнения. Тег: <" + node.Name + ">, Строка: " + node.LineNumber.ToString() + ", значение второго операнда: '" + Substr + "'"
                     //                    + node.Operand_1.ToString()
                     //                    +node.Operand_2.ToString()  
                     //                    + "(''), " + node.Operand_3 + ", " 
                     //                     +node.Operand_4
                     ,
                    "", "Не удалось выделить подстроку. Неверные параметры. Тег: <" + node.Name + ">, Строка: " + node.LineNumber.ToString() + ", значение второго операнда: '" + Substr + "'"
                    //                    + ((Variable) node.Operand_1).Name.ToString() 
                    //                    +((Variable)node.Operand_2).Name.ToString()  
                    //                    + "(''), " + node.Operand_3 + ", " 
                    //                        + node.Operand_4
                    );
                    return;
                }

                try
                {
                    Substr = Substr.Substring(Offs - 1, Amount);
                }
                catch
                {

                    string op1val;
                    if (((Variable)node.Operand_1).Value == null)
                    {
                        op1val = "nul";
                    }
                    else
                    {
                        op1val = ((Variable)node.Operand_1).Value.ToString();
                    }

                    string op2val;
                    if (((Variable)node.Operand_2).Value == null)
                    {
                        op2val = "nul";
                    }
                    else
                    {
                        op2val = ((Variable)node.Operand_2).Value.ToString();
                    }


                    string mess = ". Значение тега: " + ((Variable)node.Operand_1).Name.ToString() +
                        ", (" + op1val +
                        "), " + ((Variable)node.Operand_2).Name.ToString() + ",(" + op2val +
                        "), " + node.Operand_3 + ", " +
                                  node.Operand_4;

                    AddReportRecord(0, "", 0, 0,
                    "Ошибка исполнения. Тег: <" + node.Name + ">, Строка: " + node.LineNumber.ToString() + mess,
                    "", "Не удалось выделить подстроку. Неверные параметры. Тег: <" + node.Name + ">, Строка: " + node.LineNumber.ToString() +
                    mess);

                    Substr = "";
                }
                ((StringVar)node.Operand_1).Value = Substr;
            }
        }

        //E.R. 30.11.2017 Извлечение подчисла из числа в рамках нового кода дефекта. Параметры:
        // куда положить(переменная), откуда взять (переменная), сколько отступить от конца числа, сколько взять цифр, 
        private void ExtractSubNumber(NodeClass node)
        {

            DecodeOperands(node, node.Text);
            //Если в node Operand_3 и Operand_4 не пустые
            if ((node.Operand_3.ToString() != "") && (node.Operand_4.ToString() != ""))
            {
                // В операнде 3 - отступ от конца.
                int Offs = Convert.ToInt16(node.Operand_3);
                // В операнде 4 - количество цифр
                int Amount = Convert.ToInt16(node.Operand_4);
                string _numstr = ((Variable)node.Operand_2).Value.ToString();

                if ((Amount + Offs - 1) > _numstr.Length)
                {
                    if ((_numstr.Length - (Amount + Offs - 1)) != 1)
                    {
                        AddReportRecord(0, "", 0, 0,
                        "Ошибка исполнения. Тег: <" + node.Name + ">, Строка: " + node.LineNumber.ToString() + ", значение второго операнда: " + _numstr
                        //                    + ((Variable)node.Operand_1).Name.ToString()
                        //                    + ((Variable)node.Operand_2).Name.ToString()
                        + ", " + node.Operand_3 + ", "
                        + node.Operand_4
    ,
                        "", "Не удалось выделить подстроку. Неверные параметры. Тег: <" + node.Name + ">, Строка: " + node.LineNumber.ToString() + ", значение второго операнда: " + _numstr
                        //                    + ((Variable)node.Operand_1).Name.ToString()
                        //                    + ((Variable)node.Operand_2).Name.ToString()
                        + ", " + node.Operand_3 + ", "
                        + node.Operand_4
                        );
                        return;
                    }
                    else // Случай предваряющего нуля в числе
                    {
                        Amount--;
                    }
                }

                string Substr;
                try
                {
                    Substr = _numstr.Substring(_numstr.Length - Offs - Amount + 1, Amount);
                }
                catch
                {
                    string op1val;
                    if (((Variable)node.Operand_1).Value == null)
                    {
                        op1val = "nul";
                    }
                    else
                    {
                        op1val = ((Variable)node.Operand_1).Value.ToString();
                    }

                    string op2val;
                    if (((Variable)node.Operand_2).Value == null)
                    {
                        op2val = "nul";
                    }
                    else
                    {
                        op2val = ((Variable)node.Operand_2).Value.ToString();
                    }
                    string mess = ". Значение тега: " + ((Variable)node.Operand_1).Name.ToString() +
                        ", (" + op1val +
                        "), " + ((Variable)node.Operand_2).Name.ToString() + ",(" + op2val +
                        "), " + node.Operand_3 + ", " +
                                  node.Operand_4;

                    AddReportRecord(0, "", 0, 0,
                    "Ошибка исполнения. Тег: <" + node.Name + ">, Строка: " + node.LineNumber.ToString() +
                    mess,
                    "", "Не удалось выделить подстроку. Неверные параметры. Тег: <" + node.Name + ">, Строка: " + node.LineNumber.ToString() +
                    mess);
                    Substr = "";
                }
                try
                {
                    ((Variable)node.Operand_1).Value = Convert.ToInt32(Substr);
                }
                catch
                {
                    string op1val;
                    if (((Variable)node.Operand_1).Value == null)
                    {
                        op1val = "nul";
                    }
                    else
                    {
                        op1val = ((Variable)node.Operand_1).Value.ToString();
                    }

                    string op2val;
                    if (((Variable)node.Operand_2).Value == null)
                    {
                        op2val = "nul";
                    }
                    else
                    {
                        op2val = ((Variable)node.Operand_2).Value.ToString();
                    }


                    string mess = ". Значение тега: " + ((Variable)node.Operand_1).Name.ToString() +
                        ", (" + op1val +
                        "), " + ((Variable)node.Operand_2).Name.ToString() + ",(" + op2val +
                         "), " + node.Operand_3 + ", " +
                                  node.Operand_4;

                    AddReportRecord(0, "", 0, 0,
                    "Ошибка исполнения. Тег: <" + node.Name + ">, Строка: " + node.LineNumber.ToString() +
                    mess,
                    "", "Не удалось преобразовать подстроку в число. Неверные параметры. Тег: <" + node.Name + ">, Строка: " + node.LineNumber.ToString() +
                    mess);
                    ((Variable)node.Operand_1).Value = 0;
                }
            }
        }


        private void AddReportRecord(int type, string sheet, int column, int row, string value, string defect, string text)
        {
            ReportRecord newRecord = new ReportRecord(type, sheet, column, row, value, defect, text);
            
            Report.Add(newRecord);
        }

        #region Operation

        private void OperationMOV(NodeClass node)
        {
            string typeOp1, typeOp2, valueStr;
            double valueNum;
            DecodeOperands(node, node.Text);
            typeOp1 = node.Operand_1.GetType().Name;
            typeOp2 = node.Operand_2.GetType().Name;
            switch (typeOp1)
            {
                case "Variable":
                    switch (typeOp2)
                    {
                        case "Variable":
                            ((Variable)node.Operand_1).Value = ((Variable)node.Operand_2).Value;
                            break;
                        case "Double":
                            ((Variable)node.Operand_1).Value = ((Double)node.Operand_2);
                            break;
                        case "Pointer":
                            if (ReadNumber(((Pointer)node.Operand_2).Cell, out valueNum) == false)
                            {
                                CellAddr cell = new CellAddr();
                                cell.CopyAddr(((Pointer)node.Operand_2).Cell);
                                AddReportRecord(0, cell.Sheet, cell.Column + cell.Offset, cell.Row,
                                     "Ошибка исполнения. Тег: <" + node.Name + ">, Строка: " + node.LineNumber.ToString(),
                                     "", "Значение в ячейке не является числом");
                            }
                            ((Variable)node.Operand_1).Value = valueNum;
                            break;
                        case "CellAddr":
                            if (ReadNumber((CellAddr)node.Operand_2, out valueNum) == false)
                            {
                                CellAddr cell = new CellAddr();
                                cell.CopyAddr((CellAddr)node.Operand_2);
                                AddReportRecord(0, cell.Sheet, cell.Column + cell.Offset, cell.Row,
                                     "Ошибка исполнения. Тег: <" + node.Name + ">, Строка: " + node.LineNumber.ToString(),
                                     "", "Значение в ячейке не является числом");
                            }
                            ((Variable)node.Operand_1).Value = valueNum;
                            break;
                    }
                    break;
                case "Pointer":
                    switch (typeOp2)
                    {
                        case "Pointer":
                            ((Pointer)node.Operand_1).Cell.CopyAddr(((Pointer)node.Operand_2).Cell);
                            break;
                        case "CellAddr":
                            ((Pointer)node.Operand_1).Cell.CopyAddr((CellAddr)node.Operand_2);
                            break;
                    }
                    break;
                case "StringVar":
                    switch (typeOp2)
                    {
                        case "StringVar":
                            ((StringVar)node.Operand_1).Value = ((StringVar)node.Operand_2).Value;
                            break;
                        case "String":
                            ((StringVar)node.Operand_1).Value = (String)node.Operand_2;
                            break;
                        case "Pointer":
                            ReadString(((Pointer)node.Operand_2).Cell, out valueStr);
                            ((StringVar)node.Operand_1).Value = valueStr;
                            break;
                        case "CellAddr":
                            ReadString((CellAddr)node.Operand_2, out valueStr);
                            ((StringVar)node.Operand_1).Value = valueStr;
                            break;
                    }
                    break;
            }
        }

        private void MathOperations(NodeClass node)
        {
            CellAddr cell = new CellAddr();
            bool bReadResult = true;
            string typeOp1, typeOp2;
            double valueNum = 0;
            DecodeOperands(node, node.Text);
            typeOp1 = node.Operand_1.GetType().Name;
            typeOp2 = node.Operand_2.GetType().Name;
            switch (typeOp2)
            {
                case "Variable": valueNum = ((Variable)node.Operand_2).Value; break;
                case "Pointer":
                    if ((bReadResult = ReadNumber(((Pointer)node.Operand_2).Cell, out valueNum)) == false)
                        cell.CopyAddr(((Pointer)node.Operand_2).Cell);
                    break;
                case "CellAddr":
                    if ((bReadResult = ReadNumber((CellAddr)node.Operand_2, out valueNum)) == false)
                        cell.CopyAddr((CellAddr)node.Operand_2);
                    break;
                case "Double": valueNum = (Double)node.Operand_2; break;
            }
            if (bReadResult == true)
            {
                switch (node.Name)
                {
                    case "ADD": ((Variable)node.Operand_1).Value += valueNum; break;
                    case "SUB": ((Variable)node.Operand_1).Value -= valueNum; break;
                    case "MUL": ((Variable)node.Operand_1).Value *= valueNum; break;
                    case "DIV": ((Variable)node.Operand_1).Value /= valueNum; break;
                }
            }
            else
                AddReportRecord(0, cell.Sheet, cell.Column + cell.Offset, cell.Row,
                     "Ошибка исполнения. Тег: <" + node.Name + ">, Строка: " + node.LineNumber.ToString(),
                     "", "Значение в ячейке не является числом");
        }

        private void CompareNumbers(NodeClass node)
        {
            CellAddr cell = new CellAddr();
            string typeOp1, typeOp2;
            double value1 = 0, value2 = 0;
            bool bReadResult1 = true, bReadResult2 = true;
            DecodeOperands(node, node.Text);
            typeOp1 = node.Operand_1.GetType().Name;
            typeOp2 = node.Operand_2.GetType().Name;
            switch (typeOp1)
            {
                case "Variable": value1 = ((Variable)node.Operand_1).Value; break;
                case "Pointer":
                    if ((bReadResult1 = ReadNumber(((Pointer)node.Operand_1).Cell, out value1)) == false)
                        cell.CopyAddr(((Pointer)node.Operand_1).Cell);
                    break;
                case "CellAddr":
                    if ((bReadResult1 = ReadNumber((CellAddr)node.Operand_1, out value1)) == false)
                        cell.CopyAddr((CellAddr)node.Operand_1);
                    break;
            }
            if (bReadResult1 == false)
                AddReportRecord(0, cell.Sheet, cell.Column + cell.Offset, cell.Row,
                    "Ошибка исполнения. Тег: <" + node.Name + ">, Строка: " + node.LineNumber.ToString(),
                    "", "Значение в ячейке не является числом");
            switch (typeOp2)
            {
                case "Variable": value2 = ((Variable)node.Operand_2).Value; break;
                case "Pointer":
                    if ((bReadResult2 = ReadNumber(((Pointer)node.Operand_2).Cell, out value2)) == false)
                        cell.CopyAddr(((Pointer)node.Operand_2).Cell);
                    break;
                case "CellAddr":
                    if ((bReadResult2 = ReadNumber((CellAddr)node.Operand_2, out value2)) == false)
                        cell.CopyAddr((CellAddr)node.Operand_2);
                    break;
                case "Double": value2 = (Double)node.Operand_2; break;
            }
            if (bReadResult2 == false)
                AddReportRecord(0, cell.Sheet, cell.Column + cell.Offset, cell.Row,
                     "Ошибка исполнения. Тег: <" + node.Name + ">, Строка: " + node.LineNumber.ToString(),
                     "", "Значение в ячейке не является числом");
            node.Result = false;
            if ((bReadResult1 == true) && (bReadResult2 == true))
            {
                switch (node.Name)
                {
                    case "CE": if (value1 == value2) node.Result = true; break;
                    case "CNE": if (value1 != value2) node.Result = true; break;
                    case "CL": if (value1 < value2) node.Result = true; break;
                    case "CLE": if (value1 <= value2) node.Result = true; break;
                    case "CG": if (value1 > value2) node.Result = true; break;
                    case "CGE": if (value1 >= value2) node.Result = true; break;
                }
            }
        }

        private void ComparePointers(NodeClass node)
        {
            string typeOp2;
            CellAddr cell1, cell2 = null;
            DecodeOperands(node, node.Text);
            typeOp2 = node.Operand_2.GetType().Name;
            cell1 = ((Pointer)node.Operand_1).Cell;
            switch (typeOp2)
            {
                case "Pointer": cell2 = ((Pointer)node.Operand_2).Cell; break;
                case "CellAddr": cell2 = (CellAddr)node.Operand_2; break;
            }
            if ((cell1.Sheet == cell2.Sheet) && (cell1.Column == cell2.Column) &&
                (cell1.Row == cell2.Row) && (cell1.Offset == cell2.Offset))
                node.Result = true;
            else
                node.Result = false;
        }

        private void CompareStrings(NodeClass node, bool bUseRegEx)
        {
            string typeOp1, typeOp2;
            string str1 = "", str2 = "";
            DecodeOperands(node, node.Text);
            typeOp1 = node.Operand_1.GetType().Name;
            typeOp2 = node.Operand_2.GetType().Name;
            switch (typeOp1)
            {
                case "StringVar": str1 = ((StringVar)node.Operand_1).Value; break;
                case "Pointer": ReadString(((Pointer)node.Operand_1).Cell, out str1); break;
                case "CellAddr": ReadString((CellAddr)node.Operand_1, out str1); break;
            }
            str1 = NormalizeString(str1);
            switch (typeOp2)
            {
                case "StringVar": str2 = ((StringVar)node.Operand_2).Value; break;
                case "Pointer": ReadString(((Pointer)node.Operand_2).Cell, out str2); break;
                case "CellAddr": ReadString((CellAddr)node.Operand_2, out str2); break;
                case "String": str2 = (String)node.Operand_2; break;
            }
            str2 = NormalizeString(str2);
            if (bUseRegEx == true)
            {
                if (Regex.IsMatch(str1, str2))
                    node.Result = true;
                else
                    node.Result = false;
            }
            else
            {
                if (str1 == str2)
                    node.Result = true;
                else
                    node.Result = false;
            }
        }

        private void OperationAND(NodeClass node)
        {
            NodeClass tmpNode = node.Child;
            node.Result = tmpNode.Result;
            while (tmpNode != null)
            {
                tmpNode = tmpNode.Next;
                if (tmpNode != null)
                    if (tmpNode.Result == false)
                        node.Result = false;
            }
        }

        private void OperationOR(NodeClass node)
        {
            NodeClass tmpNode = node.Child;
            node.Result = tmpNode.Result;
            while (tmpNode != null)
            {
                tmpNode = tmpNode.Next;
                if (tmpNode != null)
                    if (tmpNode.Result == true)
                        node.Result = true;
            }
        }

        private void CheckIsData(NodeClass node)
        {
            string offsetStr = "", str = node.Text;
            int index;
            CellAddr cell = null;
            switch (node.Text.Substring(0, 1))
            {
                case "*":
                    if ((index = str.IndexOf('+')) != -1)
                    {
                        offsetStr = str.Substring(index, str.Length - index);
                        str = str.Substring(0, index);
                    }
                    else
                        if ((index = str.IndexOf('-')) != -1)
                    {
                        offsetStr = str.Substring(index, str.Length - index);
                        str = str.Substring(0, index);
                    }
                    cell = ((Pointer)Pointers.List[Pointers.IndexOf(str)]).Cell;
                    if (offsetStr != "")
                        cell.Offset = Convert.ToInt32(offsetStr);
                    else
                        cell.Offset = 0;
                    break;
                case "[":
                    cell = new CellAddr();
                    cell.DecodeAddr(str);
                    break;
            }
            node.Operand_1 = cell;
            node.Result = ExistData(cell);
        }

        private void CheckCriterion(NodeClass node)
        {
            NodeClass tmpNode;
            node.Result = node.Child.Result;
            if (node.Parent.Name == "IF")
            {
                if (node.Result == true)
                {
                    tmpNode = node;
                    while (tmpNode.Name != "THEN")
                        tmpNode = tmpNode.Next;
                    Tags.SetCurrentNode(tmpNode);
                }
                else
                {
                    tmpNode = node;
                    while ((tmpNode.Next != null) && (tmpNode.Name != "ELSE"))
                        tmpNode = tmpNode.Next;
                    if (tmpNode.Name == "ELSE")
                        Tags.SetCurrentNode(tmpNode);
                    else
                        Tags.SetCurrentNode(node.Parent);
                }
            }
            else
                if (node.Result == false)
                Tags.SetCurrentNode(node.Parent);
        }

        private void LoadArray(NodeClass node, bool bUnique)
        {
            string value;
            int index;
            bool bFinish = false;
            CellAddr cell = new CellAddr();
            string text = node.Text;
            int commaPos = text.IndexOf(',');
            string array = text.Substring(0, commaPos).Trim();
            text = text.Substring(commaPos + 1, text.Length - commaPos - 1);
            DecodeOperands(node, text);
            switch (node.Operand_1.GetType().Name)
            {
                case "Pointer": cell.CopyAddr(((Pointer)node.Operand_1).Cell); break;
                case "CellAddr": cell.CopyAddr((CellAddr)node.Operand_1); break;
            }


            if ((index = StrArrays.IndexOf(array)) == -1)
            {
                StrArrays.Add(array);
                index = StrArrays.IndexOf(array);
            }
            else
                StrArrays.ClearItem(index);

            while (bFinish != true)
            {
                if (ReadString(cell, out value) == true)
                {
                    // Лечим ошибку в справочнике типов дефектов "сврных швов"
                    value = value.Replace("сврн", "сварн");
                    value = NormalizeString(value);
                    if (bUnique)
                    {
                        if (!((StringArray)StrArrays.List[index]).Array.Contains(value))
                            ((StringArray)StrArrays.List[index]).Array.Add(value);
                    }
                    else
                        ((StringArray)StrArrays.List[index]).Array.Add(value);
                }
                cell.Row++;
                if (node.Operand_2 == null)
                {
                    if (ExistData(cell) == false)
                        bFinish = true;
                }
                else
                    if ((((CellAddr)node.Operand_2).Sheet == cell.Sheet) &&
                        (((CellAddr)node.Operand_2).Column == cell.Column) &&
                        (((CellAddr)node.Operand_2).Row < cell.Row))
                    bFinish = true;
            }
        }

        private void SearchInArray(NodeClass node, bool bUseRegex)
        {
            string valStr = "";
            int index = 0;
            string text = node.Text;
            int commaPos = text.IndexOf(',');
            string array = text.Substring(0, commaPos).Trim();
            text = text.Substring(commaPos + 1, text.Length - commaPos - 1);
            DecodeOperands(node, text);
            switch (node.Operand_1.GetType().Name)
            {
                case "StringVar": valStr = ((StringVar)node.Operand_1).Value; break;
                case "Pointer": ReadString(((Pointer)node.Operand_1).Cell, out valStr); break;
                case "CellAddr": ReadString((CellAddr)node.Operand_1, out valStr); break;
                case "String": valStr = (String)node.Operand_1; break;
            }
            valStr = NormalizeString(valStr);
            if ((index = StrArrays.IndexOf(array)) != -1)
            {
                node.Result = false;
                for (int i = 0; i < ((StringArray)StrArrays.List[index]).Array.Count; i++)
                    if (bUseRegex == true)
                    {
                        if (Regex.IsMatch((String)((StringArray)StrArrays.List[index]).Array[i], valStr) == true)
                            node.Result = true;
                    }
                    else
                    {
                        if ((String)((StringArray)StrArrays.List[index]).Array[i] == valStr)
                            node.Result = true;
                    }
            }
        }

        private void SetOutput(NodeClass node)
        {
            CellAddr cell = new CellAddr();
            string text = node.Text;
            int commaPos = text.IndexOf(',');
            string typeStr = text.Substring(0, commaPos);
            string value;
            int type = 0;
            text = text.Substring(commaPos + 1, text.Length - commaPos - 1);
            DecodeOperands(node, text);
            switch (node.Operand_1.GetType().Name)
            {
                case "Pointer": cell.CopyAddr(((Pointer)node.Operand_1).Cell); break;
                case "CellAddr": cell.CopyAddr((CellAddr)node.Operand_1); break;
            }
            if (typeStr.ToUpper() == "ERROR")
                type = 1;
            else
                if (typeStr.ToUpper() == "WARNING")
                type = 2;
            ReadString(cell, out value);
            string defect = "";
            if (cell.Sheet == "Дефекты")
            {
                CellAddr tmpCell = new CellAddr();
                tmpCell.CopyAddr(cell);
                tmpCell.Column = 6;
                tmpCell.Offset = 0;
                ReadString(tmpCell, out defect);
            }
            AddReportRecord(type, cell.Sheet, cell.Column + cell.Offset, cell.Row, value, defect, (String)node.Operand_2);
        }

        private void InitArray(NodeClass node)
        {
            string text = node.Text;
            int commaPos = text.IndexOf(',');
            string array = text.Substring(0, commaPos).Trim();
            StrArrays.Add(array);
            text = text.Substring(commaPos + 1, text.Length - commaPos - 1);
            int index = StrArrays.IndexOf(array);
            commaPos = text.IndexOf('{');
            text = text.Substring(commaPos + 1, text.Length - commaPos - 1);
            string value;
            while ((commaPos = text.IndexOf(',')) != -1)
            {
                value = text.Substring(0, commaPos);
                text = text.Substring(commaPos + 1, text.Length - commaPos - 1);
                ((StringArray)StrArrays.List[index]).Array.Add(NormalizeString(value));
            }
            commaPos = text.IndexOf('}');
            value = text.Substring(0, commaPos);
            ((StringArray)StrArrays.List[index]).Array.Add(NormalizeString(value));
        }


        #endregion Operation

        private bool ReadString(CellAddr cell, out string result)
        {
            try
            {
                result = "";
                if ((cell.Sheet == "") || (cell.Row == 0) ||
                    (cell.Column == 0))
                {
                    AddReportRecord(0, cell.Sheet, cell.Column + cell.Offset, cell.Row,
                         "Ошибка исполнения", "", "Неправильный адрес в XML-шаблоне");
                    return false;
                }
                else
                {
                    if (cell.Sheet != LastSheet)
                    {
                        CurrentSheet = Workbook.Worksheets[cell.Sheet];
                        LastSheet = cell.Sheet;
                    }
                    var range = CurrentSheet.Cells[cell.Row - 1,cell.Column + cell.Offset - 1];
                    result = range != null ? range.StringValue : "";
                    if (result == "")
                    {
                        //AddReportRecord(0, cell.Sheet, cell.Column + cell.Offset, cell.Row, "Ошибка исполнения", "Отстутствуют данные типа \"Строка\"");
                        return false;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                //  MessageBox.Show(ex.Message);
                result = "";
                return false;
            }
        }

        private bool ReadNumber(CellAddr cell, out Double result)
        {

            try
            {

                if ((cell.Sheet == "") || (cell.Row == 0) ||
                    (cell.Column == 0))
                {
                    result = 0; // ошибка адреса
                    //AddReportRecord(0, cell.Sheet, cell.Column + cell.Offset, cell.Row, "Ошибка исполнения", "Неправильный адрес в XML-шаблоне");
                    return false;
                }
                else
                {
                    if (cell.Sheet != LastSheet)
                    {
                        CurrentSheet = Workbook.Worksheets[cell.Sheet];
                        LastSheet = cell.Sheet;
                    }
                    var range = CurrentSheet.Cells[cell.Row - 1, cell.Column + cell.Offset - 1];
                    
                    string value = range != null ? NormalizeString(range.StringValue) : "";
                    //value = value.Replace(',', '.');
                    
                    value = value.Replace(".", DecimalSeparator).Replace(",", DecimalSeparator);
                    if (value == "")
                    {
                        result = 0;
                        //AddReportRecord(0, cell.Sheet, cell.Column + cell.Offset, cell.Row, "Ошибка исполнения", "Отстутствуют данные типа \"Число\"");
                        return false;
                    }
                    else
                    if (Double.TryParse(value, out result) == false)
                    {
                        result = 0; // ошибка данных
                        //AddReportRecord(0, cell.Sheet, cell.Column + cell.Offset, cell.Row, "Ошибка исполнения", "Значение в ячейке не является числом");
                        return false;
                    }
                    else
                        return true;
                }
            }
            catch
            {
                result = 0;
                return false;
            }
        }

        private bool ExistData(CellAddr cell)
        {
            try
            {
                if ((cell.Sheet == "") || (cell.Row == 0) || (cell.Column == 0))
                    return false; // ошибка адреса
                else
                {
                    //if (cell.Sheet != LastSheet)
                    {
                        CurrentSheet = Workbook.Worksheets[cell.Sheet];
                        LastSheet = cell.Sheet;
                    }
                    var range = CurrentSheet.Cells[cell.Row - 1, cell.Column + cell.Offset - 1];
                    if (range.StringValue != "")
                        return true;
                    else
                        return false; // ошибка данных
                }
            }
            catch
            {
                return false;
            }
        }

        private void DecodeOperands(NodeClass node, string text)
        {
            //E.R. 20.11.2017 Извлечение подстроки из строки в рамках нового кода дефекта
            // В функции извлечения 4 операнда, поэтому дополнительно еще два.
            string[] strvar = text.Split(',');
            int Offset;
            int Amount;

            double number;
            int index;
            string strOp1, strOp2 = "";

            string strOp3 = "", strOp4 = "";

            string offsetStr = "";
            int commaPos = text.IndexOf(',');
            if (commaPos > 0)
            {
                strOp1 = text.Substring(0, commaPos);
                strOp2 = text.Substring(commaPos + 1, text.Length - commaPos - 1);
            }
            else
                strOp1 = text;


            strOp1 = strOp1.Trim();
            strOp2 = strOp2.Trim();

            if (strvar.Length > 2) // Извлечение подстроки
            {
                strOp2 = strvar[1];
                strOp2 = strOp2.Trim();

                strOp3 = strvar[2];
                strOp4 = strvar[3];
            }

            strOp3 = strOp3.Trim();
            strOp4 = strOp4.Trim();
            switch (strOp1.Substring(0, 1))
            {
                case "#":
                    node.Operand_1 = Vars.Add(strOp1);
                    break;
                case "*":
                    if (strOp1.IndexOf('+') != -1)
                    {
                        strOp1 = strOp1.Replace(" ", "");
                        index = strOp1.IndexOf('+');
                        offsetStr = strOp1.Substring(index, strOp1.Length - index);
                        strOp1 = strOp1.Substring(0, index);
                    }
                    else
                        if (strOp1.IndexOf('-') != -1)
                    {
                        strOp1 = strOp1.Replace(" ", "");
                        index = strOp1.IndexOf('-');
                        offsetStr = strOp1.Substring(index, strOp1.Length - index);
                        strOp1 = strOp1.Substring(0, index);
                    }
                    node.Operand_1 = Pointers.Add(strOp1);
                    if (offsetStr != "")
                        ((Pointer)node.Operand_1).Cell.Offset = Convert.ToInt32(offsetStr);
                    else
                        ((Pointer)node.Operand_1).Cell.Offset = 0;
                    break;
                case "[":
                    node.Operand_1 = new CellAddr();
                    ((CellAddr)node.Operand_1).DecodeAddr(strOp1);
                    break;
                case "$":
                    node.Operand_1 = Strings.Add(strOp1);
                    break;
            }
            offsetStr = "";
            if (strOp2 != "")
            {
                switch (strOp2.Substring(0, 1))
                {
                    case "#":
                        node.Operand_2 = ((Variable)Vars.List[Vars.IndexOf(strOp2)]);
                        break;
                    case "*":
                        if (strOp2.IndexOf('+') != -1)
                        {
                            strOp2 = strOp2.Replace(" ", "");
                            index = strOp2.IndexOf('+');
                            offsetStr = strOp2.Substring(index, strOp2.Length - index);
                            strOp2 = strOp2.Substring(0, index);
                        }
                        else
                            if (strOp2.IndexOf('-') != -1)
                        {
                            strOp2 = strOp2.Replace(" ", "");
                            index = strOp2.IndexOf('-');
                            offsetStr = strOp2.Substring(index, strOp2.Length - index);
                            strOp2 = strOp2.Substring(0, index);
                        }
                        node.Operand_2 = ((Pointer)Pointers.List[Pointers.IndexOf(strOp2)]);
                        if (offsetStr != "")
                            ((Pointer)node.Operand_2).Cell.Offset = Convert.ToInt32(offsetStr);
                        else
                            ((Pointer)node.Operand_2).Cell.Offset = 0;
                        break;
                    case "$":
                        node.Operand_2 = ((StringVar)Strings.List[Strings.IndexOf(strOp2)]);
                        break;
                    case "[":
                        node.Operand_2 = new CellAddr();
                        ((CellAddr)node.Operand_2).DecodeAddr(strOp2);
                        break;
                    default:
                        //  strOp2 = strOp2.Replace(",", ".");//SEF++
                        if (Double.TryParse(strOp2.Replace(".", DecimalSeparator).Replace(",", DecimalSeparator), out number) == true)
                            node.Operand_2 = number;
                        else
                            node.Operand_2 = strOp2;
                        break;
                }
            }
            else
                node.Operand_2 = null;

            if (strOp3 != "")   // Есть 3 операнд - извлечение подстроки. Отступ от начала строки
            {
                node.Operand_3 = strOp3;
            }

            if (strOp4 != "")   // Есть 4 операнд - извлечение подстроки. Отступ от начала строки
            {
                node.Operand_4 = strOp4;
            }
        }

        private string NormalizeString(string str)
        {
            //            str = str.ToUpper();
            // убираем лишние символы
            str = str.Replace(" ", "");
            str = str.Replace("\n", "");
            str = str.Replace("\r", "");
            str = str.Replace("\t", "");
            // Преобразуем кириллицу в латиницу
            str = str.Replace('А', 'A');
            str = str.Replace('В', 'B');
            str = str.Replace('С', 'C');

            return str;
        }
    }
}