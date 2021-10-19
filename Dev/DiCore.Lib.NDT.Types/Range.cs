using System;
using System.Diagnostics;

namespace DiCore.Lib.NDT.Types
{
    /// <summary>
    /// Тип описывающий диапазон величин
    /// </summary>    
    [DebuggerDisplay("[{Begin.ToString()}:{End.ToString()}]")]
    public struct Range<T>
    {
        public T Begin { get; set; }
        public T End { get; set; }

        public Range(T beg, T end) : this()
        {
            Begin = beg;
            End = end;
        }

        public static implicit operator Range<T>(Range<float> source)
        {
            var type = typeof(T);
            return new Range<T>((T)Convert.ChangeType(source.Begin, type),
                (T)Convert.ChangeType(source.End, type));
        }

        public override string ToString()
        {
            return $"[{Begin}:{End}]";
        }
    }
}
