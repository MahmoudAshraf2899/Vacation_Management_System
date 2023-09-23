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
    public class DepartmentRepository : GenericRepository<VacationManagementDBContext, Department>, IDepartmentRepository
    {
        public async Task<List<DtoGetAllDepartments>> GetAllDepartments()
        {
            var result = await (from q in Context.Departments.AsNoTracking()
                                let employees = Context.Users.AsNoTracking()
                                                             .Where(c => c.DepartmentId == q.Id).Count()

                                let numberOfVacations = Context.EmpVacationsTransactions.AsNoTracking()
                                                        .Where(c => c.HrApprovalStatus == true && c.Emp.DepartmentId == q.Id)
                                                        .Count()
                                select new DtoGetAllDepartments
                                {
                                    id = q.Id,
                                    name = q.Name,
                                    noEmployees = employees,
                                    numberOfVacations= numberOfVacations
                                }).OrderByDescending(c => c.id).ToListAsync();
            return result;
        }
    }
}
