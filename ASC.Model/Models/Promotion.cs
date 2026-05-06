using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASC.Model.BaseTypes;

namespace ASC.Model.Models
{
    public class Promotion : BaseEntity
    {
        public Promotion()
        {
        }

        public Promotion(string type)
        {
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = type;
        }

        public string Header { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}