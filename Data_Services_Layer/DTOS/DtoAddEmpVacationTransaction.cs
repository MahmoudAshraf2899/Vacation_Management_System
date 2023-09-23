using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Services_Layer.DTOS
{
    public record DtoAddEmpVacationTransaction
    {
        public int empId { get; set; }
        public int vacationTypeId { get; set; }        
        public DateTime? startDate { get; set; }
        public DateTime? finishDate { get; set; }
        public string description { get; set; }
    }
}
