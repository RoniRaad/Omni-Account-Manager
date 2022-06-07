using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Core.Models
{
    public class RankedGraph
    {
        public string Title { get; set; }
        public List<RankedGraphData> Data { get; set; } = new();
    }
}
