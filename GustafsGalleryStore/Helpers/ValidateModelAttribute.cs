using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace GustafsGalleryStore.Helpers
{

    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid == false)
            {
                context.Result = new BadRequestObjectResult(
                    context.ModelState.Values
                        .SelectMany(e => e.Errors)
                        .Select(e => e.ErrorMessage));
            }
        }

    }
}
