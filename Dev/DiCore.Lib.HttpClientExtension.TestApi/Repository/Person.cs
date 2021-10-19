using System;

namespace DiCore.Lib.RestClient.TestCore.Api.Repository
{
    public class Person : IEquatable<Person>

    {
        public string Name { get; set; }
        public int Age { get; set; }
        public bool Active { get; set; }

        public bool Equals(Person other)
        {
            return string.Equals(Name, other.Name) && Age == other.Age && Active == other.Active;
        }
    }
}