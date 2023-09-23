using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Services_Layer.DTOS
{
    public class DtoRegister
    {
        public string userName { get; set; }
        public string lastName { get; set; }
        public string firstName { get; set; }
        public string password { get; set; }
        public string confirmPassword { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public bool? isHrAdmin { get; set; }
        public int? departmentId { get; set; }

    }
}
