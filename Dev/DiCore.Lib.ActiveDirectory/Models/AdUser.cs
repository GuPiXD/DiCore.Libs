using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiCore.Lib.ActiveDirectory.Models
{
    public class AdUser
    {
        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Уникальный идентификатор в границах контроллера домена
        /// </summary>
        public string Sid { get; set; }
        
        /// <summary>
        /// Логин
        /// </summary>
        public string Login { get; set; }

        public string Principal { get; set; }
        /// <summary>
        /// Табельный номер
        /// </summary>
        public int? Number { get; set; }

        /// <summary>
        /// Домен
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Телефон
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Адрес электронной почты
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Организация
        /// </summary>
        public string Organization { get; set; }

        /// <summary>
        /// Адрес
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Компания
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// Должность
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// Город
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Помещение
        /// </summary>
        public string Room { get; set; }

        /// <summary>
        /// ФИО
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Имя
        /// </summary>
        public string FirstName
        {
            get
            {
                var fioSplit = Regex.Split(FullName, @"\s+");
                if (fioSplit.Count() > 2)
                    return fioSplit[1];
                return "";
            }
        }

        /// <summary>
        /// Отчество
        /// </summary>
        public string SecondName
        {
            get
            {
                var fioSplit = Regex.Split(FullName, @"\s+");
                if (fioSplit.Count() > 2)
                    return fioSplit[2];
                return "";
            }
        }

        /// <summary>
        /// Фамилия
        /// </summary>
        public string LastName
        {
            get
            {
                var fioSplit = Regex.Split(FullName, @"\s+");
                if (fioSplit.Count() > 2)
                    return fioSplit[0];
                return "";
            }
        }
    }
}
