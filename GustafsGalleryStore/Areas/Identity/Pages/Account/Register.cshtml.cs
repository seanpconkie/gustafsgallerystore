using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GustafsGalleryStore.Areas.Identity.Data;
using GustafsGalleryStore.Models;
using GustafsGalleryStore.Models.DataModels;
using GustafsGalleryStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using IEmailSender = GustafsGalleryStore.Services.IEmailSender;

namespace GustafsGalleryStore.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly GustafsGalleryStoreContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
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

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
            public string Forename { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
            public string Surname { get; set; }

            [Required]
            public string Title { get; set; }
            public string RecaptchaResponse { get; set; }
            public string StatusMessage { get; set; }
            public string FailureMessage { get; set; }
            public string SuccessMessage { get; set; }

            public List<SelectListItem> Titles { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {

            Input = new InputModel
            {
                Titles = CustomerTitle.GetTitles(_context.Titles.Where(c => c.Id > 0).OrderBy(x => x.Value).ToList())
            };
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {

                if (string.IsNullOrWhiteSpace(Input.RecaptchaResponse))
                {
                    Input.FailureMessage = "Something went wrong, please try again.";
                    Input.Titles = CustomerTitle.GetTitles(_context.Titles.Where(c => c.Id > 0).OrderBy(x => x.Value).ToList());
                    return Page();
                }

                if (!RecaptchaHelper.IsReCaptchValid(Input.RecaptchaResponse))
                {
                    Input.FailureMessage = "Something went wrong, please try again.";
                    Input.Titles = CustomerTitle.GetTitles(_context.Titles.Where(c => c.Id > 0).OrderBy(x => x.Value).ToList());
                    return Page();
                }

                var user = new ApplicationUser { 
                    UserName = Input.Email, 
                    Email = Input.Email,
                    Forename = Input.Forename,
                    Surname = Input.Surname,
                    Title = Input.Title
                };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            Input.FailureMessage = "Something went wrong, please try again.";
            Input.Titles = CustomerTitle.GetTitles(_context.Titles.Where(c => c.Id > 0).OrderBy(x => x.Value).ToList());
            return Page();
        }
    }
}
