using System.Collections.Generic;

namespace DiCore.Lib.RestClient.TestCore.Api.Repository
{
    public class PersonItemssEqualityComparer : IEqualityComparer<PersonItem>
    {
        public bool Equals(PersonItem x, PersonItem y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            return new PersonsEqualityComparer().Equals(x, y) && x.Id == y.Id;
        }

        public int GetHashCode(PersonItem obj)
        {
            return new PersonsEqualityComparer().GetHashCode(obj) ^ (obj?.Id.GetHashCode() ?? 0);
        }
    }
}