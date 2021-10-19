using System;
using System.Collections.Generic;
using System.Text;

namespace DiCore.Lib.TestModels.Models
{
    public class JsonModelWrapper<T, E>
    {
        public int Id { get; set; }
        public T Value { get; set; }

        public EventParameters<E> EventParameters { get; set; }
    }

    public class EventParameters<T>
    {
        public Guid Id { get; set; }
        public T Value { get; set; }
    }
}
