using Context_Layer.Models;
using Data_Services_Layer.DTOS;
using IServices_Repository_Layer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Services_Repository_Layer;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<VacationManagementDBContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
   b => b.MigrationsAssembly(typeof(VacationManagementDBContext).Assembly.FullName)));


//JWT Configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration
                                                                                     .GetSection("Jwt")
                                                                                     .Get<DtoTokenManager>().SecretKey))
    };
});

#region Inject Services
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IEmpVacationCreditRepository, EmpVacationCreditRepository>();
builder.Services.AddTransient<IVacation_TypeRepository, Vacation_TypeRepository>();
builder.Services.AddTransient<IEmp_Vacations_Transactions_Repository, Emp_Vacations_Transactions_Repository>();
builder.Services.AddTransient<IDepartmentRepository, DepartmentRepository>();

#endregion

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors(policy => policy.AllowAnyHeader()
                            .AllowAnyMethod()
                            .SetIsOriginAllowed(origin => true)
                            .AllowCredentials());
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
