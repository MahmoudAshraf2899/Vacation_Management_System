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
    public class Emp_Vacations_Transactions_Repository : GenericRepository<VacationManagementDBContext, EmpVacationsTransaction>, IEmp_Vacations_Transactions_Repository
    {


        public async Task<Tuple<string, EmpVacationsTransaction>> AddVacationTransForEmp(DtoAddEmpVacationTransaction body)
        {
            var empVacationsTransaction = new EmpVacationsTransaction();
            if (body is not null)
            {
                //#-1 Get Employee Credit depend on his request vacation type
                var empVacationTypeCredit = await Context.EmpVacationCredits.AsNoTracking()
                                                                      .Where(c => c.EmpId == body.empId && c.VacationTypeId == body.vacationTypeId)
                                                                      .FirstOrDefaultAsync();
                if (empVacationTypeCredit is not null)
                {
                    //#-2 Check If Employee Credit is enough to take vacation on this period
                    var vacationRequestDuration = Math.Abs((body.finishDate.Value - body.startDate.Value).TotalDays);

                    if (vacationRequestDuration > empVacationTypeCredit.CurrentCredit)
                    {
                        return Tuple.Create("Sorry,your credit isn't enough to take vacation ", empVacationsTransaction);
                    }
                    else //#-3 Send Vacation Transaction Request To Hr
                    {
                        empVacationsTransaction.StartDate = body.startDate;
                        empVacationsTransaction.FinishDate = body.finishDate;
                        empVacationsTransaction.VacationTypeId = body.vacationTypeId;
                        empVacationsTransaction.EmpId = body.empId;
                        empVacationsTransaction.Description = body.description;
                        empVacationsTransaction.CreatedAt = DateTime.Now.Date;

                        await AddAsync(empVacationsTransaction);
                        await SaveAsync();

                        return Tuple.Create("Operation Completed Successfully", empVacationsTransaction);

                    }
                }
                else
                {
                    return Tuple.Create("Something went wrong !!", empVacationsTransaction);
                }
            }
            return Tuple.Create("Something went wrong !!", empVacationsTransaction);

        }

        public async Task<Tuple<string, EmpVacationsTransaction>> EditVacationTransForEmp(DtoEditEmpVacationTransaction body)
        {
            var empVacationsTransaction = new EmpVacationsTransaction();
            if (body is not null)
            {
                //#-1 Get Vacation Transaction object
                var vacationTransObject = await FindBy(c => c.Id == body.id).FirstOrDefaultAsync();
                //#-2 Check If this transaction is rejected or approved
                if (vacationTransObject is not null)
                {
                    if (vacationTransObject.HrApprovalStatus != null)
                    {
                        return Tuple.Create("Sorry You Cannot Edit On This Document", empVacationsTransaction);

                    }

                    //#-3 Get Employee Credit depend on his request vacation type
                    var empVacationTypeCredit = await Context.EmpVacationCredits.AsNoTracking()
                                                                          .Where(c => c.EmpId == body.empId && c.VacationTypeId == body.vacationTypeId)
                                                                          .FirstOrDefaultAsync();
                    if (empVacationTypeCredit is not null)
                    {
                        //#-4 Check If Employee Credit is enough to take vacation on this period
                        var vacationRequestDuration = Math.Abs((body.finishDate.Value - body.startDate.Value).TotalDays);

                        if (vacationRequestDuration > empVacationTypeCredit.CurrentCredit)
                        {
                            return Tuple.Create("Sorry,your credit isn't enough to take vacation ", empVacationsTransaction);
                        }
                        else //#-5 Update Vacation Transaction Request 
                        {
                            vacationTransObject.StartDate = body.startDate;
                            vacationTransObject.FinishDate = body.finishDate;
                            vacationTransObject.Description = body.description;
                            vacationTransObject.VacationTypeId = body.vacationTypeId;

                            await UpdateAsync(vacationTransObject);
                            await SaveAsync();
                            return Tuple.Create("Operation Completed Successfully", empVacationsTransaction);

                        }
                    }
                    else
                    {
                        return Tuple.Create("Something went wrong !!", empVacationsTransaction);
                    }
                }
                return Tuple.Create("Something went wrong !!", empVacationsTransaction);

            }
            return Tuple.Create("Something went wrong !!", empVacationsTransaction);

        }

        public async Task<List<DtoGetAllVacationsTransactionForEmp>> GetAllVacationTransForEmployee(int empId)
        {
            var result = await (from q in Context.EmpVacationsTransactions.AsNoTracking()
                                where q.EmpId == empId
                                select new DtoGetAllVacationsTransactionForEmp
                                {
                                    id = q.Id,
                                    createdAt = q.CreatedAt,
                                    description = q.Description,
                                    empName = q.Emp.UserName,
                                    finishDate = q.FinishDate,
                                    startDate = q.StartDate,
                                    hrManagerStatus = q.HrApprovalStatus == null ? "Pending" :
                                                     q.HrApprovalStatus == false ? "Rejected" : "Approved"

                                }).OrderByDescending(c => c.id).ToListAsync();
            return result;
        }

        public async Task<DtoEmpVacationTransactionById?> GetVacationTransForEmpById(int vacationTransId)
        {
            var result = await (from q in Context.EmpVacationsTransactions.AsNoTracking()
                                where q.Id == vacationTransId
                                select new DtoEmpVacationTransactionById
                                {
                                    id = q.Id,
                                    description = q.Description,
                                    finishDate = q.FinishDate,
                                    startDate = q.StartDate,
                                    hrManagerStatusName = q.HrApprovalStatus == null ? "Pending" :
                                                     q.HrApprovalStatus == false ? "Rejected" : "Approved",
                                    empId = q.EmpId,
                                    vacationTypeId = q.VacationTypeId,
                                }).FirstOrDefaultAsync();
            return result;
        }

        #region HR-Dashboard
        public async Task<int?> GetTotalNumberVacationsPerDepartment(int departmentId)
        {
            var result = await (from q in Context.EmpVacationsTransactions.AsNoTracking()
                                where q.HrApprovalStatus == true && q.Emp.DepartmentId == departmentId
                                select q).CountAsync();
            return result;
        }

        public async Task<DtoAverageDurationResponse> GetAverageVacationDurationPerEmployee(int empId, DateTime? startDate, DateTime? finishDate)
        {
            var result = new List<Dictionary<string, int?>>();


            DateTime? start = startDate;

            #region Get Total Months 
            var totalMonthsDuration = 0;
            while (startDate < finishDate)
            {
                totalMonthsDuration++;
                startDate = startDate.Value.Date.AddMonths(1);
            }
            #endregion

            #region Get Total Years             
            var totalYears = (finishDate.Value.Year - start.Value.Year) + 1;
            #endregion
            var totalVacationDays = await (from q in Context.EmpVacationsTransactions.AsNoTracking()
                                           where q.EmpId == empId &&
                                           q.HrApprovalStatus == true &&
                                           (q.StartDate >= start && q.FinishDate <= finishDate)
                                           select q).CountAsync();


            var averageVacationDurationPerYear = totalVacationDays / totalYears;
            var averageVacationDurationPerMonth = totalVacationDays / totalMonthsDuration;


            DtoAverageDurationResponse response = new DtoAverageDurationResponse();
            response.perMonth = averageVacationDurationPerMonth;
            response.perYear = averageVacationDurationPerYear;
            return response;

        }

        public async Task<int?> GetTotalVacationRequestsForHr()
        {
            var result = await (from q in Context.EmpVacationsTransactions.AsNoTracking()
                                where q.HrApprovalStatus == null
                                select q).CountAsync();
            return result;
        }
        public async Task<List<DtoGetAllVacationRequestsForHr>> GetAllVacationRequestsForHr()
        {
            var result = await (from q in Context.EmpVacationsTransactions.AsNoTracking()
                                where q.HrApprovalStatus == null
                                select new DtoGetAllVacationRequestsForHr
                                {
                                    id = q.Id,
                                    empName = q.Emp.UserName,
                                    departmentName = q.Emp.Department.Name,
                                    startDate = q.StartDate,
                                    finishDate = q.FinishDate,
                                    vacationType = q.VacationType.VacationTypeName,
                                    description = q.Description
                                }).OrderByDescending(c => c.id).ToListAsync();
            return result;
        }

        private async Task<EmpVacationCredit?> DeductPeriodOfRequestFromEmpCredit(EmpVacationsTransaction transactionObj)
        {
            var empCredit = await Context.EmpVacationCredits
                                      .Where(c => c.EmpId == transactionObj.EmpId && c.VacationTypeId == transactionObj.VacationTypeId)
                                      .FirstOrDefaultAsync();
            if (empCredit is not null)
            {
                var period = Math.Abs((transactionObj.FinishDate.Value - transactionObj.StartDate.Value).TotalDays);
                empCredit.CurrentCredit = empCredit.CurrentCredit - (int)period;
                Context.Set<EmpVacationCredit>().Update(empCredit);
                await Context.SaveChangesAsync();
                return empCredit;
            }
            return empCredit;

        }
        public async Task<EmpVacationsTransaction?> ApproveVacationTransaction(int transactionId)
        {
            //#-1 Get Transaction By Id And Approve It
            var transactionObj = await FindBy(c => c.Id == transactionId).FirstOrDefaultAsync();
            if (transactionObj is not null)
            {
                //#-2 Update Hr Approval Status
                transactionObj.HrApprovalStatus = true;
                await UpdateAsync(transactionObj);
                await SaveAsync();

                #region Deduct Period of request from employee credit
                var empCreditResult = await DeductPeriodOfRequestFromEmpCredit(transactionObj);
                if (empCreditResult != null)
                {
                    return transactionObj;
                }
                else
                {
                    return null;
                }
                #endregion
            }
            return null;

        }

        public async Task<EmpVacationsTransaction?> RejectVacationTransaction(int transactionId)
        {
            //#-1 Get Transaction By Id And Approve It
            var transactionObj = await FindBy(c => c.Id == transactionId).FirstOrDefaultAsync();
            if (transactionObj is not null)
            {
                //#-2 Update Hr Approval Status
                transactionObj.HrApprovalStatus = false;
                await UpdateAsync(transactionObj);
                await SaveAsync();

                return transactionObj;
            }
            return null;
        }
        #endregion
    }
}
