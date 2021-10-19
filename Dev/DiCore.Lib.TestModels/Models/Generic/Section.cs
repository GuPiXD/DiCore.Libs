using System;
using System.Collections.Generic;
using System.Text;

namespace DiCore.Lib.TestModels.Models.Generic
{
    /// <summary>
    /// Трубная секция
    /// </summary>
    public class Section<TJson>
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        public Guid TokenId { get; set; }

        /// <summary>
        /// Дистанция (относительно точки пуска)
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// Номер секции
        /// </summary>
        public int? Number { get; set; }

        /// <summary>
        /// Тип секции
        /// </summary>
        public SectionType SectionType { get; set; }

        /// <summary>
        /// Средняя толщина стенки
        /// </summary>
        public float? AverageWallThickness { get; set; }

        /// <summary>
        /// Угол примыкания ПРШ - начальный
        /// </summary>
        public float? AxialWeldStartAngle { get; set; }

        /// <summary>
        /// Угол примыкания ПРШ - конечный
        /// </summary>
        public float? AxialWeldEndAngle { get; set; }

        /// <summary>
        /// Секция нефтепровода
        /// </summary>
        public Guid? PipelineSectionId { get; set; }

        /// <summary>
        /// Тип трубы
        /// </summary>
        public PipeType PipeType { get; set; }

        /// <summary>
        /// Высота
        /// </summary>
        public float? Altitude { get; set; }

        /// <summary>
        /// Второй угол примыкания ПРШ
        /// </summary>
        public float? AxialWeldSecondAngle { get; set; }

        /// <summary>
        /// Дополнительные данные
        /// </summary>
        public TJson Data { get; set; }

        public bool vrDeleted { get; set; }
    }
}
