using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace DPC_core
{
    [ElasticsearchType(Name = "student")]
    public class Student
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime DateTime { get; set; }
    }
}
