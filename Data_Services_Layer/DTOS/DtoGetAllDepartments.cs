using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Services_Layer.DTOS
{
    public record DtoGetAllDepartments
    {
        public int id { get; init; }
        public int noEmployees { get; init; }
        public int numberOfVacations { get; init; }
        public string name { get; init; }
    }
}
