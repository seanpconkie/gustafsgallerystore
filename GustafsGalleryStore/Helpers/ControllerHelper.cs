using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GustafsGalleryStore.Areas.Identity.Data;
using GustafsGalleryStore.Models.DataModels;
using GustafsGalleryStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace GustafsGalleryStore.Helpers
{
    public static class ControllerHelper
    {

        #region Helpers

        public static IActionResult ReturnResult(UpdateResult result, string message = null)
        {
            if (result == UpdateResult.Success)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    return new OkResult();
                }
                else
                {
                    return new OkObjectResult(message);
                }
            }
            else if (result == UpdateResult.Error)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    return new BadRequestResult();
                }
                else
                {
                    return new BadRequestObjectResult(message);
                }
            }

            return new EmptyResult();
        }

        public static IActionResult RedirectToLocal(ControllerBase controller, string returnUrl)
        {
            if (controller.Url.IsLocalUrl(returnUrl))
            {
                return controller.Redirect(returnUrl);
            }
            else
            {
                return controller.RedirectToAction("/Account/Login");
            }
        }

        public static void AddErrors(ControllerBase controller, IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                controller.ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        #endregion
    }
}
