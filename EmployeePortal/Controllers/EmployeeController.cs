using EmployeePortal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeePortal.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly EmployeeService _employeeService;

        public EmployeeController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }



        private bool IsAuthenticated()
        {
            var username = HttpContext.Session.GetString("username");
            var role = HttpContext.Session.GetString("role");

            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(role);
        }

        private string GetUserRole()
        {
            return HttpContext.Session.GetString("role");
        }

        [HttpGet]
        public async Task<IActionResult> List(
      [FromQuery] string SearchTerm,
      [FromQuery] string SelectedDepartment,
      [FromQuery] string SelectedType,
      [FromQuery] int PageNumber = 1,
      [FromQuery] int PageSize = 5)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var (employees, totalCount) = await _employeeService.GetEmployees(
                SearchTerm, SelectedDepartment, SelectedType, PageNumber, PageSize);

            var viewModel = new EmployeeListViewModel
            {
                Employees = employees,
                PageNumber = PageNumber,
                PageSize = PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / PageSize),
                SearchTerm = SearchTerm,
                SelectedDepartment = SelectedDepartment,
                SelectedType = SelectedType
            };

            GetSelectLists();
            ViewBag.PageSizeOptions = new SelectList(new List<int> { 3, 5, 10, 15, 20, 25 }, PageSize);
            Console.WriteLine("Session username: " + HttpContext.Session.GetString("username"));
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (GetUserRole() != "Admin")
            {
                TempData["Error"] = "You do not have permission to create employees.";
                return RedirectToAction("List");
            }

            GetSelectLists();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Employee employee)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (GetUserRole() != "Admin")
            {
                TempData["Error"] = "You do not have permission to create employees.";
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                await _employeeService.CreateEmployee(employee);
                return RedirectToAction("Success", new { id = employee.Id });
            }

            GetSelectLists();
            return View(employee);
        }

        public async Task<IActionResult> Success([FromRoute] int id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var employee = await _employeeService.GetEmployeeById(id);
            if (employee == null) return NotFound();

            return View(employee);
        }

        public async Task<IActionResult> Details([FromRoute] int id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var employee = await _employeeService.GetEmployeeById(id);
            if (employee == null) return NotFound();

            return View(employee);
        }

        [HttpGet]
        public async Task<IActionResult> Update([FromRoute] int id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (GetUserRole() != "Admin")
            {
                TempData["Error"] = "You do not have permission to update employees.";
                return RedirectToAction("List");
            }

            var employee = await _employeeService.GetEmployeeById(id);
            if (employee == null) return NotFound();

            GetSelectLists();
            return View(employee);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromForm] Employee employee)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (GetUserRole() != "Admin")
            {
                TempData["Error"] = "You do not have permission to update employees.";
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                await _employeeService.UpdateEmployee(employee);
                TempData["Message"] = $"Employee with ID {employee.Id} and Name {employee.FullName} has been updated.";
                return RedirectToAction("List");
            }

            GetSelectLists();
            return View(employee);
        }

        [HttpGet]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (GetUserRole() != "Admin")
            {
                TempData["Error"] = "You do not have permission to delete employees.";
                return RedirectToAction("List");
            }

            var employee = await _employeeService.GetEmployeeById(id);
            if (employee == null) return NotFound();

            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed([FromRoute] int id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (GetUserRole() != "Admin")
            {
                TempData["Error"] = "You do not have permission to delete employees.";
                return RedirectToAction("List");
            }

            var employee = await _employeeService.GetEmployeeById(id);
            if (employee == null) return NotFound();

            await _employeeService.DeleteEmployee(id);
            TempData["Message"] = $"Employee with ID {id} and Name {employee.FullName} has been deleted.";

            return RedirectToAction("List");
        }

        [HttpGet]
        public JsonResult GetPositions(Department department)
        {
            if (!IsAuthenticated())
                return Json(new List<string>());

            var positions = new Dictionary<Department, List<string>>
            {
                { Department.IT, new List<string> { "Software Developer", "System Administrator", "Network Engineer" } },
                { Department.HR, new List<string> { "HR Specialist", "HR Manager", "Talent Acquisition Coordinator" } },
                { Department.Sales, new List<string> { "Sales Executive", "Sales Manager", "Account Executive" } },
                { Department.Admin, new List<string> { "Office Manager", "Executive Assistant", "Receptionist" } }
            };

            var result = positions.ContainsKey(department) ? positions[department] : new List<string>();
            return Json(result);
        }

        private void GetSelectLists()
        {
            ViewBag.DepartmentOptions = new SelectList(Enum.GetValues(typeof(Department)).Cast<Department>());
            ViewBag.EmployeeTypeOptions = new SelectList(Enum.GetValues(typeof(EmployeeType)).Cast<EmployeeType>());
        }
    }
}
