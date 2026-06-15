using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ModulesAndDi.Core
{
    public interface IAppModule
    {
        string Name { get; }
        IReadOnlyCollection<string> Requires { get; }
        void RegisterServices(IServiceCollection services);
        Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }


}
