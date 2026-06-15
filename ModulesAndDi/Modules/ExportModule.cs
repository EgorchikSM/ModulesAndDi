using ModulesAndDi.Core;
using Microsoft.Extensions.DependencyInjection;
using ModulesAndDi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ModulesAndDi.Modules
{
    public class ExportModule : IAppModule
    {
        public string Name => "Export";
        public IReadOnlyCollection<string> Requires => new[] { "Core", "Validation" };
        public void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IAppAction, ExportAction>();
        }
        public Task InitializeAsync(IServiceProvider sp, CancellationToken ct)
        {
            Console.WriteLine("ExportModule: инициализирован");
            return Task.CompletedTask;
        }

        private class ExportAction : IAppAction
        {
            private readonly IStorage storage;
            public ExportAction(IStorage storage) => this.storage = storage;
            public string Title => "Экспорт";
            public async Task ExecuteAsync(CancellationToken ct)
            {
                var data = storage.GetAll();
                string path = Path.Combine(AppContext.BaseDirectory, "export.txt");
                await File.WriteAllLinesAsync(path, data, ct);
                Console.WriteLine($"ExportAction: данные экспортированы в {path}");
            }
        }
    }

}
