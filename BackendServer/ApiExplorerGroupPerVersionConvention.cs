using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Linq;

namespace BackendServer
{
    public class ApiExplorerGroupPerVersionConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var typeinfo = controller.ControllerType;
            var skip = typeinfo.GetCustomAttributes(true)
                .OfType<ApiExplorerSettingsAttribute>().Any(x => x.IgnoreApi);

            if (skip)
                return;

            var controllerNamespace = typeinfo.Namespace;
            var apiVersion = controllerNamespace.Split('.')[^2].ToLower();
            controller.ApiExplorer.GroupName = apiVersion;
        }
    }
}
