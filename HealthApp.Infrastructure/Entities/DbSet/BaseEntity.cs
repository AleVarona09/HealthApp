﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthApp.Infrastructure.Entities.DbSet
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Status { get; set; } = 1;
        public DateTime AddedDate { get; set; } = DateTime.Now;
        public DateTime UpdateDate { get; set; }


    }
}
