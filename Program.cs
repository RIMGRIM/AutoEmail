using YourApp.Models;
using YourApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
#if DEBUG
    .AddRazorRuntimeCompilation()
#endif
;
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<ScheduledEmailOptions>(builder.Configuration.GetSection("EmailSchedule"));

builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IRazorViewRenderer, RazorViewRenderer>();

// 啟用每日自動寄信（依 appsettings: EmailSchedule.Enabled）
builder.Services.AddHostedService<ScheduledEmailService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Email}/{action=Send}/{id?}");

app.Run();
