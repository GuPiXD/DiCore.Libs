using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml;
using Aspose.Cells;

namespace DiCore.Lib.TankValidate
{
    //Получение файлов. Парсинг и проверка XML файла
    public partial class Validator
    {
        private TagsTree Tags = new TagsTree();
        private VariablesList Vars = new VariablesList();
        private PointersList Pointers = new PointersList();
        private StringsList Strings = new StringsList();
        private StringArrayList StrArrays = new StringArrayList();
        private string LastSheet;

        private Workbook Workbook { get; set; }
        private SyntaxErrorsList SyntaxErrors = new SyntaxErrorsList();

        private readonly string DecimalSeparator =
            System.Globalization.NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator;


        public SyntaxError[] LoadFile(MemoryStream fileXml, MemoryStream fileExcel)
        {
            ClearProreties();

            ReadXML(fileXml);
            Workbook = ReadExcel(fileExcel);
            return GetSyntaxErrors();
        }

        private void ClearProreties()
        {
            Tags = new TagsTree();
            Vars = new VariablesList();
            Pointers = new PointersList();
            Strings = new StringsList();
            StrArrays = new StringArrayList();
            LastSheet = string.Empty;
            Workbook = null;
            SyntaxErrors = new SyntaxErrorsList();
            Report = new ArrayList();
            CurrentSheet = null;
        }


        private bool ReadXML(MemoryStream file)
        {
            bool bNotSyntaxError = true;
            XmlTextReader xmlReader = null;
            try
            {
                xmlReader = new XmlTextReader(file);
                xmlReader.WhitespaceHandling = WhitespaceHandling.None;
                Tags.Clear();
                while (xmlReader.Read())
                    switch (xmlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            Tags.Add(xmlReader.Name, xmlReader.LineNumber);
                            //if (xmlReader.HasAttributes)
                            //{
                            //    xmlReader.MoveToAttribute(0);
                            //    Tags.SetAttribute(xmlReader.Value);
                            //}
                            break;
                        case XmlNodeType.EndElement:
                            Tags.EndElement(xmlReader.Name);
                            break;
                        case XmlNodeType.Text:
                            Tags.SetText(xmlReader.Value);
                            break;
                    }
                Tags.RefreshData(Tags.RootNode);
            }
            catch (Exception exc)
            {
                SyntaxErrors.Add("Ошибка загрузки", 0, "Ошибка загрузки xml шаблона для проверки", exc.Message);
                bNotSyntaxError = false;
            }
            finally
            {
                if (xmlReader != null)
                    xmlReader.Close();
            }

            CheckSyntax();

            return bNotSyntaxError;
        }

        private Workbook ReadExcel(MemoryStream fileExcel)
        {
            try
            {
                var workbook = new Workbook(fileExcel);
                if (workbook.Worksheets.Count != 57)
                    SyntaxErrors.Add("Ошибка загрузки", null, "Количество листов во входной форме должно быть 57", $"Количество листов в файле {workbook.Worksheets.Count}");

                return workbook;
            }
            catch (Exception ex)
            {
                SyntaxErrors.Add("Ошибка загрузки", 0, "Ошибка загрузки проверяемого excel файла", ex.Message);
            }

            return null;
        }

        private bool CheckSyntax()
        {
            bool bSyntError = false;
            ArrayList RequiredTags = new ArrayList();
            ArrayList ExtraTags = new ArrayList();
            ArrayList Operand1 = new ArrayList();
            ArrayList Operand2 = new ArrayList();
            //ArrayList PrefOperand = new ArrayList();

            while (Tags.Read())
            {
                RequiredTags.Clear();
                ExtraTags.Clear();
                Operand1.Clear();
                Operand2.Clear();
                //PrefOperand.Clear();
                
                switch (Tags.ReadedNode.Name)
                {
                    case "MOV":
                        FillArrayList(Operand1, new string[] { "Pointer", "StringVar", "Variable" });
                        FillArrayList(Operand2, new string[] { "Pointer", "CellAddr", "Variable", "Double", "StringVar", "String" });
                        CheckTypesOperands(Tags.ReadedNode, Operand1, Operand2);
                        break;

                    case "SUBSTRING":
                        FillArrayList(Operand1, new string[] { "Pointer", "StringVar", "Variable" });
                        FillArrayList(Operand2, new string[] { "Pointer", "CellAddr", "Variable", "Double", "StringVar", "String" });
                        CheckTypesOperands(Tags.ReadedNode, Operand1, Operand2);
                        break;

                    case "SUBNUMBER":
                        FillArrayList(Operand1, new string[] { "Pointer", "StringVar", "Variable" });
                        FillArrayList(Operand2, new string[] { "Pointer", "CellAddr", "Variable", "Double", "StringVar", "String" });
                        CheckTypesOperands(Tags.ReadedNode, Operand1, Operand2);
                        break;

                    case "ADD":
                    case "SUB":
                    case "MUL":
                    case "DIV":
                        Operand1.Add("Variable");
                        FillArrayList(Operand2, new string[] { "Pointer", "CellAddr", "Variable", "Double" });
                        CheckTypesOperands(Tags.ReadedNode, Operand1, Operand2);
                        break;
                    case "CE":
                    case "CNE":
                    case "CL":
                    case "CLE":
                    case "CG":
                    case "CGE":
                        FillArrayList(Operand1, new string[] { "Pointer", "CellAddr", "Variable" });
                        FillArrayList(Operand2, new string[] { "Pointer", "CellAddr", "Variable", "Double" });
                        CheckTypesOperands(Tags.ReadedNode, Operand1, Operand2);
                        break;
                    case "CPE":
                    case "CPNE":
                        Operand1.Add("Pointer");
                        FillArrayList(Operand2, new string[] { "Pointer", "CellAddr" });
                        CheckTypesOperands(Tags.ReadedNode, Operand1, Operand2);
                        break;
                    case "CMP":
                    case "CMPRE":
                        FillArrayList(Operand1, new string[] { "Pointer", "CellAddr", "StringVar" });
                        FillArrayList(Operand2, new string[] { "Pointer", "CellAddr", "StringVar", "String" });
                        CheckTypesOperands(Tags.ReadedNode, Operand1, Operand2);
                        break;
                    case "ISDATA":
                    case "EMPTY":
                    case "REQUIRED":
                        FillArrayList(Operand1, new string[] { "Pointer", "CellAddr" });
                        CheckTypesOperands(Tags.ReadedNode, Operand1, Operand2);
                        break;
                    case "NEXTR":
                    case "NEXTC":
                        Operand1.Add("Pointer");
                        CheckTypesOperands(Tags.ReadedNode, Operand1, Operand2);
                        break;
                    case "LOAD":
                    case "LOADU":
                        FillArrayList(Operand1, new string[] { "Pointer", "CellAddr" });
                        Operand2.Add("CellAddr");
                        CheckTypesOperands(Tags.ReadedNode, Operand1, Operand2);
                        break;
                    case "INARRAY":
                    case "INARRAYRE":
                        FillArrayList(Operand1, new string[] { "Pointer", "CellAddr", "StringVar", "String" });
                        CheckTypesOperands(Tags.ReadedNode, Operand1, Operand2);
                        break;
                    case "OUTPUT":
                        FillArrayList(Operand1, new string[] { "Pointer", "CellAddr" });
                        Operand2.Add("String");
                        CheckTypesOperands(Tags.ReadedNode, Operand1, Operand2);
                        break;
                    case "NOT":
                    case "AND":
                    case "OR":
                    case "CRITERION":
                        FillArrayList(ExtraTags, new string[] { "CE", "CNE", "CL", "CLE", "CG", "CGE", "CMP", "CMPRE", "CPE", "CPNE",
                            "ISDATA", "EMPTY", "INARRAY", "INARRAYRE", "NOT", "AND", "OR"});
                        break;
                    case "UNIT":
                    case "THEN":
                    case "ELSE":
                        FillArrayList(ExtraTags, new string[] { "CE", "CNE", "CL", "CLE", "CG", "CGE", "CMP", "CMPRE", "CPE", "CPNE",
                            "ISDATA", "EMPTY", "INARRAY", "INARRAYRE", "NOT", "AND", "OR"});
                        FillArrayList(ExtraTags, new string[] { "MOV", "ADD", "SUB", "MUL", "DIV", "NEXTR", "NEXTC", "LOAD", "LOADU",
                            "OUTPUT", "WHILE", "IF", "UNIT", "REQUIRED", "DATEDIFF", "ARRAY", "SUBSTRING", "SUBNUMBER"});
                        break;
                    case "WHILE":
                        FillArrayList(ExtraTags, new string[] { "CE", "CNE", "CL", "CLE", "CG", "CGE", "CMP", "CMPRE", "CPE", "CPNE",
                            "ISDATA", "EMPTY", "INARRAY", "INARRAYRE", "NOT", "AND", "OR"});
                        FillArrayList(ExtraTags, new string[] { "MOV", "ADD", "SUB", "MUL", "DIV", "NEXTR", "NEXTC", "LOAD", "LOADU",
                            "OUTPUT", "WHILE", "IF", "UNIT", "REQUIRED", "DATEDIFF", "ARRAY","SUBSTRING", "SUBNUMBER"});
                        RequiredTags.Add("CRITERION");
                break;
                    case "IF":
                        RequiredTags.Add("CRITERION");
                        RequiredTags.Add("THEN");
                        ExtraTags.Add("ELSE");
                        break;
                    case "DATEDIFF":
                        break;
                    case "ARRAY":
                        break;
                    default:
                        bSyntError = true;
                        SyntaxErrors.Add(Tags.ReadedNode.Name, Tags.ReadedNode.LineNumber, "Неразрешенный тег: " + Tags.ReadedNode.Name,
                            Tags.ReadedNode.Text);
                        break;
                }
                CheckTagSyntax(Tags.ReadedNode, RequiredTags, ExtraTags);
            }
            Tags.RefreshData(Tags.RootNode);
            return bSyntError;
        }



        private void CheckTypesOperands(NodeClass node, ArrayList listOp1, ArrayList listOp2)
        {
            double number;
            string strOp1, strOp2 = "";
            string typeOp1 = null, typeOp2 = null;
            int commaPos;
            string text = node.Text;
            ArrayList prefTags = new ArrayList();
            FillArrayList(prefTags, new string[] { "OUTPUT", "LOAD", "LOADU", "INARRAY", "INARRAYRE" });
            if (prefTags.Contains(node.Name) == true)
            {
                commaPos = text.IndexOf(',');
                string prefStr = text.Substring(0, commaPos);
                text = text.Substring(commaPos + 1, text.Length - commaPos - 1);
            }
            commaPos = text.IndexOf(',');
            if (commaPos > 0)
            {
                strOp1 = text.Substring(0, commaPos);
                strOp2 = text.Substring(commaPos + 1, text.Length - commaPos - 1);
            }
            else
                strOp1 = text;
            strOp1 = strOp1.Trim();
            strOp2 = strOp2.Trim();
            if (strOp1 != "")
            {
                switch (strOp1.Substring(0, 1))
                {
                    case "#": typeOp1 = "Variable"; break;
                    case "*": typeOp1 = "Pointer"; break;
                    case "[": typeOp1 = "CellAddr"; break;
                    case "$": typeOp1 = "StringVar"; break;
                }
            }
            if (typeOp1 == null)
                SyntaxErrors.Add(node.Name, node.LineNumber, "Отсутствует или неправильно определен обязательный 1-й операнд",
                    node.Text);
            else
            {
                if (listOp1.Contains(typeOp1) == false)
                    SyntaxErrors.Add(node.Name, node.LineNumber, "Недопустимый тип данных для 1-го операнда",
                        strOp1);
            }
            if (strOp2 != "")
            {
                switch (strOp2.Substring(0, 1))
                {
                    case "#": typeOp2 = "Variable"; break;
                    case "*": typeOp2 = "Pointer"; break;
                    case "$": typeOp2 = "StringVar"; break;
                    case "[": typeOp2 = "CellAddr"; break;
                    default:
                        strOp2 = strOp2.Replace(".", DecimalSeparator).Replace(",", DecimalSeparator);
                        if (Double.TryParse(strOp2, out number) == true)
                            typeOp2 = "Double";
                        else
                            typeOp2 = "String";
                        break;
                }
            }
            if (typeOp2 == null)
            {
                ArrayList list = new ArrayList();
                FillArrayList(list, new string[] { "ISDATA", "EMPTY", "REQUIRED", "NEXTR", "NEXTC", "LOAD", "LOADU",
                    "INARRAY", "INARRAYRE" });
                if (list.Contains(node.Name) == false)
                    SyntaxErrors.Add(node.Name, node.LineNumber, "Отсутствует или неправильно определен обязательный 2-й операнд",
                        node.Text);
            }
            else
            {
                if (listOp2.Contains(typeOp2) == false)
                    SyntaxErrors.Add(node.Name, node.LineNumber, "Недопустимый тип данных для 2-го операнда",
                        strOp2);
            }
        }

        private void CheckTagSyntax(NodeClass node, ArrayList required, ArrayList extra)
        {
            NodeClass tmpNode;
            if ((required.Count == 0) && (extra.Count == 0))
            {
                if (node.Child != null)
                    SyntaxErrors.Add(node.Name, node.LineNumber, "Данный XML-тег не может содержать вложенных тегов",
                        node.Child.Name);
            }
            else
            {
                if (required.Count > 0)
                {
                    for (int i = 0; i < required.Count; i++)
                    {
                        tmpNode = node.Child;
                        bool bExist = false;
                        while (tmpNode != null)
                        {
                            if (tmpNode.Name == (String)required[i])
                                bExist = true;
                            tmpNode = tmpNode.Next;
                        }
                        if (bExist == false)
                            SyntaxErrors.Add(node.Name, node.LineNumber, "Отсутствует обязательный XML-тег", (String)required[i]);
                    }
                }
                if (extra.Count > 0)
                {
                    if (node.Child != null)
                    {
                        tmpNode = node.Child;
                        while (tmpNode != null)
                        {
                            if ((required.Contains(tmpNode.Name) == false) && (extra.Contains(tmpNode.Name) == false))
                                SyntaxErrors.Add(node.Name, node.LineNumber, "Неразрешенный вложенный XML-тег", tmpNode.Name);
                            tmpNode = tmpNode.Next;
                        }
                    }
                }
            }

            // Проверка наличия обязательных тегов
            tmpNode = node.Child;
        }


        private void FillArrayList(ArrayList list, string[] values)
        {
            for (int i = 0; i < values.Length; i++)
                list.Add(values[i]);
        }
        private SyntaxError[] GetSyntaxErrors()
        {
            if (Tags.RootNode == null)
            {
                return new SyntaxError[] { new SyntaxError("Ошибка загрузки", 0, "Не найден xml шаблон для проверки", null) };
            }
            else if (Workbook == null)
            {
                return new SyntaxError[] { new SyntaxError("Ошибка загрузки", 0, "Не найден проверяемый excel файл", null) };
            }
            else if (SyntaxErrors.List.Count != 0)
            {
                return SyntaxErrors.List.Cast<SyntaxError>().ToArray();
            }
            return new SyntaxError[0];
        }

        private bool ExistSyntaxErrors()
        {
            return SyntaxErrors.List.Count != 0 || Tags.RootNode == null || Workbook == null;
        }

        private ReportRecord[] GetListErrors()
        {
            return Report.Count != 0 ? Report.Cast<ReportRecord>().ToArray() : null;
        }

        
        public void ExcelErrorsSave(ReportRecord[] errors, string path)
        {
            HelperErrors.ExcelErrorsSave(errors, path);
        }
        
        public MemoryStream GetExcelErrorsMemoryStream(ReportRecord[] errors)
        {
            return HelperErrors.GetExcelErrorsMemoryStream(errors);
        }
        
        public void InputExcelAppendErrorsSave(ReportRecord[] errors, string path)
        {
            HelperErrors.InputExcelAppendErrorsSave(errors, path, Workbook);
        }
        
        public MemoryStream GetInputExcelAppendErrorsMemoryStream(ReportRecord[] errors)
        {
            return HelperErrors.GetInputExcelAppendErrorsMemoryStream(errors, Workbook);
        }
        
        public static MemoryStream GetInputExcelAppendErrorsMemoryStream(ReportRecord[] errors, MemoryStream fileExcel)
        {
            var workbook = new Workbook(fileExcel);
            return HelperErrors.GetInputExcelAppendErrorsMemoryStream(errors, workbook);
        }
    }
}