using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Services_Layer.DTOS
{
    public record DtoGetAllVacationRequestsForHr
    {
        public int id { get; init; }
        public string empName { get; init; }
        public string departmentName { get; init; }
        public string description { get; init; }
        public DateTime? startDate { get; init; }
        public DateTime? finishDate { get; init; }
        public string vacationType { get; init; }
    }
}
