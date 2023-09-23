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
    public class EmpVacationCreditRepository : GenericRepository<VacationManagementDBContext, EmpVacationCredit>, IEmpVacationCreditRepository
    {
        public async Task<DtoAddEmployeeCreditByHrResponse> AddEmpVacationCredit(DtoAddEmployeeCreditByHr body)
        {
            if (body is not null)
            {
                DtoAddEmployeeCreditByHrResponse response = new DtoAddEmployeeCreditByHrResponse();

                //#1-Check If Employee Exist Before
                var employeeExist = await Context.EmpVacationCredits.AsNoTracking().Where(c => c.EmpId == body.empId).AnyAsync();
                if (employeeExist)
                {
                    return null;
                }
                else
                {
                    //#2- Get Vacation Type
                    var vacationType = await Context.VacationTypes.AsNoTracking().Where(c => c.Id == body.vacationTypeId).FirstOrDefaultAsync();
                    if (vacationType is not null)
                    {
                        //#3- Add User With New Vacation Credit
                        EmpVacationCredit empVacationCredit = new EmpVacationCredit()
                        {
                            CurrentCredit = body.credit,
                            EmpId = body.empId,
                            VacationTypeId = body.vacationTypeId
                        };
                        await AddAsync(empVacationCredit);
                        await SaveAsync();

                        response.empId = empVacationCredit.Id;
                        response.newCredit = empVacationCredit.CurrentCredit;
                        response.vacationTypeId = empVacationCredit.VacationTypeId;
                        return response;
                    }
                    return null;
                }
            }
            return null;

        }


        public async Task<DtoUpdateEmployeeVacationCreditByHr> UpdateEmpVacationCredit(DtoUpdateEmployeeVacationCreditByHr body)
        {
            if (body is not null)
            {
                //#-1 Get Employee Credit Obj
                var empCredit = await FindBy(c => c.Id == body.id).FirstOrDefaultAsync();
                if (empCredit is not null)
                {
                    empCredit.CurrentCredit = body.newCredit;
                    await UpdateAsync(empCredit);
                    await SaveAsync();

                    //#-2 Assign New Credit to object to use in client side..
                    body.newCredit = empCredit.CurrentCredit;
                    return body;
                }
                return null;

            }
            return null;
        }

        public async Task<List<DtoAllEmployeesCreditVacation>> GetAllEmployeesCreditVacation()
        {
            var result = await (from q in Context.EmpVacationCredits.AsNoTracking()
                                select new DtoAllEmployeesCreditVacation
                                {
                                    id = q.Id,
                                    employeeName = q.Emp.UserName,
                                    departmentName = q.Emp.Department.Name
                                }).OrderByDescending(c => c.id).ToListAsync();
            return result;
        }

        public async Task<List<DtoAllNewEmployeesForDropDown>> GetAllNewEmployeesForDropDown()
        {
            var listOfExistEmps = await Context.EmpVacationCredits.AsNoTracking().Select(c => c.EmpId).ToListAsync();
            var result = await (from q in Context.Users.AsNoTracking()
                                where !listOfExistEmps.Contains(q.Id)
                                select new DtoAllNewEmployeesForDropDown
                                {
                                    id = q.Id,
                                    empName = q.UserName
                                }).OrderByDescending(c => c.id).ToListAsync();
            return result;
        }
    }
}
