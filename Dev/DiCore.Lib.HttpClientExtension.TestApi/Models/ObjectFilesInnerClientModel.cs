using System;
using System.Linq;
using DiCore.Lib.HttpClientExtension;

namespace DiCore.Lib.RestClient.TestCore.Api.Models
{
    public class ObjectFilesInnerClientModel : IEquatable<TestDataManyInner>, IDisposable
    {
        public string Name { get; set; }
        public int Len { get; set; }
        public double Angle { get; set; }
        public Guid Uid { get; set; }
        public bool Flag { get; set; }
        public DateTime Timespan { get; set; }
        public FileContent[] File { get; set; }
        public string[] StringArray { get; set; }

        protected bool StringArrayEquals(string[] array1, string[] array2)
        {
            if (array1 == null && array2 == null)
                return true;
            if (array1 == null || array2 == null)
                return false;
            if (array1.Length == 0 && array2.Length == 0)
                return true;
            if (array1.Length != array2.Length)
                return false;
            return !array1.Where((t, i) => !t.Equals(array2[i])).Any();
        }

        protected bool DateTimeEquals(DateTime dt1, DateTime dt2)
        {
            return dt1.Year == dt2.Year && dt1.Month == dt2.Month && dt1.Day == dt2.Day && dt1.Hour == dt2.Hour &&
                   dt1.Minute == dt2.Minute && dt1.Second == dt2.Second;
        }

        public bool Equals(TestDataManyInner other)
        {
            return Name == other.Name && Len == other.Len && Math.Abs(Angle - other.Angle) < .001 &&
                   Flag == other.Flag && Uid == other.Uid && DateTimeEquals(Timespan, other.Timespan) &&
                   StringArrayEquals(StringArray, other.StringArray);
        }

        public void Dispose()
        {
            if (File == null)
                return;
            foreach (var fileDescription in File)
            {
                fileDescription?.Dispose();
            }
        }
    }
}