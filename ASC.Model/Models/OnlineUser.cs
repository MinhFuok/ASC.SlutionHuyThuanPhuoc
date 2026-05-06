using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASC.Model.BaseTypes;

namespace ASC.Model.Models
{
    public class OnlineUser : BaseEntity
    {
        public OnlineUser()
        {
        }

        public OnlineUser(string name)
        {
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = name;
        }
    }
}