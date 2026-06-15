using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesAndDi.Services
{
    public interface IClock
    {
        DateTime Now { get; }
    }
}
