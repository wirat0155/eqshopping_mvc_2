using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Controllers
{
    public class BaseController : Controller
    {
        private readonly ICompositeViewEngine _viewEngine;
        public BaseController(ICompositeViewEngine viewEngine)
        {
            _viewEngine = viewEngine;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }

        protected JsonResult GenerateErrorResponse()
        {
            var errorMessages = ModelState.Keys
                .Where(key => ModelState[key].Errors.Any())
                .SelectMany(key => ModelState[key].Errors.Select((error, index) =>
                {
                    var errorMessage = error.ErrorMessage;

                    if (key.Contains("["))
                    {
                // Replace the first part of the key, keeping the brackets as is
                var formattedKey = key.Replace("[", "[") // Ensure we keep the brackets
                                               .Replace("]", "]"); // Ensure we keep the brackets

                // Format the key correctly with the brackets
                return new { property = formattedKey, errorMessage };
                    }
                    else
                    {
                        return new { property = key, errorMessage };
                    }
                }))
                .ToList();

            if (errorMessages.Count == 0)
            {
                errorMessages.Add(new { property = "txt_form", errorMessage = "Invalid data." });
            }

            return Json(new { success = false, errors = errorMessages });
        }

        public async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            ViewData.Model = model;
            using (var writer = new StringWriter())
            {
                var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);
                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"View {viewName} not found.");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return writer.GetStringBuilder().ToString();
            }
        }

    }
}
