using System;

namespace DiCore.Lib.RestClient.TestCore.Api.Repository
{
    public class PersonItem : Person, IDbItem<Guid>, IEquatable<PersonItem>
    {
        public Guid Id { get; set; }

        public bool Equals(PersonItem other)
        {
            return base.Equals(other) && Id == other.Id;
        }
    }
}