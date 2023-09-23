using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Services_Layer.DTOS
{
    public record TokenResults
    {
        public string access_token { get; set; }
        public string iss { get; set; }
        public string sub { get; set; }
        public string acn { get; set; }
        public string aci { get; set; }
        public string Dns { get; set; }
    }
}
