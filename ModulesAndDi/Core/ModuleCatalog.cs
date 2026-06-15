using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Loader;
using ModulesAndDi.Core;

public class ModuleLoadException : Exception
{
    public ModuleLoadException(string message) : base(message) { }
}

public static class ModuleCatalog
{
    // Обнаружение модулей через рефлексию в текущей сборке
    public static IReadOnlyDictionary<string, IAppModule> DiscoverFromAssembly(Assembly assembly)
    {
        var result = new Dictionary<string, IAppModule>(StringComparer.OrdinalIgnoreCase);
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsClass && !type.IsAbstract && typeof(IAppModule).IsAssignableFrom(type))
            {
                var module = (IAppModule)Activator.CreateInstance(type)!;
                result[module.Name] = module;
            }
        }
        return result;
    }

    // Обнаружение модулей, загружая все сборки из папки "modules"
    public static IReadOnlyDictionary<string, IAppModule> DiscoverFromModulesFolder(string folder)
    {
        if (!Directory.Exists(folder))
            return new Dictionary<string, IAppModule>();

        var result = new Dictionary<string, IAppModule>(StringComparer.OrdinalIgnoreCase);
        foreach (var dll in Directory.GetFiles(folder, "*.dll"))
        {
            // Настраиваем отдельный контекст для загрузки плагина
            var loadContext = new PluginLoadContext(dll);
            var name = Path.GetFileNameWithoutExtension(dll);
            try
            {
                var assembly = loadContext.LoadFromAssemblyName(new AssemblyName(name));
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && !type.IsAbstract && typeof(IAppModule).IsAssignableFrom(type))
                    {
                        var module = (IAppModule)Activator.CreateInstance(type)!;
                        result[module.Name] = module;
                    }
                }
            }
            catch (Exception ex)
            {
                // Не указано, как обрабатывать ошибки загрузки модуля – просто пропускаем
                Console.WriteLine($"Не удалось загрузить модуль из {dll}: {ex.Message}");
            }
        }
        return result;
    }

    // Построение порядка запуска (алгоритм Канна, топологическая сортировка)
    public static IReadOnlyList<IAppModule> BuildExecutionOrder(
        IReadOnlyDictionary<string, IAppModule> allModules,
        IEnumerable<string> enabledModuleNames)
    {
        // Фильтруем только включённые по конфигу модули
        var enabled = new Dictionary<string, IAppModule>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in enabledModuleNames)
        {
            if (!allModules.TryGetValue(name, out var module))
                throw new ModuleLoadException($"Модуль не найден, имя модуля {name}");
            enabled[name] = module;
        }
        // Проверяем, что все зависимости присутствуют
        foreach (var module in enabled.Values)
        {
            foreach (var dep in module.Requires)
            {
                if (!enabled.ContainsKey(dep))
                    throw new ModuleLoadException(
                        $"Не хватает модуля для зависимости, модуль {module.Name} требует {dep}");
            }
        }
        // Строим граф зависимостей
        var indegree = enabled.Values.ToDictionary(m => m.Name, m => 0);
        var graph = enabled.Values.ToDictionary(m => m.Name, m => new List<string>());
        foreach (var module in enabled.Values)
        {
            foreach (var dep in module.Requires)
            {
                graph[dep].Add(module.Name);
                indegree[module.Name]++;
            }
        }
        // Алгоритм Канна
        var queue = new Queue<string>(indegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));
        var ordered = new List<IAppModule>();
        while (queue.Count > 0)
        {
            var name = queue.Dequeue();
            ordered.Add(enabled[name]);
            foreach (var next in graph[name])
            {
                indegree[next]--;
                if (indegree[next] == 0)
                    queue.Enqueue(next);
            }
        }
        // Проверка на циклы
        if (ordered.Count != enabled.Count)
        {
            var cycle = indegree.Where(kv => kv.Value > 0).Select(kv => kv.Key);
            throw new ModuleLoadException(
                $"Обнаружена циклическая зависимость, проблемные модули {string.Join(", ", cycle)}");
        }
        return ordered;
    }
}

public class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver resolver;
    public PluginLoadContext(string pluginPath)
        : base(isCollectible: false)
    {
        resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        string? path = resolver.ResolveAssemblyToPath(assemblyName);
        if (path != null)
            return LoadFromAssemblyPath(path);
        return null!;
    }
}
