﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Infrastructure.Common
{
    public interface IUpdatedAt
    {
        DateTime UpdatedAt { get; set; }
    }
}
