using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiCore.Lib.ODFormGenerator;

namespace TestFormGenerator
{
    internal class Program
    {
        private const string filePath = "out.xlsx";

        private static void Main(string[] args)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            var generator = new Generator();
            var parameters = new List<GeneratorParameter>
            {
                
                new GeneratorParameter()
                {
                    Id = "FabricationMethodId",
                    Name = "Метод изготовления",
                    Format = "справочник",
                },
                new GeneratorParameter()
                {
                    Id = "HasGrid",
                    Name = "Наличие решетки",
                    Format = "признак",
                    Range = "1 - да, 0 - нет"
                },
                new GeneratorParameter()
                {
                    Id = "Height",
                    Name = "Строительная высота тройника",
                    Format = "число",
                    Unit = "м"
                },
                new GeneratorParameter()
                {
                    Id = "PipeInstallationId",
                    Name = "Идентификатор",
                }
            };
            using (var memoryStream = new MemoryStream())
            {


                using (var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
                {
                    //generator.Generate(memoryStream, "оборудование", parameters);
                    memoryStream.Position = 0;
                    memoryStream.CopyTo(fileStream);
                }
            }
            Console.Write("OK");
            Console.ReadKey();
        }
    }
}
