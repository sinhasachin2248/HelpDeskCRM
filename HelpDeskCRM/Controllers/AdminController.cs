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
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }


        // =====================================================
        // VIEW ADMIN
        // ONLY ACTIVE ADMINS ARE DISPLAYED
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model =
                new AdminManagementViewModel
                {
                    Admins =
                        await _db.Admins
                            .Where(x => x.Status == 1)
                            .OrderBy(x => x.AdminId)
                            .ToListAsync()
                };

            return View(model);
        }


        // =====================================================
        // UPDATE ADMIN - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int adminId)
        {
            var admin =
                await _db.Admins
                    .FirstOrDefaultAsync(
                        x => x.AdminId == adminId &&
                             x.Status == 1
                    );

            if (admin == null)
            {
                TempData["ErrorMessage"] =
                    "Active admin not found.";

                return RedirectToAction(nameof(Index));
            }

            return View(admin);
        }


        // =====================================================
        // UPDATE ADMIN - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int AdminId,
            string AdminName,
            string UserId,
            string? NewPassword)
        {
            AdminName =
                AdminName?.Trim() ?? "";

            UserId =
                UserId?.Trim() ?? "";


            var admin =
                await _db.Admins
                    .FirstOrDefaultAsync(
                        x => x.AdminId == AdminId &&
                             x.Status == 1
                    );

            if (admin == null)
            {
                TempData["ErrorMessage"] =
                    "Active admin not found.";

                return RedirectToAction(nameof(Index));
            }


            // =================================================
            // ADMIN NAME VALIDATION
            // =================================================

            if (string.IsNullOrWhiteSpace(AdminName))
            {
                ModelState.AddModelError(
                    "AdminName",
                    "Admin Name is required."
                );
            }
            else if (!Regex.IsMatch(
                AdminName,
                @"^[a-zA-Z ]+$"))
            {
                ModelState.AddModelError(
                    "AdminName",
                    "Admin Name can contain only letters and spaces."
                );
            }


            // =================================================
            // DUPLICATE ADMIN NAME
            // =================================================

            if (!string.IsNullOrWhiteSpace(AdminName))
            {
                bool duplicateAdminName =
                    await _db.Admins.AnyAsync(
                        x =>
                            x.AdminId != AdminId &&
                            x.Status == 1 &&
                            x.AdminName.ToLower() ==
                            AdminName.ToLower()
                    );

                if (duplicateAdminName)
                {
                    ModelState.AddModelError(
                        "AdminName",
                        "Admin with this name already exists."
                    );
                }
            }


            // =================================================
            // USER ID VALIDATION
            // =================================================

            if (string.IsNullOrWhiteSpace(UserId))
            {
                ModelState.AddModelError(
                    "UserId",
                    "User ID is required."
                );
            }
            else if (UserId.Length < 3 ||
                     UserId.Length > 30)
            {
                ModelState.AddModelError(
                    "UserId",
                    "User ID must be between 3 and 30 characters."
                );
            }


            // =================================================
            // DUPLICATE USER ID
            // =================================================

            if (!string.IsNullOrWhiteSpace(UserId))
            {
                bool duplicateUserId =
                    await _db.Admins.AnyAsync(
                        x =>
                            x.AdminId != AdminId &&
                            x.UserId.ToLower() ==
                            UserId.ToLower()
                    );

                if (duplicateUserId)
                {
                    ModelState.AddModelError(
                        "UserId",
                        "User ID already exists."
                    );
                }
            }


            // =================================================
            // OPTIONAL PASSWORD
            // =================================================

            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                if (NewPassword.Length < 8 ||
                    NewPassword.Length > 16)
                {
                    ModelState.AddModelError(
                        "NewPassword",
                        "Password must be between 8 and 16 characters."
                    );
                }

                if (!Regex.IsMatch(
                    NewPassword,
                    "[A-Z]"))
                {
                    ModelState.AddModelError(
                        "NewPassword",
                        "Password must contain an uppercase letter."
                    );
                }

                if (!Regex.IsMatch(
                    NewPassword,
                    "[a-z]"))
                {
                    ModelState.AddModelError(
                        "NewPassword",
                        "Password must contain a lowercase letter."
                    );
                }

                if (!Regex.IsMatch(
                    NewPassword,
                    "[0-9]"))
                {
                    ModelState.AddModelError(
                        "NewPassword",
                        "Password must contain a number."
                    );
                }

                if (!Regex.IsMatch(
                    NewPassword,
                    @"[^a-zA-Z0-9]"))
                {
                    ModelState.AddModelError(
                        "NewPassword",
                        "Password must contain a special symbol."
                    );
                }
            }


            // =================================================
            // VALIDATION FAILED
            // =================================================

            if (!ModelState.IsValid)
            {
                admin.AdminName = AdminName;
                admin.UserId = UserId;

                return View(admin);
            }


            // =================================================
            // STORE OLD USER ID
            // =================================================

            string oldUserId =
                admin.UserId;


            // =================================================
            // UPDATE ADMIN
            // STATUS REMAINS 1
            // =================================================

            admin.AdminName =
                AdminName;

            admin.UserId =
                UserId;


            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                admin.PasswordHash =
                    AdminPasswordHasher.HashPassword(
                        NewPassword
                    );
            }


            await _db.SaveChangesAsync();


            // =================================================
            // UPDATE LOGIN COOKIE
            // =================================================

            string? loggedInUserId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (!string.IsNullOrWhiteSpace(
                    loggedInUserId) &&
                loggedInUserId.Equals(
                    oldUserId,
                    StringComparison.OrdinalIgnoreCase))
            {
                var claims = new List<Claim>
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        admin.UserId
                    ),

                    new Claim(
                        ClaimTypes.Name,
                        admin.AdminName
                    ),

                    new Claim(
                        ClaimTypes.Role,
                        "Admin"
                    )
                };

                var identity =
                    new ClaimsIdentity(
                        claims,
                        CookieAuthenticationDefaults
                            .AuthenticationScheme
                    );

                var principal =
                    new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults
                        .AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc =
                            DateTimeOffset.UtcNow.AddHours(8)
                    }
                );
            }


            TempData["SuccessMessage"] =
                "Admin details updated successfully.";


            return RedirectToAction(
                nameof(Index)
            );
        }


        // =====================================================
        // DELETE / DEACTIVATE ADMIN
        // SOFT DELETE
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int adminId)
        {
            var admin =
                await _db.Admins
                    .FirstOrDefaultAsync(
                        x => x.AdminId == adminId
                    );


            if (admin == null)
            {
                TempData["ErrorMessage"] =
                    "Admin not found.";

                return RedirectToAction(
                    nameof(Index)
                );
            }


            // =================================================
            // DON'T ALLOW CURRENT ADMIN TO DEACTIVATE THEMSELF
            // =================================================

            string? currentUserId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );


            if (!string.IsNullOrWhiteSpace(currentUserId) &&
                admin.UserId.Equals(
                    currentUserId,
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] =
                    "You cannot deactivate the currently logged-in admin.";

                return RedirectToAction(
                    nameof(Index)
                );
            }


            // =================================================
            // SOFT DELETE
            // 1 = ACTIVE
            // 0 = NOT ACTIVE
            // =================================================

            admin.Status = 0;


            await _db.SaveChangesAsync();


            TempData["SuccessMessage"] =
                $"Admin '{admin.AdminName}' has been deactivated.";


            return RedirectToAction(
                nameof(Index)
            );
        }
    }
}