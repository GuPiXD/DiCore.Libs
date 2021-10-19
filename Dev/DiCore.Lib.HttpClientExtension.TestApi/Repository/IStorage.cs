using System;
using System.Collections.Generic;

namespace DiCore.Lib.RestClient.TestCore.Api.Repository
{
    public interface IStorage
    {
        void InitializePersons(Dictionary<Guid, Person> data);
        IEnumerable<PersonItem> GetPersons();
        PersonItem GetPerson(Guid id);
        Guid InsertPerson(Person person);
        bool UpdatePerson(Guid id, Person person);
        bool DeletePerson(Guid id);
        FileDataItem GetFileData(Guid id);
        Guid InsertFileData(FileData data);
        bool UpdateFileData(Guid id, FileData data);
        bool DeleteFileData(Guid id);
    }
}