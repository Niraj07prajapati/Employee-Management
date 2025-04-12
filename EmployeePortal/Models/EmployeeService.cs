using EmployeePortal.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeePortal.Models
{
    public class EmployeeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Employee> Employees, int TotalCount)> GetEmployees(
            string SearchTerm,
            string SelectedDepartment,
            string SelectedType,
            int PageNumber,
            int PageSize)
        {
            var filteredEmployees = _context.Employees.AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                filteredEmployees = filteredEmployees.Where(p =>
                    p.FullName.ToLower().Contains(SearchTerm.ToLower()));
            }

            if (!string.IsNullOrEmpty(SelectedDepartment) &&
                Enum.TryParse(SelectedDepartment, out Department department))
            {
                filteredEmployees = filteredEmployees.Where(p => p.Department == department);
            }

            if (!string.IsNullOrEmpty(SelectedType) &&
                Enum.TryParse(SelectedType, out EmployeeType type))
            {
                filteredEmployees = filteredEmployees.Where(p => p.Type == type);
            }

            int totalCount = await filteredEmployees.CountAsync();

            var employees = await filteredEmployees
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return (employees, totalCount);
        }

        public async Task<Employee?> GetEmployeeById(int id)
        {
            return await _context.Employees.FindAsync(id);
        }

        public async Task CreateEmployee(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEmployee(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
        }
    }
}
