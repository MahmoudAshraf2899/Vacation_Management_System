using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Services_Layer.DTOS
{
    public record DtoAllNewEmployeesForDropDown
    {
        public int id { get; init; }
        public string empName { get; init; }
    }
}
