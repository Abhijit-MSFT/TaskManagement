using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagement.Model
{
    public class PageLoadData
    {
        public string NewTaskId { get; set; }
        public List<string> ListofTaskIDs { get; set; }
        public List<string> TeamMembers { get; set; }
    }
}
