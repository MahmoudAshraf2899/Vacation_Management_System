using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Services_Layer.DTOS
{
    public record DtoAddEmployeeCreditByHrResponse
    {
        public int empId { get; set; }
        public int? vacationTypeId { get; set; }
        public int? newCredit { get; set; }
    }
}

