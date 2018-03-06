using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using ProductService.Models;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;

namespace ProductService
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Enable Filtering
            config.Filter();

            /*
             * This code does two things:
             *  - Creates an Entity Data Model (EDM).
             *  - Adds a route.
             *
             *  An EDM is an abstract model of the data. The EDM is used to create the service metadata document. 
             *  The ODataConventionModelBuilder class creates an EDM by using default naming conventions. 
             *  This approach requires the least code. If you want more control over the EDM, 
             *  you can use the ODataModelBuilder class to create the EDM by adding properties, keys, and navigation properties explicitly.
             *  
             *  A route tells Web API how to route HTTP requests to the endpoint. 
             *  To create an OData v4 route, call the MapODataServiceRoute extension method.
             *
             *  If your application has multiple OData endpoints, create a separate route for each. Give each route a unique route name and prefix.
             */
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Product>("Products");
            builder.EntitySet<Supplier>("Suppliers");
            config.MapODataServiceRoute(
                routeName: "ODataRoute",
                routePrefix: null,
                model: builder.GetEdmModel());
        }
    }
}
