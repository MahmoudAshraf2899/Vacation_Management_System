using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Services_Layer.DTOS
{
    public record DtoEmpVacationTransactionById
    {
        public int id { get; init; }
        public int? vacationTypeId { get; init; }
        public int? empId { get; init; }         
        public string description { get; init; }
        public string hrManagerStatusName { get; init; }
        public DateTime? startDate { get; init; }
        public DateTime? finishDate { get; init; }
    }
}
