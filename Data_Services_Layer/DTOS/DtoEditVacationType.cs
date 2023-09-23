using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Services_Layer.DTOS
{
    public record DtoEditVacationType
    {
        public int id { get; set; }
        public string name { get; set; }    
    }
}
