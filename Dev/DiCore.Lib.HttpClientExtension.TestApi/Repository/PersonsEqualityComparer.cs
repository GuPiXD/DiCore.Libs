using System.Collections.Generic;

namespace DiCore.Lib.RestClient.TestCore.Api.Repository
{
    public class PersonsEqualityComparer : IEqualityComparer<Person>
    {
        public bool Equals(Person x, Person y)
        {
            if (x == null || y == null)
                return false;
            return x.Equals(y);
        }

        public int GetHashCode(Person obj)
        {
            return obj?.Name?.GetHashCode() ^ obj?.Age.GetHashCode() ^ obj?.Active.GetHashCode() ?? 0;
        }
    }
}