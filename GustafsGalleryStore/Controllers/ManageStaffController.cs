using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GustafsGalleryStore.Areas.Identity.Data;
using GustafsGalleryStore.Helpers;
using GustafsGalleryStore.Models.DataModels;
using GustafsGalleryStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IEmailSender = GustafsGalleryStore.Services.IEmailSender;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
namespace GustafsGalleryStore.Controllers
{
    [Authorize(Roles = MasterStrings.StaffRole)]
    public class ManageStaffController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ManageStaffController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly GustafsGalleryStoreContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ManageStaffController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ManageStaffController> logger,
            IEmailSender emailSender,
            GustafsGalleryStoreContext context,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _roleManager = roleManager;
        }

        public string ReturnUrl { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        // GET: /<controller>/
        public async Task<IActionResult> Index(string statusMessage = null, string successMessage = null, string failureMessage = null)
        {

            var viewModel = new StaffListViewModel
            {
                Staff = await _userManager.GetUsersInRoleAsync(MasterStrings.StaffRole),
                StatusMessage = statusMessage,
                SuccessMessage = successMessage,
                FailureMessage = failureMessage
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult AddStaff(string statusMessage = null, string successMessage = null, string failureMessage = null)
        {

            var viewModel = new InputViewModel
            {
                Titles = CustomerTitle.GetTitles(_context.Titles.Where(c => c.Id > 0).OrderBy(x => x.Value).ToList()),
                ReturnUrl = "~/ManageStaff/AddStaff"
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStaff(InputViewModel input, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = input.Email,
                    Email = input.Email,
                    Forename = input.Forename,
                    Surname = input.Surname,
                    Title = input.Title
                };

                var result = await _userManager.CreateAsync(user, input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    await _userManager.AddToRoleAsync(user, MasterStrings.StaffRole);

                    return ControllerHelper.RedirectToLocal(this,"/ManageStaff?successMessage=" + input.Forename + " added.");

                }

                ControllerHelper.AddErrors(this,result);

                input.FailureMessage = "Something went wrong.";

            }

            input.Titles = CustomerTitle.GetTitles(_context.Titles.Where(c => c.Id > 0).OrderBy(x => x.Value).ToList());
            return View(input);
        }

        public async Task<IActionResult> DeleteStaff(string id, string returnUrl = null)
        {

            IList<ApplicationUser> users = new List<ApplicationUser>();

            users = await _userManager.GetUsersInRoleAsync(MasterStrings.StaffRole);

            if (users.Count > 0)
            {
                foreach (var user in users)
                {
                    if (user.Id == id)
                    {
                        var result = await _userManager.DeleteAsync(user);
                        if (result.Succeeded)
                        {
                            _logger.LogInformation("User account deleted.");

                            return ControllerHelper.RedirectToLocal(this,"/ManageStaff?successMessage=User deleted.");
                        }

                        ControllerHelper.AddErrors(this,result);

                        return ControllerHelper.RedirectToLocal(this, "/ManageStaff?failureMessage=Something went wrong.");
                    }
                }
            }

            return ControllerHelper.RedirectToLocal(this, "/ManageStaff?failureMessage=No user found.");
        }


    }
}
