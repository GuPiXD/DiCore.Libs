using System;

namespace DiCore.Lib.RestClient.TestCore.Api.Models
{
    public class ObjectFilesClientModel : ObjectFilesInnerClientModel, IEquatable<TestDataMany>
    {
        public ObjectFilesInnerClientModel InnerModel { get; set; }

        protected bool InnerModelEquals(ObjectFilesInnerClientModel innerTestModel, TestDataManyInner testDataManyInner)
        {
            if (innerTestModel == null && testDataManyInner == null)
                return true;
            if (innerTestModel == null || testDataManyInner == null)
                return false;
            return innerTestModel.Equals(testDataManyInner);
        }

        public bool Equals(TestDataMany other)
        {
            return base.Equals(other) && InnerModelEquals(InnerModel, other.InnerModel);
        }
    }
}