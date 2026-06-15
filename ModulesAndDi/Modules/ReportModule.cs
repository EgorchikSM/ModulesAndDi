using ModulesAndDi.Core;
using ModulesAndDi.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesAndDi.Modules
{
    public class ReportModule : IAppModule
    {
        public string Name => "Report";
        public IReadOnlyCollection<string> Requires => new[] { "Core", "Export" };
        public void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IAppAction, ReportAction>();
        }
        public Task InitializeAsync(IServiceProvider sp, CancellationToken ct)
        {
            Console.WriteLine("ReportModule: инициализирован");
            return Task.CompletedTask;
        }

        private class ReportAction : IAppAction
        {
            private readonly IClock clock;
            private readonly IStorage storage;
            public ReportAction(IClock clock, IStorage storage)
            {
                this.clock = clock; this.storage = storage;
            }
            public string Title => "Отчет";
            public Task ExecuteAsync(CancellationToken ct)
            {
                Console.WriteLine($"ReportAction: {clock.Now}, записей {storage.GetAll().Count}");
                return Task.CompletedTask;
            }
        }
    }

}
