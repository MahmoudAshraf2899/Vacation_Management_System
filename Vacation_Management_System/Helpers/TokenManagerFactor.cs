using Context_Layer.Models;
using Data_Services_Layer.DTOS;
using System.Security.Claims;
using System.Security.Principal;

namespace Vacation_Management_System.Helpers
{
    public class TokenManagerFactor
    {
        public static DtoUserInfo GetUserInfo(IIdentity token)
        {
            try
            {
                var identity = (ClaimsIdentity)token;
                List<Claim> claims = identity.Claims.ToList();
                var account = new DtoUserInfo();

                var sub = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);                 

                if (sub != null)
                {
                    using var context = new VacationManagementDBContext();
                    account = context.Users.Where(x => x.UserName == sub.Value)
                    .Select(x => new DtoUserInfo
                    {
                        id = x.Id,
                        userName = x.UserName,
                        isHrAdmin = x.IsHrAdmin
                    }).FirstOrDefault();

                    if (account == null)
                    {
                        return new DtoUserInfo();
                    }
                    return account;
                }

                return account;
            }
            catch (Exception ex)
            {
                DtoUserInfo info = new DtoUserInfo();
                info.userName = ex.Message;
                return info;

            }
        }
    }
}
