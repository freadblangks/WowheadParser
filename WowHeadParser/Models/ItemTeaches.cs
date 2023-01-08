using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowHeadParser.Models
{
    public class ItemTeaches
    {
        public int cat { get; set; }
        public int id { get; set; }
        public int level { get; set; }
        public string name { get; set; }
        public int nskillup { get; set; }
        public int schools { get; set; }
        public int[] skill { get; set; }
    }

}
