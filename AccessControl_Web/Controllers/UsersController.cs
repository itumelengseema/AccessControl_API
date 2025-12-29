using AccessControl_API.Models.DTO;
using AccessControl_Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace AccessControl_Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var response = await _userService.GetAllUsersAsync();

            if (response?.Success == true && response.Data != null)
            {
                return View(response.Data);
            }

            TempData["Error"] = response?.Message ?? "Failed to load users";
            return View(new List<UserDTO>());
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateUpdateDTO userDto)
        {
            if (!ModelState.IsValid)
            {
                return View(userDto);
            }

            var response = await _userService.CreateUserAsync(userDto);

            if (response?.Success == true)
            {
                TempData["Success"] = "User created successfully";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = response?.Message ?? "Failed to create user";
            return View(userDto);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _userService.GetAllUsersAsync();
            
            if (response?.Success == true && response.Data != null)
            {
                var user = response.Data.FirstOrDefault(u => u.Id == id);
                if (user != null)
                {
                    var updateDto = new UserCreateUpdateDTO
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        IdentificationNumber = user.IdentificationNumber,
                        GroupId = user.GroupId
                    };
                    ViewBag.UserId = id;
                    return View(updateDto);
                }
            }

            TempData["Error"] = "User not found";
            return RedirectToAction(nameof(Index));
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserCreateUpdateDTO userDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserId = id;
                return View(userDto);
            }

            var response = await _userService.UpdateUserAsync(id, userDto);

            if (response?.Success == true)
            {
                TempData["Success"] = "User updated successfully";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = response?.Message ?? "Failed to update user";
            ViewBag.UserId = id;
            return View(userDto);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _userService.GetAllUsersAsync();
            
            if (response?.Success == true && response.Data != null)
            {
                var user = response.Data.FirstOrDefault(u => u.Id == id);
                if (user != null)
                {
                    return View(user);
                }
            }

            TempData["Error"] = "User not found";
            return RedirectToAction(nameof(Index));
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _userService.DeleteUserAsync(id);

            if (response?.Success == true)
            {
                TempData["Success"] = "User deleted successfully";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = response?.Message ?? "Failed to delete user";
            return RedirectToAction(nameof(Index));
        }
    }
}
