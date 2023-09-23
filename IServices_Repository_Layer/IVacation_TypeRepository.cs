using Context_Layer.Models;
using Data_Services_Layer.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IServices_Repository_Layer
{
    public interface IVacation_TypeRepository :IGenericRepository<VacationType>
    {
        Task<List<DtoGetAllVacationTypes>> GetAllVacationTypes();
        Task<DtoAddVacationType> AddNewVacationType(DtoAddVacationType body);
        Task<DtoEditVacationType> EditVacationType(DtoEditVacationType body);
    }
}
