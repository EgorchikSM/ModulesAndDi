using ModulesAndDi.Core;
using ModulesAndDi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ModulesAndDi.Modules
{
    public class ValidationModule : IAppModule
    {
        public string Name => "Validation";
        public IReadOnlyCollection<string> Requires => new[] { "Core" };
        public void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IAppAction, ValidationAction>();
        }
        public Task InitializeAsync(IServiceProvider sp, CancellationToken ct)
        {
            Console.WriteLine("ValidationModule: инициализирован");
            return Task.CompletedTask;
        }

        private class ValidationAction : IAppAction
        {
            private readonly IStorage storage;
            public ValidationAction(IStorage storage) => this.storage = storage;
            public string Title => "Валидация";
            public Task ExecuteAsync(CancellationToken ct)
            {
                string value = "testing";
                if (value.Length < 5) throw new Exception("Слишком короткая строка");
                storage.Add(value);
                Console.WriteLine($"ValidationAction: добавлено '{value}' в хранилище");
                return Task.CompletedTask;
            }
        }
    }

}
