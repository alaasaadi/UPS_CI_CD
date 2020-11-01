using System;
using UPS.Core.Models;

namespace UPS.Core.Handlers
{
    public class EmployeeHandler : AbstractHandler<Employee>
    {
        protected override string GetApiSubAddress(Employee entity, HandlerActionType actionType)
        {
            switch (actionType)
            {
                case HandlerActionType.Read:
                case HandlerActionType.Update:
                case HandlerActionType.Delete:
                    return $"{ApiControllerName.users}/{entity.Id}";

                case HandlerActionType.ReadPage:
                case HandlerActionType.Create:
                    return $"{ApiControllerName.users}";

                default:
                    throw new NotImplementedException();
            }
        }

    }
}
