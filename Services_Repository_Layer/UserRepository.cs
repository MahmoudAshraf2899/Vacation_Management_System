using Context_Layer.Models;
using Data_Services_Layer.DTOS;
using IServices_Repository_Layer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services_Repository_Layer
{
    public class UserRepository : GenericRepository<VacationManagementDBContext, User>, IUserRepository
    {
        public async Task<List<DtoGetAllEmployeesForGrid>> GetAllEmpsForGrid()
        {
            var result = await (from q in Context.Users.AsNoTracking()
                                select new DtoGetAllEmployeesForGrid 
                                {
                                    id=q.Id,
                                    empName=q.UserName,
                                    email=q.Email,
                                    isHr=q.IsHrAdmin == true ? "Yes" :"No",
                                    department=q.Department.Name
                                }).OrderByDescending(c=>c.id).ToListAsync();
            return result;  
        }
    }
}
