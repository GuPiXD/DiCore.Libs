using System;

namespace DiCore.Lib.SqlDataQuery.SqlCode
{
    public abstract class BaseCondition
    {
        /// <summary>
        /// Внешний оператор условия
        /// </summary>
        public EnConditionType ExternalOperator { get; set; }

        public virtual string ToString(bool single)
        {
            throw new NotImplementedException();
        }
    }
}
