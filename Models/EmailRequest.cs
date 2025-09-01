using System.ComponentModel.DataAnnotations;

namespace AutoEmail.Models;

public class EmailRequest
{
    [Required, EmailAddress]
    public string To { get; set; } = "";

    [Required, MaxLength(200)]
    public string Subject { get; set; } = "";

    [Required]
    public string Message { get; set; } = "";

    public string? UserName { get; set; } // 用於模板展示
}
