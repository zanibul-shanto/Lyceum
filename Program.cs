using Lyceum.Components;
using Lyceum.Models;
using Lyceum.Services;
using Radzen;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<LyceumDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ASP.NET Identity with cookie authentication
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<LyceumDbContext>()
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<LyceumUserClaimsPrincipalFactory>();

// Cookie configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/login";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Auth state provider — revalidating against Identity
builder.Services.AddScoped<AuthenticationStateProvider, LyceumAuthStateProvider>();

// Application services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<TeacherService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<SubjectService>();
builder.Services.AddScoped<EnrollmentService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<GradeService>();
builder.Services.AddScoped<TimetableService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<SystemSettingService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<CsvImportService>();

builder.Services.AddRadzenComponents();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Database auto-initialization/seeding on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LyceumDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    await DbInitializer.InitializeAsync(context, userManager);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/api/auth/login", async (HttpContext context, SignInManager<User> signInManager, UserManager<User> userManager) =>
{
    var form = await context.Request.ReadFormAsync();
    string username = form["username"]!;
    string password = form["password"]!;
    
    var user = await userManager.FindByNameAsync(username) ?? await userManager.FindByEmailAsync(username);
    if (user != null && user.IsActive)
    {
        var result = await signInManager.PasswordSignInAsync(user, password, false, false);
        if (result.Succeeded)
        {
            return Results.Redirect(user.Role switch
            {
                UserRole.Admin => "/admin/dashboard",
                UserRole.Teacher => "/teacher/dashboard",
                _ => "/student/dashboard"
            });
        }
    }
    return Results.Redirect("/login?error=Invalid credentials");
}).DisableAntiforgery();

app.MapGet("/api/auth/logout", async (SignInManager<User> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/login");
});

app.Run();
