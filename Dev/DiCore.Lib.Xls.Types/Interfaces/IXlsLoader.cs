using System.Collections.Generic;

namespace DiCore.Lib.Xls.Types.Interfaces
{
    public interface IXlsLoader<T>
    {
        /// <summary>
        /// �������� ������ �� �������� �����
        /// </summary>
        /// <returns>������ ������</returns>
        bool Load();

        /// <summary>
        /// ��������� ������ � �������
        /// </summary>
        /// <returns></returns>
        T GetResult();

        /// <summary>
        /// ��������� ������ ������ </summary>
        /// <returns></returns>
        List<InputFormError> GetErrors();

        /// <summary>
        /// ��� �������� �����
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// ������ �� ������� �����
        /// </summary>
        List<InputFormError> Errors { get; }

        /// <summary>
        /// ����� � �������
        /// </summary>
        T DataObject { get; }
    }
}