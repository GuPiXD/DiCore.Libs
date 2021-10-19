using System.Collections.Generic;
using System.Linq;

namespace DiCore.Lib.RestClient.TestCore.Api.Repository
{
    public static class PersonHelper
    {
        public static bool ItemEquals(this Person[] persons, Person[] otherPersons)
        {
            return persons.Length == otherPersons.Length &&
                   !persons.Except(otherPersons, new PersonsEqualityComparer()).Any() &&
                   !otherPersons.Except(persons, new PersonsEqualityComparer()).Any();
        }

        public static bool ItemEquals(this PersonItem[] personItems, PersonItem[] otherPersonItems)
        {
            return personItems.Length == otherPersonItems.Length &&
                   !personItems.Except(otherPersonItems, new PersonItemssEqualityComparer()).Any() &&
                   !otherPersonItems.Except(personItems, new PersonItemssEqualityComparer()).Any();
        }
    }
}