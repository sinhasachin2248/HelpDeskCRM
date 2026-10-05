using HelpDeskCRM.Data;
using HelpDeskCRM.Models;
using HelpDeskCRM.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace HelpDeskCRM.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }


        // =====================================================
        // ADMIN LOGIN PAGE
        // =====================================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.LoginSuccess = false;

            return View("~/Views/Account/Login.cshtml");
        }


        // =====================================================
        // ADMIN LOGIN
        // =====================================================

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model,
            string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.LoginSuccess = false;

            if (!ModelState.IsValid)
            {
                return View(
                    "~/Views/Account/Login.cshtml",
                    model
                );
            }

            // User ID is used only for login
            string userId = model.Username.Trim();

            // Only ACTIVE admins can login
            var admin = await _db.Admins
                .FirstOrDefaultAsync(x =>
                    x.UserId.ToLower() == userId.ToLower() &&
                    x.Status == 1
                );

            if (admin == null ||
                !AdminPasswordHasher.VerifyPassword(
                    model.Password,
                    admin.PasswordHash))
            {
                ModelState.AddModelError(
                    "",
                    "Invalid User ID or Password."
                );

                return View(
                    "~/Views/Account/Login.cshtml",
                    model
                );
            }


            // =================================================
            // ADMIN AUTHENTICATION
            // =================================================

            var claims = new List<Claim>
            {
                // User ID - authentication only
                new Claim(
                    ClaimTypes.NameIdentifier,
                    admin.UserId
                ),

                // Admin Name - displayed throughout application
                new Claim(
                    ClaimTypes.Name,
                    admin.AdminName
                ),

                // Admin Role
                new Claim(
                    ClaimTypes.Role,
                    "Admin"
                )
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            var properties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                properties
            );

            // Stay on the same login page
            ViewBag.LoginSuccess = true;
            ViewBag.LoggedInUser = admin.AdminName;

            ModelState.Clear();

            return View(
                "~/Views/Account/Login.cshtml",
                new LoginViewModel()
            );
        }


        // =====================================================
        // CREATE ADMIN PAGE
        // INDEPENDENT - NO LOGIN REQUIRED
        // =====================================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(
                "~/Views/Account/Register.cshtml"
            );
        }


        // =====================================================
        // CREATE ADMIN
        // INDEPENDENT - NO LOGIN REQUIRED
        // =====================================================

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            AdminRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(
                    "~/Views/Account/Register.cshtml",
                    model
                );
            }

            string adminName = model.AdminName.Trim();
            string userId = model.UserId.Trim();
            string password = model.Password;


            // =================================================
            // ADMIN NAME VALIDATION
            // =================================================

            if (!Regex.IsMatch(
                    adminName,
                    @"^[a-zA-Z ]+$"))
            {
                ModelState.AddModelError(
                    "AdminName",
                    "Admin Name can contain only letters and spaces."
                );

                return View(
                    "~/Views/Account/Register.cshtml",
                    model
                );
            }


            // =================================================
            // DUPLICATE ADMIN NAME CHECK
            // =================================================

            bool adminNameExists =
                await _db.Admins.AnyAsync(
                    x => x.AdminName.ToLower() ==
                         adminName.ToLower()
                );

            if (adminNameExists)
            {
                ModelState.AddModelError(
                    "AdminName",
                    "Admin with this name already exists."
                );

                return View(
                    "~/Views/Account/Register.cshtml",
                    model
                );
            }


            // =================================================
            // DUPLICATE USER ID CHECK
            // =================================================

            bool userExists =
                await _db.Admins.AnyAsync(
                    x => x.UserId.ToLower() ==
                         userId.ToLower()
                );

            if (userExists)
            {
                ModelState.AddModelError(
                    "UserId",
                    "User ID already exists."
                );

                return View(
                    "~/Views/Account/Register.cshtml",
                    model
                );
            }


            // =================================================
            // PASSWORD VALIDATION
            // =================================================

            if (password.Length < 8 ||
                password.Length > 16)
            {
                ModelState.AddModelError(
                    "Password",
                    "Password must be between 8 and 16 characters."
                );
            }

            if (!Regex.IsMatch(password, "[A-Z]"))
            {
                ModelState.AddModelError(
                    "Password",
                    "Password must contain at least one uppercase letter."
                );
            }

            if (!Regex.IsMatch(password, "[a-z]"))
            {
                ModelState.AddModelError(
                    "Password",
                    "Password must contain at least one lowercase letter."
                );
            }

            if (!Regex.IsMatch(password, "[0-9]"))
            {
                ModelState.AddModelError(
                    "Password",
                    "Password must contain at least one number."
                );
            }

            if (!Regex.IsMatch(
                    password,
                    @"[^a-zA-Z0-9]"))
            {
                ModelState.AddModelError(
                    "Password",
                    "Password must contain at least one special symbol."
                );
            }

            if (password != model.ConfirmPassword)
            {
                ModelState.AddModelError(
                    "ConfirmPassword",
                    "Password and Confirm Password must match."
                );
            }


            // =================================================
            // VALIDATION FAILED
            // =================================================

            if (!ModelState.IsValid)
            {
                return View(
                    "~/Views/Account/Register.cshtml",
                    model
                );
            }


            // =================================================
            // CREATE ADMIN
            // =================================================

            var admin = new Admin
            {
                AdminName = adminName,

                UserId = userId,

                PasswordHash =
                    AdminPasswordHasher.HashPassword(
                        password
                    ),

                // 1 = Active
                Status = 1,

                CreatedAt = DateTime.Now
            };

            _db.Admins.Add(admin);

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Admin account created successfully. Please login.";

            return RedirectToAction(
                nameof(Login)
            );
        }


        // =====================================================
        // CUSTOMER LOGIN PAGE
        // DO NOT REMOVE
        // =====================================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult CustomerLogin()
        {
            return View(
                "~/Views/Account/CustomerLogin.cshtml"
            );
        }


        // =====================================================
        // CUSTOMER LOGIN
        // =====================================================

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerLogin(
            CustomerLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(
                    "~/Views/Account/CustomerLogin.cshtml",
                    model
                );
            }

            string email = model.Email.Trim();

            var account =
                await _db.CustomerAccounts
                    .Include(x => x.Customer)
                    .FirstOrDefaultAsync(
                        x =>
                            x.Customer != null &&
                            x.Customer.Email.ToLower() ==
                            email.ToLower()
                    );

            if (account == null ||
                account.Customer == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password."
                );

                return View(
                    "~/Views/Account/CustomerLogin.cshtml",
                    model
                );
            }


            // =================================================
            // CUSTOMER PASSWORD VERIFICATION
            // =================================================

            bool passwordValid =
                BCrypt.Net.BCrypt.Verify(
                    model.Password,
                    account.PasswordHash
                );

            if (!passwordValid)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password."
                );

                return View(
                    "~/Views/Account/CustomerLogin.cshtml",
                    model
                );
            }


            // =================================================
            // CUSTOMER AUTHENTICATION
            // =================================================

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    account.Customer.CustomerId.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    $"{account.Customer.FirstName} " +
                    $"{account.Customer.LastName}"
                ),

                new Claim(
                    ClaimTypes.Email,
                    account.Customer.Email
                ),

                new Claim(
                    ClaimTypes.Role,
                    "Customer"
                )
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            var properties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                properties
            );

            return RedirectToAction(
                "Index",
                "CustomerPortal"
            );
        }


        // =====================================================
        // LOGOUT
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction(
                nameof(Login)
            );
        }


        // =====================================================
        // ACCESS DENIED
        // =====================================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}