using System;
using System.IO;
using System.Linq;
using DiCore.Lib.TankValidate;

namespace DiCore.Lib.TankValidateTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileXml = new MemoryStream();
            var fileExcel = new MemoryStream();
            using (var fsxls = File.OpenRead(@"D:\testValid\Шаблон форм в соответст.XLS"))
            {
                fsxls.CopyTo(fileExcel);
                fileExcel.Position = 0;
            }
            using (var fsxml = File.OpenRead(@"D:\testValid\Проверка входных форм РВС v3.1.XML"))
            {
                fsxml.CopyTo(fileXml);
                fileXml.Position = 0;
            }


            var xmlFile = GenerateStreamFromString(Properties.Resources.Проверка_входных_форм_РВС_v3_1);



            IValidator validator = new Validator();

            var syntaxErrors = validator.LoadFile(xmlFile, fileExcel);
            if (syntaxErrors.Any())
            {
                foreach (var error in syntaxErrors)
                {
                    Console.WriteLine(error.ToText());
                }
                Console.ReadKey();
                return;
                
            }

            var errors = validator.Execute();
            if (!errors.Any())
            {
                Console.WriteLine("Файл корректный. Ошибок не найдено");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Файл содержит ошибки");

                //var excelErrors = validator.GetExcelErrorsMemoryStream(errors.ToArray());
                validator.ExcelErrorsSave(errors.ToArray(), @"D:\testValid\output.xlsx");
                validator.InputExcelAppendErrorsSave(errors, @"D:\testValid\outputWithErrors.xlsx");

                foreach (var error in errors)
                {
                    Console.WriteLine(error.ToText());
                }
                Console.ReadKey();
            }
        }



        private static MemoryStream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
