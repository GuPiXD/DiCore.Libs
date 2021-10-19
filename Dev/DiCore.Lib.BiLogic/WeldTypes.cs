using System;

namespace DiCore.Lib.BiLogic
{
    /// <summary>
    /// Тип шва
    /// </summary>
    [Flags]
    public enum WeldTypes
    {
        /// <summary>
        /// Поперечный
        /// </summary>
        Cross = 0,
        /// <summary>
        /// Продольный
        /// </summary>
        Longitudinal=1
    }
}
