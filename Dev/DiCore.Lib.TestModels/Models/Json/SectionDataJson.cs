using System;
using System.Collections.Generic;
using System.Text;

namespace DiCore.Lib.TestModels.Models.Json
{
    public class SectionDataJson
    {
        /// <summary>
        /// Радиус изгиба
        /// </summary>
        public int? BendingRadius { get; set; }

        /// <summary>
        /// Направление изгиба
        /// </summary>
        public int? BendingDirection { get; set; }

        /// <summary>
        /// Средняя толщина стенки на двухшовной секции с углами от 270 - 90 , мм
        /// </summary>
        public float? FirstListWT { get; set; }

        /// <summary>
        /// Средняя толщина стенки на двухшовной секции с углами от 90 - 270 , мм
        /// </summary>
        public float? SecondListWT { get; set; }
    }
}
