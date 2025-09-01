using Microsoft.Extensions.Options;
using AutoEmail.Models;

namespace AutoEmail.Services;

public class ScheduledEmailOptions
{
    public bool Enabled { get; set; } = false;
    public int DailyHour { get; set; } = 9;
    public int DailyMinute { get; set; } = 0;
    public string To { get; set; } = "";
    public string Subject { get; set; } = "每日通知";
    public string Template { get; set; } = "Content";
    public Dictionary<string, string> TemplateModel { get; set; } = new();
}

public class ScheduledEmailService : BackgroundService
{
    private readonly ILogger<ScheduledEmailService> _logger;
    private readonly IEmailSender _email;
    private readonly IRazorViewRenderer _renderer;
    private readonly ScheduledEmailOptions _opt;
    private readonly TimeZoneInfo _tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Taipei");

    public ScheduledEmailService(
        ILogger<ScheduledEmailService> logger,
        IEmailSender email,
        IRazorViewRenderer renderer,
        IOptions<ScheduledEmailOptions> options)
    {
        _logger = logger;
        _email = email;
        _renderer = renderer;
        _opt = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_opt.Enabled)
        {
            _logger.LogInformation("ScheduledEmailService disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var nowLocal = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, _tz);
            var nextRunLocal = new DateTimeOffset(
                nowLocal.Year, nowLocal.Month, nowLocal.Day, _opt.DailyHour, _opt.DailyMinute, 0, _tz.GetUtcOffset(nowLocal));

            if (nextRunLocal <= nowLocal)
                nextRunLocal = nextRunLocal.AddDays(1);

            var delay = nextRunLocal - nowLocal;
            _logger.LogInformation("下一次自動寄信時間(Asia/Taipei)：{NextRun}", nextRunLocal);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException) { break; }

            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                // 用 Razor 模板產生 HTML
                var model = new {
                    UserName = _opt.TemplateModel.TryGetValue("UserName", out var u) ? u : "朋友",
                    Message  = _opt.TemplateModel.TryGetValue("Message", out var m) ? m : "這是預設訊息。"
                };
                var html = await _renderer.RenderViewToStringAsync($"/Views/Email/Templates/{_opt.Template}.cshtml", model);
                await _email.SendAsync(_opt.To, _opt.Subject, html, ct: stoppingToken);
                _logger.LogInformation("自動寄信完成，To: {To}", _opt.To);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "自動寄信失敗");
            }
        }
    }
}
