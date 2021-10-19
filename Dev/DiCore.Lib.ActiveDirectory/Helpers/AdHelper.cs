using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiCore.Lib.ActiveDirectory.Models;

namespace DiCore.Lib.ActiveDirectory.Helpers
{
    /// <summary>
    /// Класс для работы с AD
    /// </summary>
    public class AdHelper
    {
        /// <summary>
        /// Основной домен
        /// </summary>
        private readonly string domainName;

        /// <summary>
        /// Фильтр получения пользователя по табельному номер
        /// </summary>
        private const string LdapFilterUserForNumber = @"(&(objectCategory=person)(objectClass=user)(employeeID={0}))";

        /// <summary>
        /// Фильтр получения пользователя по ФИО
        /// </summary>
        private const string LdapFilterUserForName = @"(&(objectCategory=person)(objectClass=user)(|(name={0})(cn={0})))";

        /// <summary>
        /// Фильтр получения пользователя по ФИО
        /// </summary>
        private const string LdapFilterUserForLogin = @"(&(objectCategory=person)(objectClass=user)(userPrincipalName={0}))";

        /// <summary>
        /// Фильтр получения пользователя по Sid
        /// </summary>
        private const string LdapFilterUserForSid = @"(&(objectCategory=person)(objectClass=user)(objectSid={0}))";

        /// <summary>
        /// Фильтр получения списка доверенных доменов
        /// </summary>
        private const string LdapFilterTrustedDomains = @"(objectClass=trustedDomain)";

        /// <summary>
        /// Фильтр поиска пользователя по ФИО или учётной записи
        /// </summary>
        private const string LdapFilterUsersSearchForName =
            @"(&(objectCategory=person)(objectClass=user)(|(name={0}*)(cn={0}*)(userPrincipalName={0}*)))";

        /// <summary>
        /// Фильтр поиска имени ДНС по имени netbios
        /// </summary>
        private const string LdapFilterGetDnsName = @"(&(objectClass=trustedDomain)(flatName={0}))";

        /// <summary>
        /// Фильтр поиска имени netbios по имени DNS
        /// </summary>
        private const string LdapFilterGetNetbiosName = @"(&(objectClass=trustedDomain)(|(cn={0})(trustPartner={0})))";

        public AdHelper() : this(GetCurrentDomainName())
        {

        }

        public AdHelper(string domainName)
        {
            this.domainName = domainName;
        }

        public static bool IsSid(string id)
        {
            var pattern = @"^S-\d-\d+-(\d+-){1,14}\d+$";
            return Regex.IsMatch(id, pattern);
        }

        public static AdHelper GetAdHelper(string domainName = null)
        {
            var currentAdHelper = new AdHelper();
            if (string.IsNullOrWhiteSpace(domainName) ||
                string.Compare(domainName, currentAdHelper.DnsName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return currentAdHelper;
            }
            var currentNetBiosName = currentAdHelper.GetAdNetbiosName();
            if (string.Compare(domainName, currentNetBiosName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return currentAdHelper;
            }
            if (domainName.Contains('.'))
            {
                var netbiosName = currentAdHelper.GetDomainNetbiosName(domainName);
                return string.IsNullOrWhiteSpace(netbiosName) ? null : new AdHelper(domainName);
            }
            var dnsName = currentAdHelper.GetDomainDnsName(domainName);
            return string.IsNullOrWhiteSpace(dnsName) ? null : new AdHelper(dnsName);
        }

        public string DnsName
        {
            get { return domainName; }
        }

       private IEnumerable<AdUser> FindAdUsers(string host, string name)
        {
            using (var directoryEntry = new DirectoryEntry(GetDirectoryEntryAddress(host)))
            {
                using (var directorySearcher = new DirectorySearcher(directoryEntry))
                {
                    directorySearcher.SearchRoot = directoryEntry;
                    directorySearcher.Filter = string.Format(LdapFilterUsersSearchForName, name);
                    directorySearcher.PropertiesToLoad.AddRange(GetAllParameters<AdUserParameters>());
                    var searchResult = directorySearcher.FindAll();
                    var adUsers = searchResult.Cast<SearchResult>()
                        .Select(GetAdUser)
                        .Where(u => u != null);
                    return adUsers;
                }
            }
        }

        private IEnumerable<AdTrustedDomain> GetTrustedDomains(string host)
        {
            using (var directoryEntry = new DirectoryEntry(GetDirectoryEntryAddress(host)))
            {
                using (var directorySearcher = new DirectorySearcher(directoryEntry))
                {
                    directorySearcher.SearchRoot = directoryEntry;
                    directorySearcher.Filter = LdapFilterTrustedDomains;
                    directorySearcher.PropertiesToLoad.AddRange(GetAllParameters<AdTrustedDomainParameters>());
                    var searchResults = directorySearcher.FindAll();
                    var trustedDomains = searchResults.Cast<SearchResult>()
                        .Select(GetAdTrustedDomain)
                        .Where(trustedDomain => trustedDomain != null);
                    return trustedDomains;

                }
            }
        }

        private string GetAdNetbiosName(string host)
        {

            using (var directoryEntry = new DirectoryEntry(GetConfigurationNamingContext(host, "cn=partitions")))
            {
                using (var directorySearcher = new DirectorySearcher(directoryEntry))
                {
                    directorySearcher.SearchRoot = directoryEntry;
                    directorySearcher.Filter = string.Format(
                        @"(&(nETBIOSName=*)(dnsRoot={0})(objectcategory=Crossref))", host);
                    directorySearcher.PropertiesToLoad.Add("netbiosname");
                    var searchResult = directorySearcher.FindOne();
                    if (searchResult == null || searchResult.Properties.Count == 0)
                    {
                        return null;
                    }
                    var netbiosName = GetProperty(searchResult.GetDirectoryEntry(), "netbiosname") as string;
                    return netbiosName;
                }
            }
        }
        private string GetDirectoryEntryAddress(string host)
        {
            return string.Format(@"LDAP://{0}", host);
        }

        private string GetConfigurationNamingContext(string host, string section = null)
        {
            var rootDse = new DirectoryEntry(GetDirectoryEntryAddress(host) + @"/RootDSE");
            var configurationNamingContext = rootDse.Properties["configurationNamingContext"][0].ToString();
            if (section == null)
                return GetDirectoryEntryAddress(configurationNamingContext);
            return GetDirectoryEntryAddress(string.Format("{0},{1}", section, configurationNamingContext));
        }
        private string[] GetAllParameters<T>() where T : struct, IConvertible
        {
            return !typeof(T).IsEnum ? null : Enum.GetNames(typeof(T));
        }

        private static object GetProperty<T>(DirectoryEntry directoryEntry, T adParam) where T : struct, IConvertible
        {
            return GetProperty(directoryEntry, adParam.ToString(CultureInfo.InvariantCulture));
        }

        private static object GetProperty(DirectoryEntry dr, string adParam)
        {
            var prop = dr.Properties[adParam];
            if (prop.Count > 0)
                return prop[0];
            return null;
        }

        private AdTrustedDomain GetAdTrustedDomain(SearchResult searchResult)
        {
            return GetAdTrustedDomain(searchResult.GetDirectoryEntry());
        }

        private AdTrustedDomain GetAdTrustedDomain(DirectoryEntry directoryEntry)
        {
            var dnsName = GetProperty(directoryEntry, AdTrustedDomainParameters.TrustPartner) as string;
            var netbiosName = GetProperty(directoryEntry, AdTrustedDomainParameters.FlatName) as string;
            if (dnsName == null || netbiosName == null)
                return null;
            return new AdTrustedDomain()
            {
                DnsName = dnsName.ToLower(),
                NetBiosName = netbiosName.ToUpper()
            };
        }

        private AdUser GetAdUser(SearchResult searchResult)
        {
            return GetAdUser(searchResult.GetDirectoryEntry());
        }

        private AdUser GetAdUser(DirectoryEntry directoryEntry)
        {
            var login = GetProperty(directoryEntry, AdUserParameters.SamAccountName) as string;
            var employeeId = GetProperty(directoryEntry, AdUserParameters.EmployeeId) as string;
            var email = GetProperty(directoryEntry, AdUserParameters.Mail) as string;
            var phone = GetProperty(directoryEntry, AdUserParameters.TelephoneNumber) as string;
            var organization = GetProperty(directoryEntry, AdUserParameters.Company) as string;
            var address = GetProperty(directoryEntry, AdUserParameters.StreetAddress) as string;
            var company = GetProperty(directoryEntry, AdUserParameters.Company) as string;
            var name = GetProperty(directoryEntry, AdUserParameters.Name) as string;
            var description = GetProperty(directoryEntry, AdUserParameters.Description) as string;
            var l = GetProperty(directoryEntry, AdUserParameters.L) as string;
            var physicalDeliveryOfficeName = GetProperty(directoryEntry, AdUserParameters.PhysicalDeliveryOfficeName) as string;
            var guid = new Guid(GetProperty(directoryEntry, AdUserParameters.ObjectGuid) as byte[]);
            var sid =
                new SecurityIdentifier(GetProperty(directoryEntry, AdUserParameters.ObjectSid) as byte[], 0).ToString();
            var principalName = GetProperty(directoryEntry, AdUserParameters.UserPrincipalName) as string;
            var domain = principalName.Split('@').Length == 2 ? principalName.Split('@')[1] : "";

            var employeeIdIn = (int?)null;
            int employeeId_;
            if (int.TryParse(employeeId, out employeeId_))
                employeeIdIn = employeeId_;

            return new AdUser()
            {
                Address = address,
                City = l,
                Company = company,
                Email = email,
                FullName = name,
                Id = guid,
                Login = login,
                Number = employeeIdIn,
                Organization = organization,
                Phone = phone,
                Position = description,
                Principal = principalName,
                Room = physicalDeliveryOfficeName,
                Sid = sid,
                Domain = domain
            };
        }

        private AdUser GetUserByNumber(string host, int number)
        {
            using (var directoryEntry = new DirectoryEntry(GetDirectoryEntryAddress(host)))
            {
                using (var directorySearcher = new DirectorySearcher(directoryEntry))
                {
                    directorySearcher.SearchRoot = directoryEntry;                         
                    directorySearcher.Filter = string.Format(LdapFilterUserForNumber, number);
                    directorySearcher.PropertiesToLoad.AddRange(GetAllParameters<AdUserParameters>());
                    var searchResult = directorySearcher.FindOne();
                    if (searchResult == null || searchResult.Properties.Count == 0)
                    {
                        return null;
                    }
                    var user = GetAdUser(searchResult);
                    return user;
                }
            }
        }

        private AdUser GetUserByName(string host, string name)
        {
            using (var directoryEntry = new DirectoryEntry(GetDirectoryEntryAddress(host)))
            {
                using (var directorySearcher = new DirectorySearcher(directoryEntry))
                {
                    directorySearcher.SearchRoot = directoryEntry;
                    directorySearcher.Filter = string.Format(LdapFilterUserForName, name);
                    directorySearcher.PropertiesToLoad.AddRange(GetAllParameters<AdUserParameters>());
                    var searchResult = directorySearcher.FindOne();
                    if (searchResult == null || searchResult.Properties.Count == 0)
                    {
                        return null;
                    }
                    var user = GetAdUser(searchResult);
                    return user;
                }
            }
        }

        private AdUser GetUserByLogin(string host, string login)
        {
            var fullLogin = login.Contains('@') ? login : string.Format("{0}@{1}", login, host);
            using (var directoryEntry = new DirectoryEntry(GetDirectoryEntryAddress(host)))
            {
                using (var directorySearcher = new DirectorySearcher(directoryEntry))
                {
                    directorySearcher.SearchRoot = directoryEntry;
                    directorySearcher.Filter = string.Format(LdapFilterUserForLogin, fullLogin);
                    directorySearcher.PropertiesToLoad.AddRange(GetAllParameters<AdUserParameters>());
                    var searchResult = directorySearcher.FindOne();
                    if (searchResult == null || searchResult.Properties.Count == 0)
                    {
                        return null;
                    }
                    var user = GetAdUser(searchResult);
                    return user;
                }
            }
        }

        private AdUser GetUserBySid(string host, string sid)
        {
            using (var directoryEntry = new DirectoryEntry(GetDirectoryEntryAddress(host)))
            {
                using (var directorySearcher = new DirectorySearcher(directoryEntry))
                {
                    directorySearcher.SearchRoot = directoryEntry;
                    directorySearcher.Filter = string.Format(LdapFilterUserForSid, sid);
                    directorySearcher.PropertiesToLoad.AddRange(GetAllParameters<AdUserParameters>());
                    var searchResult = directorySearcher.FindOne();
                    if (searchResult == null || searchResult.Properties.Count == 0)
                    {
                        return null;
                    }
                    var user = GetAdUser(searchResult);
                    return user;
                }
            }
        }

        private AdUser GetUserById(string host, Guid id)
        {
            var user = new DirectoryEntry(string.Format(@"LDAP://{0}/<GUID={1}>", host, id));
            return GetAdUser(user);
        }

        private string GetDomainDnsName(string host, string netbiosName)
        {
            var netbiosDomainName = GetAdNetbiosName(host);
            if (string.Compare(netbiosDomainName, netbiosName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return host;
            }
            using (var directoryEntry = new DirectoryEntry(GetDirectoryEntryAddress(host)))
            {
                using (var directorySearcher = new DirectorySearcher(directoryEntry))
                {
                    directorySearcher.SearchRoot = directoryEntry;
                    directorySearcher.Filter = string.Format(LdapFilterGetDnsName, netbiosName);
                    directorySearcher.PropertiesToLoad.AddRange(GetAllParameters<AdTrustedDomainParameters>());
                    var searchResult = directorySearcher.FindOne();
                    if (searchResult == null)
                    {
                        return null;
                    }
                    var dnsName = GetAdTrustedDomain(searchResult);
                    return dnsName != null ? dnsName.DnsName : null;

                }
            }
        }

        private string GetDomainNetbiosName(string host, string dnsName)
        {
            if (string.Compare(host, dnsName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return GetAdNetbiosName();
            }
            using (var directoryEntry = new DirectoryEntry(GetDirectoryEntryAddress(host)))
            {
                using (var directorySearcher = new DirectorySearcher(directoryEntry))
                {
                    directorySearcher.SearchRoot = directoryEntry;
                    directorySearcher.Filter = string.Format(LdapFilterGetNetbiosName, dnsName);
                    directorySearcher.PropertiesToLoad.AddRange(GetAllParameters<AdTrustedDomainParameters>());
                    var searchResult = directorySearcher.FindOne();
                    if (searchResult == null)
                    {
                        return null;
                    }
                    var netbiosName = GetAdTrustedDomain(searchResult);
                    return netbiosName != null ? netbiosName.NetBiosName : null;

                }
            }
        }

        /// <summary>
        /// Получение имени текущего домена
        /// </summary>
        /// <returns>Имя текущего домена</returns>
        public static string GetCurrentDomainName()
        {
            return Domain.GetCurrentDomain().ToString();
        }
        
        /// <summary>
        /// Получение пользователя по табельному номеру
        /// </summary>
        /// <param name="number">Табельный номер</param>
        /// <returns>Пользователя AD</returns>
        public AdUser GetUserByNumber(int number)
        {
            return GetUserByNumber(domainName, number);
        }

        /// <summary>
        /// Получение пользователя по ФИО
        /// </summary>
        /// <param name="name">ФИО</param>
        /// <returns>Пользователь AD</returns>
        public AdUser GetUserByName(string name)
        {
            return GetUserByName(domainName, name);
        }

        /// <summary>
        /// Получение пользователя по логину
        /// </summary>
        /// <param name="login">Логин</param>
        /// <returns>Пользователь AD</returns>
        public AdUser GetUserByLogin(string login)
        {
            return GetUserByLogin(domainName, login);
        }

        /// <summary>
        /// Получение пользователя по sid
        /// </summary>
        /// <param name="sid">Sid</param>
        /// <returns>Пользователь AD</returns>
        public AdUser GetUserBySid(string sid)
        {
            return GetUserBySid(domainName, sid);
        }

        /// <summary>
        /// Получение пользователя по уникальному идентификатору
        /// </summary>
        /// <param name="id">Уникальный идентфикатор</param>
        /// <returns>Пользователь AD</returns>
        public AdUser GetUserById(Guid id)
        {
            return GetUserById(domainName, id);
        }

        /// <summary>
        /// Поиск пользователя по ФИО или учётной записи
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<AdUser> FindAdUsers(string name)
        {
            return FindAdUsers(domainName, name);
        }

        /// <summary>
        /// Получение Netbios name
        /// </summary>
        /// <returns>Netbios name</returns>
        public string GetAdNetbiosName()
        {
            return GetAdNetbiosName(domainName);
        }

        /// <summary>
        /// Получение списка доверенных доменов
        /// </summary>
        /// <returns>Список доверенных доменов</returns>
        public IEnumerable<AdTrustedDomain> GetTrustedDomains()
        {
            return GetTrustedDomains(domainName);
        }

        /// <summary>
        /// Получение имени DNS домена по Netbios
        /// </summary>
        /// <param name="netbiosName">Имя NetBios</param>
        /// <returns>Имя DNS</returns>
        public string GetDomainDnsName(string netbiosName)
        {
            return GetDomainDnsName(domainName, netbiosName);
        }

        /// <summary>
        /// Получение имени netbios по имени dns
        /// </summary>
        /// <param name="dnsName">Имя dns</param>
        /// <returns>Имя netbios</returns>
        public string GetDomainNetbiosName(string dnsName)
        {
            return GetDomainNetbiosName(domainName, dnsName);
        }

        

    }
}
