using System;
using System.Collections.Generic;
using System.Text;

namespace MagazineStore
{
    public class MagazineDat
    {
        public string? TotalTime { get; set; }
        public bool AnswerCorrect { get; set; }
        public List<string>? ShouldBe { get; set; }
    }
}
