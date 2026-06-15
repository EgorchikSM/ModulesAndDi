using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesAndDi.Services
{
    public class SystemClock : IClock
    {
        public DateTime Now => DateTime.Now;
    }
}
