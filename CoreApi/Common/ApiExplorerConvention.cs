using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Linq;

namespace CoreApi.Common
{
    public class ApiExplorerConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (controller == null) {
                return;
            }

            // Get controller group name
            var controllerNamespace = controller.ControllerType.Namespace; // e.g. "CoreApi.Controller.Admin"
            var listNamespace = controllerNamespace.Split('.');
            string apiVersion = "test";
            if (listNamespace.Contains("Admin")) {
                apiVersion = "admin";
            } else if (listNamespace.Contains("Social")) {
                apiVersion = "social";
            } else if (listNamespace.Contains("Upload")) {
                apiVersion = "upload";
            }
            controller.ApiExplorer.GroupName = apiVersion;

            // Change controller name by last of namespace
            controller.ControllerName = listNamespace.Last();
        }
    }
}