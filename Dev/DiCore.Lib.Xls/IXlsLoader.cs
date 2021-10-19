using System.Collections.Generic;
using DiCore.Lib.Xls.Types;

namespace DiCore.Lib.Xls
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
        /// ������ �� ������� �����
        /// </summary>
        List<InputFormError> Errors { get; }

        /// <summary>
        /// ����� � �������
        /// </summary>
        T DataObject { get; }
    }
}