using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.WebHost.Models;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Сотрудники
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Role> _roleRepository;

        public EmployeesController(
            IRepository<Employee> employeeRepository,
            IRepository<Role> roleRepository)
        {
            _employeeRepository = employeeRepository;
            _roleRepository = roleRepository;
        }

        /// <summary>
        /// Получить данные всех сотрудников
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeShortResponse>>> GetEmployeesAsync()
        {
            var employees = await _employeeRepository.GetAllAsync();

            var employeesModelList = employees.Select(x =>
                new EmployeeShortResponse()
                {
                    Id = x.Id,
                    Email = x.Email,
                    FullName = x.FullName,
                });

            return Ok(employeesModelList);
        }

        /// <summary>
        /// Получить данные сотрудника по Id
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EmployeeResponse>> GetEmployeeByIdAsync(Guid id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);

            if (employee == null)
                return NotFound();

            var employeeModel = new EmployeeResponse()
            {
                Id = employee.Id,
                Email = employee.Email,
                Roles = employee.Roles.Select(x => new RoleItemResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description
                }),
                FullName = employee.FullName,
                AppliedPromocodesCount = employee.AppliedPromocodesCount
            };

            return Ok(employeeModel);
        }

        /// <summary>
        /// Удалить данные сотрудника по Id
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEmployeeByIdAsync(Guid id)
        {
            var result = await _employeeRepository.DeleteByIdAsync(id);

            if (result == false)
                return NotFound($"Employee with id: {id} not found");

            return NoContent();
        }

        /// <summary>
        /// Создать сотрудника
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateEmployeeAsync(EmployeeRequest employeeRequest)
        {
            var rolesOfNewEmployee = await GetRolesByIds(employeeRequest.RoleIds);
            if (!rolesOfNewEmployee.Any())
            {
                return NotFound($"Roles with ids: {string.Join(",", employeeRequest.RoleIds.Select(x => x))} not found");
            }

            var employee = new Employee()
            {
                FirstName = employeeRequest.FirstName,
                LastName = employeeRequest.LastName,
                Email = employeeRequest.Email,
                Roles = rolesOfNewEmployee.ToList()
            };

            var createdEmployeeId = await _employeeRepository.AddAsync(employee);

            return Ok(createdEmployeeId);
        }

        /// <summary>
        /// Изменить данные сотрудника
        /// </summary>
        /// <returns></returns>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Guid>> UpdateEmployeeAsync(Guid id, EmployeeRequest employeeRequest)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);

            if (employee == default)
            {
                return NotFound($"Employee with id: {id} not found");
            }

            var rolesOfUpdatedEmployee = await GetRolesByIds(employeeRequest.RoleIds);
            if (!rolesOfUpdatedEmployee.Any())
            {
                return NotFound($"Roles with ids: {string.Join(",", employeeRequest.RoleIds.Select(x => x))} not found");
            }

            employee.FirstName = employeeRequest.FirstName;
            employee.LastName = employeeRequest.LastName;
            employee.Email = employeeRequest.Email;
            employee.Roles = rolesOfUpdatedEmployee.ToList();

            await _employeeRepository.UpdateAsync(employee);

            return NoContent();
        }

        private async Task<IEnumerable<Role>> GetRolesByIds(IEnumerable<Guid> roleIds)
        {
            var allRoles = await _roleRepository.GetAllAsync();

            return allRoles.Where(x => roleIds.Contains(x.Id));
        }
    }
}