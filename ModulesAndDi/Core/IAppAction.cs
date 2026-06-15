using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesAndDi.Core
{
    public interface IAppAction
    {
        string Title { get; }
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
