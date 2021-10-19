using System;
using System.Diagnostics;
using NpgsqlTypes;

namespace DiCore.Lib.PgSqlAccess.Types
{
    /// <summary>
    /// ��������� ��������
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class PropertyDescription
    {
        /// <summary>
        /// ���������� ����� ��������
        /// </summary>
        public int? Order { get; set; }

        /// <summary>
        /// ��� ��������
        /// </summary>
        public NpgsqlDbType Type { get; set; }

        /// <summary>
        /// �������� ��������
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ��� ��������
        /// </summary>
        public Type PropertyType { get; set; }
    }
}