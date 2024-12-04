﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Contracts.Inputs
{
    public class UserPreferencesRequest
    {
        public double SearchRadius { get; set; }
        public List<string> PreferredCategories { get; set; }
    }
}
