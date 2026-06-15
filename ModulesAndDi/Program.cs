using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModulesAndDi.Core;
using System.Reflection;

class Program
{
    static async Task Main(string[] args)
    {
        // Настройка Generic Host для упрощения конфигурации и DI
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(cfg =>
            {
                cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
            })
            .ConfigureServices((ctx, services) =>
            {
                // Получаем список модулей из конфигурации
                var moduleNames = ctx.Configuration.GetSection("Modules").Get<string[]>()
                                  ?? Array.Empty<string>();

                // Обнаруживаем модули
                var discovered = new Dictionary<string, IAppModule>(
                    ModuleCatalog.DiscoverFromAssembly(Assembly.GetExecutingAssembly())
                );                // Можно также загружать из папки "modules":
                var folderMods = ModuleCatalog.DiscoverFromModulesFolder("modules");
                foreach (var kv in folderMods) discovered[kv.Key] = kv.Value;

                // Строим порядок запуска
                var ordered = ModuleCatalog.BuildExecutionOrder(discovered, moduleNames);

                // Регистрируем сервисы каждого модуля
                foreach (var module in ordered)
                {
                    module.RegisterServices(services);
                }
                // После BuildServiceProvider: инициализация модулей
                services.AddHostedService(sp => new ModulesInitializer(ordered, sp));
                // Регистрируем также все IAppAction, например через Scan или вручную
            })
            .Build();

        await host.RunAsync();
    }
}

// Хост-сервис для инициализации модулей после создания ServiceProvider
public class ModulesInitializer : IHostedService
{
    private readonly IEnumerable<IAppModule> modules;
    private readonly IServiceProvider provider;
    public ModulesInitializer(IEnumerable<IAppModule> modules, IServiceProvider provider)
    {
        this.modules = modules; this.provider = provider;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var module in modules)
            await module.InitializeAsync(provider, cancellationToken);

        // После инициализации запускаем все действия модулей
        var actions = provider.GetServices<IAppAction>();
        Console.WriteLine("Запуск действий модулей:");
        foreach (var action in actions)
        {
            Console.WriteLine($"- {action.Title}");
            await action.ExecuteAsync(cancellationToken);
        }
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
