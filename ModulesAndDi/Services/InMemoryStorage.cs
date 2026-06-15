using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesAndDi.Services
{
    public class InMemoryStorage : IStorage
    {
        private readonly List<string> items = new();
        public void Add(string item) => items.Add(item);
        public IReadOnlyCollection<string> GetAll() => items.AsReadOnly();
    }
}
