﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using August2008.Model;

namespace August2008.Models
{
    public class HeroCriteria
    {
        public IEnumerable<Hero> Result { get; set; }

        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public int LanguageId { get; set; }
        public int TotalCount { get; set; }
    }
}