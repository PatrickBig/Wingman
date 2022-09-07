using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarThunder.Shared.Models
{
    public class Mission
    {
        public MissionObjective[] Objectives { get; set; } = Array.Empty<MissionObjective>();

        public string Status { get; set; } = string.Empty;
    }
}
