using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace MiniWebApplication.Helpers
{
    public class RazorViewToStringRenderer
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IActionContextAccessor _actionContextAccessor;

        public RazorViewToStringRenderer(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider,
            IActionContextAccessor actionContextAccessor)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
            _actionContextAccessor = actionContextAccessor;
        }

        public async Task<string> RenderViewToStringAsync(string viewPath, object model)
        {
            var actionContext = _actionContextAccessor.ActionContext;

            if (actionContext == null)
            {
                var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
                actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            }

            var viewEngineResult = _viewEngine.GetView(executingFilePath: null, viewPath, isMainPage: true);

            if (!viewEngineResult.Success)
            {
                var searchedLocations = string.Join(Environment.NewLine, viewEngineResult.SearchedLocations);
                var errorMessage = $"Couldn't find view '{viewPath}'. The following locations were searched:{Environment.NewLine}{searchedLocations}";
                throw new InvalidOperationException(errorMessage);
            }

            var view = viewEngineResult.View;

            using var sw = new StringWriter();
            var viewContext = new ViewContext(
                actionContext,
                view,
                new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model },
                new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                sw,
                new HtmlHelperOptions()
            );

            await view.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
}
