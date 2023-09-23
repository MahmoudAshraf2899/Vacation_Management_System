using Context_Layer.Models;
using Data_Services_Layer.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace IServices_Repository_Layer
{
    public interface IEmp_Vacations_Transactions_Repository : IGenericRepository<EmpVacationsTransaction>
    {
        Task<List<DtoGetAllVacationsTransactionForEmp>> GetAllVacationTransForEmployee(int empId);
        Task<Tuple<string, EmpVacationsTransaction>> AddVacationTransForEmp(DtoAddEmpVacationTransaction body);
        Task<Tuple<string, EmpVacationsTransaction>> EditVacationTransForEmp(DtoEditEmpVacationTransaction body);
        Task<DtoEmpVacationTransactionById?> GetVacationTransForEmpById(int vacationTransId);

        #region HR-Dashboard
        Task<int?> GetTotalNumberVacationsPerDepartment(int departmentId);
        /// <summary>
        /// Allow Hr to know the average for specific period
        /// </summary>
        /// <param name="empId"></param>
        /// <param name="startDate"></param>
        /// <param name="finishDate"></param>
        /// <returns></returns>
        Task<DtoAverageDurationResponse> GetAverageVacationDurationPerEmployee(int empId, DateTime? startDate, DateTime? finishDate);

        Task<int?> GetTotalVacationRequestsForHr();

        Task<List<DtoGetAllVacationRequestsForHr>> GetAllVacationRequestsForHr();

        Task<EmpVacationsTransaction?> ApproveVacationTransaction(int transactionId);
        Task<EmpVacationsTransaction?> RejectVacationTransaction(int transactionId);
        #endregion
    }
}
