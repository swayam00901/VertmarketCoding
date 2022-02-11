using System;
using System.Collections.Generic;
using System.Text;

namespace MagazineStore
{
    public class MagazineResp
    {
        public MagazineDat? Data { get; set; }
        public bool? Success { get; set; }
        public string? Token { get; set; }
        public string? Message { get; set; }
    }
}
