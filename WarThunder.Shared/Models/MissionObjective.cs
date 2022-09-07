using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarThunder.Shared.Models
{
    public class MissionObjective
    {
        public bool Primary { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
