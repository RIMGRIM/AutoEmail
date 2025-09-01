using Microsoft.AspNetCore.Mvc;
using AutoEmail.Models;
using AutoEmail.Services;

namespace AutoEmail.Controllers;

public class EmailController : Controller
{
    private readonly IEmailSender _email;
    private readonly IRazorViewRenderer _renderer;

    public EmailController(IEmailSender email, IRazorViewRenderer renderer)
    {
        _email = email;
        _renderer = renderer;
    }

    [HttpGet]
    public IActionResult Send() => View(new EmailRequest());

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> Send(EmailRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(req);

        // 用 Razor 模板產出 HTML 內容
        var html = await _renderer.RenderViewToStringAsync("/Views/Email/Templates/Content.cshtml", new {
            UserName = string.IsNullOrWhiteSpace(req.UserName) ? "朋友" : req.UserName,
            Message = req.Message
        });

        await _email.SendAsync(req.To, req.Subject, html, ct: ct);

        TempData["ok"] = "Email 已寄出！";
        return RedirectToAction(nameof(Send));
    }
}
