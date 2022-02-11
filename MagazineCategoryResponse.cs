using System;
using System.Collections.Generic;
using System.Text;

namespace MagazineStore
{
    public class MagazineCategoryResponse
    {
        public List<Magazine>? data { get; set; }
        public bool Success { get; set; }
        public string? Token { get; set; }
    }

}
