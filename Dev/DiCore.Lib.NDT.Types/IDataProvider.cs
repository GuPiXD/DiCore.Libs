using System;

namespace DiCore.Lib.NDT.Types
{
    /// <summary>
    /// ������� ��������� ����������� ������  
    /// </summary>
    public interface IDataProvider: IDisposable
    {
        /// <summary>
        /// ������������ �������
        /// </summary>
        string Name { get; }
        /// <summary>
        /// �������� �������
        /// </summary>
        string Description { get; }
        /// <summary>
        /// ����������� �������
        /// </summary>
        string Vendor { get; }
        /// <summary>
        /// ���� ��������� ������
        /// </summary>
        bool IsOpened { get; }
        /// <summary>
        /// �������� ������ � ������� ������������, �
        /// </summary>
        double SectionOffset { get; }

        /// <summary>
        /// ������������ ���� � ������ ������� ����
        /// </summary>
        int MaxScan { get; }

        /// <summary>
        /// ����������� ���� � ������ ������� ����
        /// </summary>
        int MinScan { get; }

        short SensorCount { get; }

        /// <summary>
        /// �������� ��������������� ������
        /// </summary>
        /// <param name="location">��������� ���������� ������</param>
        /// <returns></returns>
        bool Open(DataLocation location);
        /// <summary>
        /// �������� ��������������� ������
        /// </summary>
        void Close();
    }
}
