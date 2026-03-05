using RBACandABAC.AuthPolicies;
using RBACandABAC.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole(Roles.Admin));
    options.AddPolicy("Customer", policy => policy.RequireRole(Roles.Customer));
    options.AddPolicy("BranchManager", policy => policy.RequireRole(Roles.BranchManager));

    options.AddPolicy(Policies.CanIncreaseCreditLimit, policy =>
    policy.RequireRole(Roles.BranchManager)
          .RequireClaim("branch_id")
          .AddRequirements(new BusinessHoursRequirement())
          .AddRequirements(new WithinLimitAuthorityRequirement(1000000)));

    options.AddPolicy(Policies.CanViewAuditLogs, policy =>
    policy.RequireRole(Roles.Auditor, Roles.Admin)
          .AddRequirements(new BusinessHoursRequirement()));


});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
