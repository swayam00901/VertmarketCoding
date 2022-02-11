using System;
using System.Collections.Generic;
using System.Text;

namespace MagazineStore
{
    public class MagSubscriptions
    {
        public string? id { get; set; }
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public List<int>? magazineIds { get; set; }
    }

}
