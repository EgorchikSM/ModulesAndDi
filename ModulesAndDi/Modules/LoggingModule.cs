using ModulesAndDi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ModulesAndDi.Modules
{
    public class LoggingModule : IAppModule
    {
        public string Name => "Logging";
        public IReadOnlyCollection<string> Requires => new[] { "Core" };
        public void RegisterServices(IServiceCollection services)
        {
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<IAppAction, LoggingAction>();
        }
        public Task InitializeAsync(IServiceProvider sp, CancellationToken ct)
        {
            Console.WriteLine("LoggingModule: инициализирован");
            return Task.CompletedTask;
        }

        private class LoggingAction : IAppAction
        {
            private readonly ILogger<LoggingAction> logger;
            public LoggingAction(ILogger<LoggingAction> logger) => this.logger = logger;
            public string Title => "Логирование";
            public Task ExecuteAsync(CancellationToken ct)
            {
                logger.LogInformation("Сообщение из LoggingModule");
                return Task.CompletedTask;
            }
        }
    }

}
