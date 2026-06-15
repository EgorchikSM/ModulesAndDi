using ModulesAndDi.Core;
using Microsoft.Extensions.DependencyInjection;
using ModulesAndDi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesAndDi.Modules
{
    public class CoreModule : IAppModule
    {
        public string Name => "Core";
        public IReadOnlyCollection<string> Requires => Array.Empty<string>();
        public void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IClock, SystemClock>();
            services.AddSingleton<IStorage, InMemoryStorage>();
        }
        public Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken ct)
        {
            Console.WriteLine("CoreModule: инициализирован");
            return Task.CompletedTask;
        }
    }

}
