using System;
using System.Collections.Generic;
using System.Text;

namespace MagazineStore
{
    public class MagSubscribeResp
    {

        public List<MagSubscriptions>? Data { get; set; }
        public string? Success { get; set; }
        public string? Token { get; set; }
    }
}
