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
    public class Vacation_TypeRepository : GenericRepository<VacationManagementDBContext, VacationType>, IVacation_TypeRepository
    {
        public async Task<List<DtoGetAllVacationTypes>> GetAllVacationTypes()
        {
            var result = await (from q in Context.VacationTypes.AsNoTracking()
                                select new DtoGetAllVacationTypes
                                {
                                    id = q.Id,
                                    name = q.VacationTypeName
                                }).OrderByDescending(c => c.id).ToListAsync();
            return result;

        }
        public async Task<DtoAddVacationType> AddNewVacationType(DtoAddVacationType body)
        {
            //#-1 Check If Name Exist Before
            var nameIsExist = await FindBy(c => c.VacationTypeName == body.name).FirstOrDefaultAsync();
            if (nameIsExist == null)
            {
                VacationType vacationType = new VacationType();
                vacationType.VacationTypeName = body.name;
                await AddAsync(vacationType);
                await SaveAsync();
                return body;

            }
            return null;

        }
        public async Task<DtoEditVacationType> EditVacationType(DtoEditVacationType body)
        {
            //Get Vacation Obj
            var vacationObj = await FindBy(c => c.Id == body.id).FirstOrDefaultAsync();
            if (vacationObj is not null)
            {
                vacationObj.VacationTypeName = body.name;
                await UpdateAsync(vacationObj);
                await SaveAsync();
                return body;
            }
            return null;
        }
    }
}
