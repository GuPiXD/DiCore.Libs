namespace DiCore.Lib.ActiveDirectory.Models
{
    public enum AdUserParameters
    {
        Mail,
        /// <summary>
        /// First Name
        /// </summary>
        GivenName,
        /// <summary>
        /// Middle Name
        /// </summary>
        MiddleName,
        /// <summary>
        /// Name
        /// </summary>
        Cn,
        /// <summary>
        /// Табельный номер
        /// </summary>
        EmployeeId,
        /// <summary>
        /// ФИО
        /// </summary>
        DisplayName,
        /// <summary>
        /// Помещение
        /// </summary>
        PhysicalDeliveryOfficeName,
        /// <summary>
        /// Телефон
        /// </summary>
        TelephoneNumber,
        /// <summary>
        /// Должность
        /// </summary>
        Title,
        /// <summary>
        /// Должность
        /// </summary>
        Description,
        Department,
        Company,
        Info,
        StreetAddress,
        Ou,
        SamAccountName,
        /// <summary>
        /// Zip/Postal Code
        /// </summary>
        PostalCode,
        /// <summary>
        /// State/province
        /// </summary>
        St,
        /// <summary>
        /// Country/region
        /// </summary>
        Co,
        UserWorkstations,
        /// <summary>
        /// Членство в группах
        /// </summary>
        MemberOf,
        Domain,
        /// <summary>
        /// Город
        /// </summary>
        L,
        UserPrincipalName,
        ObjectGuid,
        ObjectSid,
        Name
    }
}