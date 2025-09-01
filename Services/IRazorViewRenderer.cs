namespace YourApp.Services;

public interface IRazorViewRenderer
{
    Task<string> RenderViewToStringAsync(string viewPath, object model);
}
