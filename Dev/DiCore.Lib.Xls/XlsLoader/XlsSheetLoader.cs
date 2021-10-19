using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Aspose.Cells;
using DiCore.Lib.Xls.Types;
using DiCore.Lib.Xls.Types.Attributes;
using DiCore.Lib.Xls.Types.Helpers;

namespace DiCore.Lib.Xls.XlsLoader
{
    /// <summary>
    /// Класс загружающий данные из файла Excel
    /// </summary>
    public class XlsSheetLoader<T> : IXlsLoader<T>
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="fileStream">Файловый поток (Excel)</param>
        /// <param name="sheetName">Название страницы (Excel)</param>
        /// <param name="beforeSetValue">Метод, вызываемый перед записью значения свойства и позволяющий изменить значение.
        /// Особенности: для полей, имеющих тип отличный от типа string
        /// применяется совместно с атрибутом AsStringAttribute.
        /// Пример: позволяет объявить свойство типа enum и для каждого строкового значения, 
        /// вернуть для записи нужное значение типа enum
        /// </param>
        public XlsSheetLoader(FileStream fileStream, string sheetName, BeforeSetValueDelegate beforeSetValue = null)
        {
            FileStream = fileStream;
            SheetName = sheetName;
            Errors = new List<InputFormError>();
            BeforeSetValue = beforeSetValue;
        }

        /// <summary>
        /// Поток (Excel)
        /// </summary>
        private FileStream FileStream { get; set; }

        /// <summary>
        /// Имя страницы документа
        /// </summary>
        public string SheetName { get; protected set; }

        /// <summary>
        /// Ошибки во входной форме
        /// </summary>
        public List<InputFormError> Errors { get; protected set; }

        /// <summary>
        /// Класс с данными
        /// </summary>
        public T DataObject { get; protected set; }

        /// <summary>
        /// Номер строки в котором начинаются данные в таблицах
        /// </summary>
        public int StartTableColumn { get; protected set; } = 0;

        private Workbook workbook { get; set; }

        private BeforeSetValueDelegate BeforeSetValue;

        /// <summary>
        /// Загрузка данных из входного файла
        /// </summary>
        /// <returns>Результат (true, если загрузка завершилась без ошибок и false - если с ошибками.)
        /// Ошибки можно получить с помощью метода GetErrors</returns>
        public bool Load()
        {
            Type typeObj;
            var listType = typeof (T);
            if (listType.Name.Contains("List`1"))
                typeObj = listType.GetGenericArguments()[0];
            else
                return false;
            
            var startTableRow = FirstDataRowAttribute.GetFirstDataRow(typeObj);
            if (startTableRow == null)
                return false;

            workbook = new Workbook(FileStream);

            var sheet = workbook.Worksheets[SheetName];
            if (sheet == null)
            {
                Errors.Add(new InputFormError(SheetName, "Отсутствует страница"));
                return false;
            }

            var columnRange = ColumnRangeAttribute.Get<T>();
            var endTableRow = InputFormCellsHelper.GetEndTableRow(sheet, startTableRow.Value, columnRange,
                StartTableColumn);

            var propsObj = (from pr in typeObj.GetProperties()
                where ColumnAttribute.GetColumnName(pr) != null
                select new
                {
                    Prop = pr,
                    ColumnName = ColumnAttribute.GetColumnName(pr),
                    AsString = AsStringAttribute.Exist(pr),
                    Nullable = AllowNullAttribute.Exist(pr)
                }).ToArray();

            DataObject = Activator.CreateInstance<T>();
            var list = DataObject;
            var listAddMethod = listType.GetMethod("Add");

            for (int i = startTableRow.Value; i < endTableRow; i++)
            {
                dynamic objectInstance = Activator.CreateInstance(typeObj);

                foreach (var propInfo in propsObj)
                {
                    string cellsName = $"{propInfo.ColumnName}{i}";
                    if (!InputFormCellsHelper.CheckCellValueForNull(sheet.Cells[cellsName], propInfo.Nullable) &&
                        String.IsNullOrEmpty(sheet.Cells[cellsName].StringValue))
                    {
                        Errors.Add(new InputFormError(sheet.Name, cellsName, "Ячейка обязательна для заполнения"));
                        continue;
                    }

                    object value;
                    bool result;
                    if (propInfo.AsString)
                        result = GetCellValue(SheetName, typeof (String), sheet.Cells[cellsName], out value);
                    else
                        result = GetCellValue(SheetName, propInfo.Prop.PropertyType, sheet.Cells[cellsName],
                            out value);

                    if (BeforeSetValue != null)
                    {
                        var val = value as string;

                        if (val != null)
                        {
                            if (propInfo.Prop.PropertyType.IsGenericType &&
                                propInfo.Prop.PropertyType.GetGenericTypeDefinition() == typeof (Nullable<>))
                            {
                                value = BeforeSetValue(val, Nullable.GetUnderlyingType(propInfo.Prop.PropertyType));
                            }
                            else
                                value = BeforeSetValue(val, propInfo.Prop.PropertyType);
                        }
                    }

                    if (result)
                        propInfo.Prop.SetValue(objectInstance, value, null);
                }
                listAddMethod.Invoke(list, new[] {objectInstance.ToThisType()});
            }
            return Errors.Count == 0;
        }

        /// <summary>
        /// Получение класса с данными
        /// </summary>
        /// <returns></returns>
        public T GetResult()
        {
            return DataObject;
        }

        /// <summary>
        /// Получение списка ошибок </summary>
        /// <returns></returns>
        public List<InputFormError> GetErrors()
        {
            return Errors;
        }

        /// <summary>
        /// Получение списка свойств типа List
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private List<PropertyInfo> GetListTypeProperties(T obj)
        {
            var result = new List<PropertyInfo>();
            foreach (var propInfo in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (propInfo.PropertyType.Name.Contains("List`1"))
                    result.Add(propInfo);
            }
            return result;
        }

        /// <summary>
        /// Получение значения ячейки заданого типа
        /// </summary>
        private bool GetCellValue(string sheetName, Type type, Cell cell, out object value, bool nullable = false)
        {
            value = null;
            var cellStringValue = cell.StringValue;
            switch (type.Name)
            {
                case "Int32":
                {
                    if (!nullable && CheckStringValueForNull(cellStringValue))
                    {
                        AddError(sheetName, cell.Name, cellStringValue, "В ячейке должно быть целое число");
                        return false;
                    }

                    int dt;
                    if (Int32.TryParse(cellStringValue, out dt))
                        value = dt;
                    return true;
                }
                case "Short":
                {
                    if (!nullable && CheckStringValueForNull(cellStringValue))
                    {
                        AddError(sheetName, cell.Name, cellStringValue, "В ячейке должно быть целое число");
                        return false;
                    }

                    short dt;
                    if (Int16.TryParse(cellStringValue, out dt))
                        value = dt;
                    return true;
                }
                case "Int16":
                {
                    if (!nullable && CheckStringValueForNull(cellStringValue))
                    {
                        AddError(sheetName, cell.Name, cellStringValue, "В ячейке должно быть целое число");
                        return false;
                    }

                    short dt;
                    if (Int16.TryParse(cellStringValue, out dt))
                        value = dt;
                    return true;
                }
                case "String":
                {
                    value = cell.StringValue;
                    return true;
                }
                case "Double":
                {
                    if (!nullable && CheckStringValueForNull(cellStringValue))
                    {
                        AddError(sheetName, cell.Name, cellStringValue, "В ячейке должно быть число с плавающей точкой");
                        return false;
                    }

                    double dt;
                    if (double.TryParse(cellStringValue, out dt))
                        value = dt;
                    return true;
                }
                case "Single":
                {
                    if (!nullable && CheckStringValueForNull(cellStringValue))
                    {
                        AddError(sheetName, cell.Name, cellStringValue, "В ячейке должно быть число с плавающей точкой");
                        return false;
                    }

                    float dt;
                    if (float.TryParse(cellStringValue, out dt))
                        value = dt;
                    return true;
                }
                case "Boolean":
                {
                    if (!nullable && CheckStringValueForNull(cellStringValue))
                    {
                        AddError(sheetName, cell.Name, cellStringValue,
                            "В ячейке должно быть значение \"Да\" или \"Нет\"");
                        return false;
                    }

                    if (cellStringValue == "Да")
                        value = true;
                    else if (cellStringValue == "Нет")
                        value = false;
                    else
                    {
                        AddError(sheetName, cell.Name, cellStringValue,
                            "В ячейке должно быть значение \"Да\" или \"Нет\"");
                        return false;
                    }
                    return true;
                }
                case "DateTime":
                {
                    if (!nullable && CheckStringValueForNull(cellStringValue))
                    {
                        AddError(sheetName, cell.Name, cellStringValue,
                            "В ячейке должна быть дата формата \"dd.MM.yyyy\"");
                        return false;
                    }

                    DateTime dt;
                    if (DateTime.TryParse(cellStringValue, out dt))
                        value = dt;
                    return true;
                }
                default:
                {
                    var underlyingType = Nullable.GetUnderlyingType(type);
                    if (underlyingType != null) // nullable типы
                    {
                        return GetCellValue(sheetName, underlyingType, cell, out value, true);
                    }

                    if (type.BaseType.Name == "Enum")
                    {
                        var fields = type.GetFields();
                        foreach (var fieldInfo in fields)
                        {
                            var description = GetDescriptionAttribute(fieldInfo);
                            if (description != null && description.Description == cellStringValue)
                            {
                                value = GetDescriptionAttribute(type, fieldInfo);
                                return true;
                            }
                        }
                        if (!(nullable && (string.IsNullOrEmpty(cellStringValue) || cellStringValue == "-")))
                        {
                            var descriptions = GetEnumDescriptions(type);
                            string values = string.Join(", ", descriptions);
                            AddError(sheetName, cell.Name, cellStringValue,
                                $"В ячейке должно быть значение из диапазона: {values}");
                            return false;
                        }
                        return true;
                    }
                    throw new ArgumentException();
                }
            }
        }

        private void AddError(string sheetName, string cellName, string cellValue, string addingMessage)
        {
            Errors.Add(new InputFormError(sheetName, cellName,
                $"Значение: {cellValue} не соответствует ожидаемому. {addingMessage}"));
        }

        private List<string> GetEnumDescriptions(Type enumType)
        {
            var result = new List<string>();
            var fields = enumType.GetFields();
            foreach (var fieldInfo in fields)
            {
                var attr = fieldInfo.GetCustomAttribute(typeof (DescriptionAttribute));
                if (attr is DescriptionAttribute)
                {
                    result.Add((attr as DescriptionAttribute).Description);
                }
            }
            return result;
        }


        /// <summary>
        /// Получение списка свойств типа List
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private List<PropertyInfo> GetTypeProperties(T obj)
        {
            var result = new List<PropertyInfo>();
            foreach (var propInfo in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (CellsAttribute.Exist(propInfo) || PictureSheetAttribute.Exist(propInfo))
                    result.Add(propInfo);
            }
            return result;
        }

        private DescriptionAttribute GetDescriptionAttribute(FieldInfo fi)
        {
            var attr = fi.GetCustomAttribute(typeof (DescriptionAttribute));
            return attr as DescriptionAttribute;
        }

        private object GetDescriptionAttribute(Type type, FieldInfo fi)
        {
            var val = Enum.Parse(type, fi.Name);
            return val;
        }

        private bool CheckStringValueForNull(string str)
        {
            return String.IsNullOrEmpty(str) || str == "-" || str == "--" || str == "---" || str == " " || str == "  ";
        }
    }
}
