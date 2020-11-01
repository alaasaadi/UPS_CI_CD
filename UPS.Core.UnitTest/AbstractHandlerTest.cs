using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using UPS.Core.Exceptions;
using UPS.Core.Handlers;
using UPS.Core.Models;

namespace UPS.Core.UnitTest
{
    [TestClass]
    public class AbstractHandlerTest
    {
        // Test abstract shared functionality using concrete class (EmployeeHandler)
        #region CRUD
        [TestMethod]
        public async Task Create_PostNewEmployee_ReturnsEmployeeObjectWithId()
        {
            Employee employee = new Employee()
            {
                Name = "Alaa Al Saadi -Test -  [Create_PostNewEmployee_ReturnsEmployeeObject]",
                Email = $"Alaa@{Guid.NewGuid()}",
                Gender = Gender.Male,
                Status = Status.Active
            };
            var emp = await new EmployeeHandler().CreateAsync(employee);

            Assert.IsTrue(emp != null && emp.Id > 0);
        }

        [TestMethod]
        public async Task Update_ModifyEmployeeBySingleUser_ReturnsTrue()
        {
            var handler = new EmployeeHandler();
            Employee employee = new Employee()
            {
                Name = "Alaa Al Saadi -Test - ",
                Email = $"Alaa@{Guid.NewGuid()}",
                Gender = Gender.Male,
                Status = Status.Active
            };

            var orginalEmployee = await handler.CreateAsync(employee);

            var modifiedEmployee = new Employee()
            {
                Id = orginalEmployee.Id,
                Name = $"{orginalEmployee.Name} [Update_ModifyEmployee_ReturnsTrue]",
                Email = orginalEmployee.Email,
                Gender = Gender.Female,
                Status = Status.Inactive
            };

            var result = await handler.UpdateAsync(modifiedEmployee, orginalEmployee);

            Assert.IsTrue(result);
        }

        [TestMethod]
        [ExpectedException(typeof(DataConcurrencyException))]
        public async Task Update_ModifyEmployeeConcurrently_ThrowsDataConcurrencyException()
        {
            var handler = new EmployeeHandler();
            Employee employee = new Employee()
            {
                Name = "Alaa Al Saadi -Test - [Update_ModifyEmployeeByMultiUserConcurrently_ThrowDataConcurrencyException]",
                Email = $"Alaa@{Guid.NewGuid()}",
                Gender = Gender.Male,
                Status = Status.Active
            };

            var orginalEmployee = await handler.CreateAsync(employee);

            var modifiedEmployee = new Employee()
            {
                Id = orginalEmployee.Id,
                Name = $"{orginalEmployee.Name} DataConcurrencyException",
                Email = orginalEmployee.Email,
                Gender = Gender.Female,
                Status = Status.Inactive
            };

            orginalEmployee.UpdateTime = DateTime.Now.AddMinutes(-1);
            await handler.UpdateAsync(modifiedEmployee, orginalEmployee);
        }

        [TestMethod]
        public async Task Delete_DeleteEmployee_ReturnsTrue()
        {
            var handler = new EmployeeHandler();
            Employee employee = new Employee()
            {
                Name = "Alaa Al Saadi -Test - ",
                Email = $"Alaa@{Guid.NewGuid()}",
                Gender = Gender.Male,
                Status = Status.Active
            };

            employee = await handler.CreateAsync(employee);
            var result = await handler.DeleteAsync(employee);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ReadPage_GetByPageNumber_ReturnsListOfEmployees()
        {
            var handler = new EmployeeHandler();
            var list = await handler.ReadPageAsync(2);

            Assert.IsTrue(list.Count() > 0 && handler.Pager.CurrentPage == 2);
        }

        [TestMethod]
        public async Task Read_GetEmployee_ReturnsEmployeeObject()
        {
            var handler = new EmployeeHandler();
            Employee employee = new Employee()
            {
                Name = "Alaa Al Saadi -Test -  [Read_GetEmployee_ReturnsEmployeeObject]",
                Email = $"Alaa@{Guid.NewGuid()}",
                Gender = Gender.Male,
                Status = Status.Active
            };

            employee = await handler.CreateAsync(employee);
            var result = await handler.ReadAsync(employee);

            Assert.IsTrue(result != null && result.Id > 0);
        }
        #endregion

        #region Filter
        [TestMethod]
        public void ClearFilter_ClearFilter_SetFilterPropertyToEmpty()
        {
            var handler = new EmployeeHandler();
            Employee employee = new Employee()
            {
                Id = 55,
                Email = "a",
                Name = "a",
                Status = Status.Inactive,
                Gender = Gender.Female
            };

            handler.SetFilter(employee);
            handler.ClearFilter();
            Assert.IsTrue(string.IsNullOrEmpty(handler.Filter));
        }

        [TestMethod]
        public async Task SetFilter_FilterByPropertyHasSearchableField_SetFilterPropertyAndReturnsListOfFilteredEmployee()
        {
            var nonFilteredHandler = new EmployeeHandler();
            var filteredHandler = new EmployeeHandler();
            Employee employee = new Employee()
            {
                Status = Status.Inactive
            };

            filteredHandler.SetFilter(employee);
            var filteredResult = await filteredHandler.ReadPageAsync();

            await nonFilteredHandler.ReadPageAsync();

            Assert.IsTrue(!string.IsNullOrEmpty(filteredHandler.Filter)
                           && filteredHandler.Pager.TotalRecords < nonFilteredHandler.Pager.TotalRecords
                           && filteredResult.Count() > 0);
        }

        [TestMethod]
        public async Task SetFilter_FilterByPropertyHasNoSearchableFieldAttribute_KeepsFilterPropertyEmptyAndReturnsNonFilteredListOfEmployee()
        {
            var nonFilteredHandler = new EmployeeHandler();
            var filteredHandler = new EmployeeHandler();
            Employee employee = new Employee()
            {
                CreateTime = DateTime.Now
            };

            filteredHandler.SetFilter(employee);
            var filteredResult = await filteredHandler.ReadPageAsync();

            await nonFilteredHandler.ReadPageAsync();

            Assert.IsTrue(string.IsNullOrEmpty(filteredHandler.Filter)
                           && filteredHandler.Pager.TotalRecords == nonFilteredHandler.Pager.TotalRecords);
        }

        #endregion
    }
}
