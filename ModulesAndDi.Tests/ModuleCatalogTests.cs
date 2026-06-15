using ModulesAndDi.Core;
using Microsoft.Extensions.DependencyInjection;

namespace ModulesAndDi.Tests
{
    public class ModuleCatalogTests
    {
        private class FakeModule : IAppModule
        {
            public FakeModule(string name, IReadOnlyCollection<string> requires)
            {
                Name = name; Requires = requires;
            }
            public string Name { get; }
            public IReadOnlyCollection<string> Requires { get; }
            public Action<IServiceProvider>? OnInit { get; init; }
            public void RegisterServices(IServiceCollection services) { }
            public Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
            {
                OnInit?.Invoke(serviceProvider);
                return Task.CompletedTask;
            }
        }

        [Fact]
        public void Order_IsCorrect_MultipleDependencies()
        {
            var a = new FakeModule("A", Array.Empty<string>());
            var b = new FakeModule("B", new[] { "A" });
            var c = new FakeModule("C", new[] { "B" });
            var all = new Dictionary<string, IAppModule>
            {
                [a.Name] = a,
                [b.Name] = b,
                [c.Name] = c
            };
            var order = ModuleCatalog.BuildExecutionOrder(all, new[] { "A", "B", "C" });
            Assert.Equal(new[] { "A", "B", "C" }, order.Select(m => m.Name).ToArray());
        }

        [Fact]
        public void MissingModule_ThrowsModuleLoadException()
        {
            var a = new FakeModule("A", Array.Empty<string>());
            var all = new Dictionary<string, IAppModule> { [a.Name] = a };
            var ex = Assert.Throws<ModuleLoadException>(
                () => ModuleCatalog.BuildExecutionOrder(all, new[] { "A", "B" }));
            Assert.Contains("Модуль не найден", ex.Message);
        }

        [Fact]
        public void CyclicDependency_ThrowsModuleLoadException()
        {
            var a = new FakeModule("A", new[] { "B" });
            var b = new FakeModule("B", new[] { "A" });
            var all = new Dictionary<string, IAppModule>
            {
                [a.Name] = a,
                [b.Name] = b
            };
            var ex = Assert.Throws<ModuleLoadException>(
                () => ModuleCatalog.BuildExecutionOrder(all, new[] { "A", "B" }));
            Assert.Contains("циклическая", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Dependencies_AreInjected_ByContainer()
        {
            var services = new ServiceCollection();
            services.AddSingleton<MarkerService>();
            var provider = services.BuildServiceProvider();
            var module = new FakeModule("M", Array.Empty<string>())
            {
                OnInit = sp => {
                    var svc = sp.GetService<MarkerService>();
                    Assert.NotNull(svc);
                }
            };
            await module.InitializeAsync(provider, CancellationToken.None);
        }
        private class MarkerService { }
    }

}
