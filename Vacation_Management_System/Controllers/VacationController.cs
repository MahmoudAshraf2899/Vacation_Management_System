using Context_Layer.Models;
using Data_Services_Layer.DTOS;
using IServices_Repository_Layer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Vacation_Management_System.Helpers;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using Services_Repository_Layer;
using System.Web.WebPages;

namespace Vacation_Management_System.Controllers
{
    [Route("api/vacation/[controller]")]
    [ApiController]
    public class VacationController : ControllerBase
    {
        private int _userId;
        private bool _IsHRAdmin;

        private readonly VacationManagementDBContext _context;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _accessor;
        private readonly IUserRepository _userRepository;
        private readonly IEmpVacationCreditRepository _empVacationCreditRepository;
        private readonly IVacation_TypeRepository _vacation_TypeRepository;
        private readonly IEmp_Vacations_Transactions_Repository _emp_Vacations_Transactions_Repository;
        private readonly IDepartmentRepository _departmentRepository;

        public VacationController(VacationManagementDBContext context,
            IConfiguration config,
            IHttpContextAccessor accessor,
            IUserRepository userRepository,
            IEmpVacationCreditRepository empVacationCreditRepository,
            IVacation_TypeRepository vacation_TypeRepository,
            IEmp_Vacations_Transactions_Repository emp_Vacations_Transactions_Repository,
            IDepartmentRepository departmentRepository)
        {
            _context = context;
            _config = config;
            _accessor = accessor;
            _userRepository = userRepository;
            _empVacationCreditRepository = empVacationCreditRepository;
            _vacation_TypeRepository = vacation_TypeRepository;
            _emp_Vacations_Transactions_Repository = emp_Vacations_Transactions_Repository;
            _departmentRepository = departmentRepository;
            StringValues tokenHeader = "";
            #region Extract Data From Token

            var token = _accessor.HttpContext.User.Identity;
            var identity = (ClaimsIdentity)token;
            _accessor.HttpContext.Request.Headers.TryGetValue("Authorization", out tokenHeader);

            if (tokenHeader.Any() == true)
            {
                if (tokenHeader[0] != null)
                {
                    var userInfo = TokenManagerFactor.GetUserInfo(identity);
                    if (userInfo.id != 0)
                    {
                        _userId = userInfo.id;
                        _IsHRAdmin = userInfo.isHrAdmin ?? false;
                    }
                }
            }
            #endregion
        }


        #region Authorization & Authentication
        private Tuple<string, TokenResults> GenerateToken(User info)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var iss = "Seven Seas Server";

            var claims = new[] {
                new Claim(ClaimTypes.Name, info.UserName),
                new Claim(ClaimTypes.Actor, iss),
                new Claim(ClaimTypes.Version, info.Id.ToString()),
                new Claim(ClaimTypes.Hash, Guid.NewGuid().ToString("D")),
                new Claim(ClaimTypes.Dns, info.IsHrAdmin.ToString()??"false"),

             };
            //Let Token Expires After Three Days
            var token = new JwtSecurityToken(expires: DateTime.Now.AddDays(3), claims: claims, signingCredentials: credentials);

            var tokenValid = new JwtSecurityTokenHandler().WriteToken(token);

            var result = new TokenResults
            {
                access_token = tokenValid,
                iss = "api/Nightmare",
                sub = info.UserName,
                Dns = info.IsHrAdmin.ToString() ?? "false",
                acn = info.Email,
                aci = info.Id.ToString(),
            };
            var finalResult = Tuple.Create(tokenValid, result);
            return finalResult;
        }


        [HttpPost]
        [Route("AddNewUser")]
        public async Task<IActionResult> AddNewUser(DtoRegister model)
        {
            //First : Check If User Name is Exist in Users Table before
            bool isExist = _context.Users.AsNoTracking().Where(C => C.UserName == model.userName).Any();
            if (isExist)
            {
                //User Already Exist
                string userNameAlert = "User Name is Already Exist";
                return BadRequest(userNameAlert);
            }
            //Second : Check If Passwords Matches
            string passwordMatching = "Password Doesn't Match";
            if (model.password != model.confirmPassword)
            {
                return BadRequest(passwordMatching);
            }
            //Third : Add Operation
            #region Add New User (We Should Move It To Repository but deadline )
            User newUser = new User();
            newUser.Email = model.email;
            newUser.CreatedAt = DateTime.Now.Date;
            newUser.CreatedBy = _userId;
            newUser.UserName = model.userName;
            newUser.FirstName = model.firstName;
            newUser.LastName = model.lastName;
            newUser.IsHrAdmin = model.isHrAdmin;
            newUser.Phone = model.phone;
            newUser.DepartmentId = model.departmentId;
            newUser.Password = PasswordHash.CreateHash(model.password);

            await _userRepository.AddAsync(newUser);
            await _userRepository.SaveAsync();
            #endregion
            return Ok();

        }


        [HttpPost]
        [Route("Login")]
        public IActionResult Login(DtoLogin dto)
        {
            var userObj = _userRepository.FindBy(c => c.UserName == dto.userName).FirstOrDefault();
            if (userObj is not null)
            {
                var checkPassword = PasswordHash.CreateHash(dto.password);
                if (!PasswordHash.ValidatePassword(dto.password, userObj.Password))
                {
                    return BadRequest("InvalidPassword");
                }
            }
            else
            {
                return BadRequest("UserNotFound");
            }
            var token = GenerateToken(userObj);
            return Ok(token.Item2);
        }
        #endregion


        #region Vacation Types
        [Authorize]
        [HttpGet]
        [Route("GetAllVacationTypes")]
        public async Task<IActionResult> GetAllVacationTypes()
        {
            if (_IsHRAdmin)
            {
                var result = await _vacation_TypeRepository.GetAllVacationTypes();
                return Ok(result);
            }
            return BadRequest();
        }
        [Authorize]
        [HttpGet]
        [Route("AddNewVacationTypes")]
        public async Task<IActionResult> AddNewVacationTypes(DtoAddVacationType body)
        {
            if (_IsHRAdmin)
            {
                var result = await _vacation_TypeRepository.AddNewVacationType(body);
                if (result is not null)
                    return Ok(result);

                return BadRequest();
            }
            return BadRequest();
        }

        [Authorize]
        [HttpGet]
        [Route("EditVacationType")]
        public async Task<IActionResult> EditVacationType(DtoEditVacationType body)
        {
            if (_IsHRAdmin)
            {
                var result = await _vacation_TypeRepository.EditVacationType(body);
                if (result is not null)
                    return Ok(result);

                return BadRequest();
            }
            return BadRequest();
        }

        #endregion

        #region Employee Credit Vacation

        //This Api For Employee Dropdown to assign credit to new employee
        [Authorize]
        [HttpGet]
        [Route("GetAllNewEmployeesForDropDown")]
        public async Task<IActionResult> GetAllNewEmployeesForDropDown()
        {
            if (_IsHRAdmin)
            {
                var result = await _empVacationCreditRepository.GetAllNewEmployeesForDropDown();
                return Ok(result);
            }
            return BadRequest();
        }

        [Authorize]
        [HttpGet]
        [Route("GetAllEmployeesCreditVacation")]
        public async Task<IActionResult> GetAllEmployeesCreditVacation()
        {
            if (_IsHRAdmin)
            {
                var result = await _empVacationCreditRepository.GetAllEmployeesCreditVacation();
                return Ok(result);
            }
            return BadRequest();
        }


        [Authorize]
        [HttpPost]
        [Route("AddEmployeeVacationCreditByHr")]
        public async Task<IActionResult> AddEmployeeVacationCreditByHr(DtoAddEmployeeCreditByHr body)
        {
            if (_IsHRAdmin)
            {
                var result = await _empVacationCreditRepository.AddEmpVacationCredit(body);
                if (result is not null)
                    return Ok(result);

                return BadRequest();

            }
            return BadRequest();
        }

        [Authorize]
        [HttpPost]
        [Route("UpdateEmployeeVacationCreditByHr")]
        public async Task<IActionResult> UpdateEmployeeVacationCreditByHr(DtoUpdateEmployeeVacationCreditByHr body)
        {
            if (_IsHRAdmin)
            {
                var result = await _empVacationCreditRepository.UpdateEmpVacationCredit(body);
                if (result is not null)
                    return Ok(result);

                return BadRequest();

            }
            return BadRequest();
        }


        #endregion

        #region Vacation Transaction    

        [Authorize]
        [HttpGet]
        [Route("GetAllVacationTransForEmployee")]
        public async Task<IActionResult> GetAllVacationTransForEmployee(int empId)
        {
            var result = await _emp_Vacations_Transactions_Repository.GetAllVacationTransForEmployee(empId);
            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        [Route("GetVacationTransForEmpById")]
        public async Task<IActionResult> GetVacationTransForEmpById(int vacationTransId)
        {
            var result = await _emp_Vacations_Transactions_Repository.GetVacationTransForEmpById(vacationTransId);
            return Ok(result);
        }


        [Authorize]
        [HttpPost]
        [Route("AddVacationTransForEmp")]
        public async Task<IActionResult> AddVacationTransForEmp(DtoAddEmpVacationTransaction body)
        {
            var result = await _emp_Vacations_Transactions_Repository.AddVacationTransForEmp(body);
            if (result.Item2 is not null)
            {
                return Ok(result.Item1);
            }
            return BadRequest(result.Item1);
        }

        [Authorize]
        [HttpPost]
        [Route("EditVacationTransForEmp")]
        public async Task<IActionResult> EditVacationTransForEmp(DtoEditEmpVacationTransaction body)
        {
            var result = await _emp_Vacations_Transactions_Repository.EditVacationTransForEmp(body);
            if (result.Item2 is not null)
            {
                return Ok(result.Item1);
            }
            return BadRequest(result.Item1);
        }
        #endregion

        #region HR-Dashboard 
       
      

        [Authorize]
        [HttpGet]
        [Route("GetTotalVacationRequestsForHr")]
        public async Task<IActionResult> GetTotalVacationRequestsForHr()
        {
            if (_IsHRAdmin)
            {
                var result = await _emp_Vacations_Transactions_Repository.GetTotalVacationRequestsForHr();

                return Ok(result);
            }
            return BadRequest();
        }

        [Authorize]
        [HttpGet]
        [Route("GetAllVacationRequestsForHr")]
        public async Task<IActionResult> GetAllVacationRequestsForHr()
        {
            if (_IsHRAdmin)
            {
                var result = await _emp_Vacations_Transactions_Repository.GetAllVacationRequestsForHr();

                return Ok(result);
            }
            return BadRequest();
        }


        [Authorize]
        [HttpPost]
        [Route("ApproveVacationTransactionRequest")]
        public async Task<IActionResult> ApproveVacationTransactionRequest(int transactionId)
        {
            if (_IsHRAdmin)
            {
                var result = await _emp_Vacations_Transactions_Repository.ApproveVacationTransaction(transactionId);
                if(result is not null)
                return Ok(result);

                return BadRequest();
            }
            return BadRequest();
        }

        [Authorize]
        [HttpPost]
        [Route("RejectVacationTransactionRequest")]
        public async Task<IActionResult> RejectVacationTransactionRequest(int transactionId)
        {
            if (_IsHRAdmin)
            {
                var result = await _emp_Vacations_Transactions_Repository.RejectVacationTransaction(transactionId);
                if (result is not null)
                    return Ok(result);

                return BadRequest();
            }
            return BadRequest();
        }

        #endregion

        #region Departments
        [Authorize]
        [HttpGet]
        [Route("GetAllDepartments")]
        public async Task<IActionResult> GetAllDepartments()
        {
            var result = await _departmentRepository.GetAllDepartments();
            return Ok(result);
        }
        #endregion

        #region Employees
        [Authorize]
        [HttpGet]
        [Route("GetAllEmployeesForGrid")]
        public async Task<IActionResult> GetAllEmployeesForGrid()
        {
            var result = await _userRepository.GetAllEmpsForGrid();
            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        [Route("AverageVacationDurationPerEmployee")]
        public async Task<IActionResult> AverageVacationDurationPerEmployee(int empId, DateTime? startDate, DateTime? finishDate)
        {
            if (_IsHRAdmin)
            {
                var result = await _emp_Vacations_Transactions_Repository.GetAverageVacationDurationPerEmployee(empId, startDate, finishDate);
                return Ok(result);
            }
            return BadRequest();
        }
        #endregion

    }
}
