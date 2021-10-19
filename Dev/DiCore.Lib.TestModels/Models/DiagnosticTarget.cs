using System;

namespace DiCore.Lib.TestModels.Models
{
    /// <summary>
    /// Задание на диагностику
    /// </summary>
    public class DiagnosticTarget
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Заказчик
        /// </summary>
        public Guid CustomerContractorId { get; set; }

        /// <summary>
        /// Дата начала задания
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Дата окончания
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Организация, проводящая диагностику
        /// </summary>
        public Guid? PerformerContractorId { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Код прогона
        /// </summary>
        public string RunCode { get; set; }

        /// <summary>
        /// Номер отчёта
        /// </summary>
        public string ReportNumber { get; set; }

        public int DiameterId { get; set; }

        /// <summary>
        /// Диаметр участка
        /// </summary>
        public ConstructiveDiameter Diameter { get; set; }

        /// <summary>
        /// Участок начала диагностики
        /// </summary>
        public Guid StartRouteId { get; set; }

        /// <summary>
        /// Дистанция на стартовом участке, км
        /// </summary>
        public double StartRouteDistance { get; set; }

        /// <summary>
        /// Участок окончания диагностики
        /// </summary>
        public Guid EndRouteId { get; set; }

        /// <summary>
        /// Дистанция на конечном участке трубопровода
        /// </summary>
        public double EndRouteDistance { get; set; }

        /// <summary>
        /// Идентификатор прибора
        /// </summary>
        public Guid? PigId { get; set; }

        /// <summary>
        /// Справочник методов диагностики
        /// </summary>
        public DiagnosticMethod DiagnosticMethod { get; set; }
    }

    public class DiagnosticTargetInsert: DiagnosticTarget
    {
        public Guid ReasonEventId { get; set; }
    }
}
