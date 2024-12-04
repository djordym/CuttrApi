using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Infrastructure.Common
{
    public interface ICreatedAt
    {
        DateTime CreatedAt { get; set; }
    }
}
