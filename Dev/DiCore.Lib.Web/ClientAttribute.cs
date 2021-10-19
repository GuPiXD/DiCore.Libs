using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiCore.Lib.Web
{
    public class ClientAttribute
    {
        public static ClientAttribute NullAttribute => new ClientAttribute();

        public ClientAttribute(string name, long value)
        {
            Name = name;
            Value = value;
        }

        public ClientAttribute(string name, long? value)
        {
            Name = name;
            Value = value;
        }

        public ClientAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public ClientAttribute(string name, Guid value)
        {
            Name = name;
            Value = value;
        }

        public ClientAttribute(string name, Guid? value)
        {
            Name = name;
            Value = value;
        }

        public ClientAttribute(string name, double value)
        {
            Name = name;
            Value = value;
        }

        public ClientAttribute(string name, double? value)
        {
            Name = name;
            Value = value;
        }

        public ClientAttribute(string name, DateTime value)
        {
            Name = name;
            Value = value;
        }

        public ClientAttribute(string name, DateTime? value)
        {
            Name = name;
            Value = value;
        }

        private ClientAttribute(long value)
        {
            Name = null;
            Value = value;
        }

        private ClientAttribute(string value)
        {
            Name = null;
            Value = value;
        }

        private ClientAttribute(Guid value)
        {
            Name = null;
            Value = value;
        }

        private ClientAttribute()
        {
            Name = null;
            Value = null;
        }

        public string Name { get; }
        public object Value { get; }

        public static implicit operator ClientAttribute(string value)
        {
            return new ClientAttribute(value);
        }

        public static implicit operator ClientAttribute(long value)
        {
            return new ClientAttribute(value);
        }

        public static implicit operator ClientAttribute(Guid value)
        {
            return new ClientAttribute(value);
        }

        internal static ClientAttribute[] PropertiesToAttributes(params object[] objects)
        {
            var attributes = new List<ClientAttribute>();

            foreach (var o in objects)
            {
                var properties =
                    o.GetType()
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);

                
                foreach (var property in properties)
                {

                    attributes.Add(new ClientAttribute(property.Name, property.GetValue(o, null)?.ToString()));
                }
            }

            return attributes.ToArray();
        }
    }
}
