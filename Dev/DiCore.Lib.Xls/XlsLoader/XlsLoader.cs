using System;
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
    public delegate object BeforeSetValueDelegate(string val, Type type);

    /// <summary>
    /// Класс загружающий данные из файла Excel
    /// </summary>
    public class XlsLoader<T> : IXlsLoader<T>
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="fileStream">Файловый поток</param>
        /// <param name="beforeSetValue">Метод, вызываемый перед записью значения свойства и позволяющий изменить значение.
        /// Особенности: для полей, имеющих тип отличный от типа string
        /// применяется совместно с атрибутом AsStringAttribute.
        /// Пример: позволяет объявить свойство типа enum и для каждого строкового значения, 
        /// вернуть для записи нужное значение типа enum
        /// </param>
        public XlsLoader(FileStream fileStream, BeforeSetValueDelegate beforeSetValue = null)
        {
            FileStream = fileStream;
            Errors = new List<InputFormError>();
            BeforeSetValue = beforeSetValue;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="fileName">Имя файла (Excel)</param>
        /// <param name="beforeSetValue">Метод, вызываемый перед записью значения свойства и позволяющий изменить значение.
        /// Особенности: для полей, имеющих тип отличный от типа string
        /// применяется совместно с атрибутом AsStringAttribute.
        /// Пример: позволяет объявить свойство типа enum и для каждого строкового значения, 
        /// вернуть для записи нужное значение типа enum
        /// </param>
        public XlsLoader(string fileName, BeforeSetValueDelegate beforeSetValue = null)
            : this(new FileStream(fileName, FileMode.Open), beforeSetValue)
        {
            Errors = new List<InputFormError>();
            BeforeSetValue = beforeSetValue;
        }

        /// <summary>
        /// Имя входного файла Excel
        /// </summary>
        public string FileName { get; protected set; }

        public FileStream FileStream { get; protected set; }

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
            LoadMainParameters(DataObject);

            var listProperties = GetListTypeProperties(DataObject); // получаем списки
            foreach (var listProperty in listProperties)
            {
                var sheetName = SheetAttribute.GetSheetName(listProperty);
                var startTableRow = FirstDataRowAttribute.GetFirstDataRow(listProperty);
                if (startTableRow == null) // если не задана строка с которой начинаются данные, то пропускаем список
                    continue;

                var sheet = workbook.Worksheets[sheetName];
                if (sheet == null)
                {
                    Errors.Add(new InputFormError(sheetName, "Отсутствует страница"));
                    continue;
                }

                var columnRange = ColumnRangeAttribute.Get(listProperty);
                var endTableRow = InputFormCellsHelper.GetEndTableRow(sheet, startTableRow.Value, columnRange,
                    StartTableColumn);

                var props =
                    listProperty.PropertyType.GetGenericArguments()[0].GetProperties(BindingFlags.Public |
                                                                                     BindingFlags.Instance);
                var propsObj = (from pr in props
                    where ColumnAttribute.GetColumnName(pr) != null
                    select new
                    {
                        Prop = pr,
                        ColumnName = ColumnAttribute.GetColumnName(pr),
                        AsString = AsStringAttribute.Exist(pr),
                        Nullable = AllowNullAttribute.Exist(pr)
                    }).ToArray();
                var typeObj = listProperty.PropertyType.GetGenericArguments()[0];

                var list = DataObject.GetType().GetProperty(listProperty.Name).GetValue(DataObject, null);
                var listAddMethod = listProperty.PropertyType.GetMethod("Add");

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
                            result = GetCellValue(sheetName, typeof(String), sheet.Cells[cellsName], out value);
                        else
                            result = GetCellValue(sheetName, propInfo.Prop.PropertyType, sheet.Cells[cellsName], out value);

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
            }
            return Errors.Count == 0;
        }

        private void LoadMainParameters(T obj)
        {
            Errors.Clear();

            using (FileStream)
            {
                workbook = new Workbook(FileStream);

                Aspose.Cells.Properties.DocumentPropertyCollection customProperties = workbook.CustomDocumentProperties;
                Aspose.Cells.Properties.DocumentProperty customProperty = customProperties["RouteId"];

                DataObject = Activator.CreateInstance<T>();

                var mainSheetName = SheetAttribute.GetSheetName<T>();
                if (mainSheetName == null)
                    return;

                var sheet = workbook.Worksheets[mainSheetName];
                if (sheet == null)
                    Errors.Add(new InputFormError(mainSheetName, "Отсутствует страница основных параметров"));
                else
                {
                    var properties = GetTypeProperties(DataObject);
                    foreach (var property in properties)
                    {
                        var cellName = CellsAttribute.GetCellsName(property);
                        if (cellName != null)
                        {
                            var cell = sheet.Cells[cellName];
                            if (!InputFormCellsHelper.CheckCellValueForNull(cell, property))
                            {
                                Errors.Add(new InputFormError(sheet.Name, cell.Name, "Ячейка обязательна для заполнения"));
                                continue;
                            }
                            object value;
                            var result = GetCellValue(sheet.Name, property.PropertyType, sheet.Cells[cell.Name],
                                out value);
                            if (result)
                                property.SetValue(DataObject, value, null);
                        }
                    }

                    var rootProperty = DataObject.GetType().GetProperties().SingleOrDefault(p => p.Name == "RouteId");

                    rootProperty?.SetValue(DataObject, new Guid(customProperty.Value.ToString()), null);
                }
            }
        }

        private List<InputFormError> LoadPictures(T obj)
        {
            var errors = new List<InputFormError>();
            using (FileStream)
            {
                workbook = new Workbook(FileStream);
                var properties = GetTypeProperties(DataObject);
                foreach (var property in properties)
                {
                    var pictureSheetName = PictureSheetAttribute.GetSheetName(property);
                    if (pictureSheetName != null)
                    {
                        var sheet = workbook.Worksheets[pictureSheetName];
                        if (sheet != null)
                        {
                            var minCount = PictureSheetAttribute.GetPictureMinCount(property);
                            var maxCount = PictureSheetAttribute.GetPictureMaxCount(property);
                            var isLabel = PictureSheetAttribute.GetPictureIsLabel(property);

                            if (isLabel != null && isLabel == true)
                            {
                                if (sheet.TextBoxes.Count > 0)
                                {
                                    property.SetValue(DataObject, sheet.TextBoxes[0].Text, null);
                                }
                                continue;
                            }

                            if (!(sheet.Pictures.Count >= minCount && sheet.Pictures.Count <= maxCount))
                            {
                                errors.Add(new InputFormError(sheet.Name, null,
                                    "Рисунков должно быть " + maxCount + (minCount == 0 ? " или 0." : ".")));
                            }

                            if (sheet.Pictures.Count > 0)
                            {
                                property.SetValue(DataObject, sheet.Pictures[0].Data, null);
                            }
                        }
                    }
                }
            }
            return errors;
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
                        AddError(sheetName, cell.Name, cellStringValue, "В ячейке должно быть значение \"Да\" или \"Нет\"");
                        return false;
                    }

                    if (cellStringValue == "Да")
                        value = true;
                    else if (cellStringValue == "Нет")
                        value = false;
                    else
                    {
                        AddError(sheetName, cell.Name, cellStringValue, "В ячейке должно быть значение \"Да\" или \"Нет\"");
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
                        return GetCellValue(sheetName, underlyingType, cell, out value, true);

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
