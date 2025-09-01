using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace YourApp.Services;

public class RazorViewRenderer : IRazorViewRenderer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IRazorViewEngine _viewEngine;

    public RazorViewRenderer(IServiceProvider sp, ITempDataProvider tempDataProvider, IRazorViewEngine viewEngine)
    {
        _serviceProvider = sp;
        _tempDataProvider = tempDataProvider;
        _viewEngine = viewEngine;
    }

    public async Task<string> RenderViewToStringAsync(string viewPath, object model)
    {
        var actionContext = new ActionContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = _serviceProvider }
        };

        var viewEngineResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewPath, isMainPage: true);
        if (!viewEngineResult.Success)
            throw new InvalidOperationException($"找不到 View: {viewPath}");

        var view = viewEngineResult.View;

        await using var sw = new StringWriter();
        var vdd = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };
        var tdd = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);
        var viewContext = new ViewContext(actionContext, view, vdd, tdd, sw, new HtmlHelperOptions());

        await view.RenderAsync(viewContext);
        return sw.ToString();
    }
}
