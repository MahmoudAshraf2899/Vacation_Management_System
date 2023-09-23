using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Services_Layer.DTOS
{
    public record DtoAverageDurationResponse
    {
        public int perMonth { get; set; }
        public int perYear { get; set; }
    }
}
