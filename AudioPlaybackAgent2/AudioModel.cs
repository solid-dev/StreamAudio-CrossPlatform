using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioPlaybackAgent2
{
    public class AudioModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string PathMp3 { get; set; }
        public string Description { get; set; }
        public string Singer { get; set; }
    }
}
