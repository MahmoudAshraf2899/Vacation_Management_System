using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Services_Layer.DTOS
{
    public record DtoAllEmployeesCreditVacation
    {
        public int id { get; init; }
        public int TotalCreditDays { get; init; }
        public string employeeName { get; init; }         
        public string departmentName { get; init; }
        
    }
}
