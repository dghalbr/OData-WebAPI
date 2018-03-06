using Microsoft.OData;
using Microsoft.OData.UriParser;
using ProductService.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.OData;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Properties;
using Microsoft.OData.Edm;
using System.Web.OData.Routing.Template;

namespace ProductService.Controllers
{
    public class ProductsController : ODataController
    {
        /*
         * The controller uses the ProductsContext class to access the database using EF. 
         * Notice that the controller overrides the Dispose method to dispose of the ProductsContext.
         */

        ProductsContext db = new ProductsContext();
        private bool ProductExists(int key)
        {
            //Product ID is the OData Key as well as the Key in the DB
            return db.Products.Any(p => p.Id == key);
        }
        
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        /* 
         * The parameterless version of the Get method returns the entire Products collection.
         * The Get method with a key parameter looks up a product by its key (in this case, the Id property). 
         * The [EnableQuery] attribute enables clients to modify the query, by using query options such as $filter, $sort, and $page.
         */
        [EnableQuery]
        public IQueryable<Product> Get()
        {
            return db.Products;
        }

        [EnableQuery]
        public SingleResult<Product> Get([FromODataUri] int key)
        {
            IQueryable<Product> result = db.Products.Where(p => p.Id == key);
            return SingleResult.Create(result);
        }

        /* 
         * GET /Products(1)/Supplier 
         * 
         * This method uses a default naming convention
         * - Method name: GetX, where X is the navigation property.
         * - Parameter name: key
         * 
         * If you follow this naming convention, Web API automatically maps the 
         * HTTP request to the controller method.
         * 
         */
        [EnableQuery]
        public SingleResult<Supplier> GetSupplier([FromODataUri] int key)
        {
            var result = db.Products.Where(m => m.Id == key).Select(m => m.Supplier);
            return SingleResult.Create(result);
        }
        
        /*
         * Create/Add a new Entity.
         */
        public async Task<IHttpActionResult> Post(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            db.Products.Add(product);
            await db.SaveChangesAsync();
            return Created(product);
        }

        /* 
         * OData supports two different semantics for updating an entity, PATCH and PUT.
         * - PATCH performs a partial update. The client specifies just the properties to update.
         * - PUT replaces the entire entity.
         * 
         * The disadvantage of PUT is that the client must send values for all of the properties in the entity, 
         * including values that are not changing. The OData spec states that PATCH is preferred.
        */
        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<Product> product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await db.Products.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            product.Patch(entity);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Updated(entity);
        }

        public async Task<IHttpActionResult> Put([FromODataUri] int key, Product update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (key != update.Id)
            {
                return BadRequest();
            }
            db.Entry(update).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Updated(update);
        }

        /*
         * Delete an Entity from the database.
         */
        public async Task<IHttpActionResult> Delete([FromODataUri] int key)
        {
            var product = await db.Products.FindAsync(key);
            if (product == null)
            {
                return NotFound();
            }
            db.Products.Remove(product);
            await db.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        /*
         * To add a relationship, the client sends a POST or PUT request to this address.
         * - PUT if the navigation property is a single entity, such as Product.Supplier.
         * - POST if the navigation property is a collection, such as Supplier.Products.
         * 
         * ex: PUT http://myproductservice.example.com/Products(6)/Supplier/$ref
         * 
         * In this example, the client sends a PUT request to /Products(6)/Supplier/$ref, 
         * which is the $ref URI for the Supplier of the product with ID = 6. 
         * If the request succeeds, the server sends a 204 (No Content) response
         */

        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            var product = await db.Products.SingleOrDefaultAsync(p => p.Id == key);
            if (product == null)
            {
                return NotFound();
            }
            switch (navigationProperty)
            {
                case "Supplier":
                    // Note: The code for GetKeyFromUri is shown later in this topic.
                    var relatedKey = Helpers.GetKeyFromUri<int>(Request, link);
                    var supplier =  await db.Suppliers.SingleOrDefaultAsync(f => f.Id == relatedKey);
                    if (supplier == null)
                    {
                        return NotFound();
                    }

                    product.Supplier = supplier;
                    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await db.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key, 
        [FromODataUri] string relatedKey, string navigationProperty)
    {
        var supplier = await db.Suppliers.SingleOrDefaultAsync(p => p.Id == key);
        if (supplier == null)
        {
            return StatusCode(HttpStatusCode.NotFound);
        }

        switch (navigationProperty)
        {
            case "Products":
                var productId = Convert.ToInt32(relatedKey);
                var product = await db.Products.SingleOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                {
                    return NotFound();
                }
                product.Supplier = null;
                break;
            default:
                return StatusCode(HttpStatusCode.NotImplemented);

        }
        await db.SaveChangesAsync();

        return StatusCode(HttpStatusCode.NoContent);
    }
    }

    public static class Helpers
    {
        public static TKey GetKeyFromUri<TKey>(HttpRequestMessage request, Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            var urlHelper = request.GetUrlHelper() ?? new UrlHelper(request);

            var routeName = request.ODataProperties().RouteName;
            ODataRoute oDataRoute = request.GetConfiguration().Routes[routeName] as ODataRoute;
            var prefixName = oDataRoute.RoutePrefix;
            var requestUri = request.RequestUri.ToString();

            string serviceRoot = requestUri.Substring(0, requestUri.IndexOf(prefixName) + prefixName.Length);

            var odataPath = request.ODataProperties().Path;

            var keySegment = odataPath.Segments.OfType<KeySegmentTemplate>().LastOrDefault().Segment.Keys.LastOrDefault();
            
            if (keySegment.Key == null)
            {
                throw new InvalidOperationException("The link does not contain a key.");
            }
            var value = ODataUriUtils.ConvertFromUriLiteral(keySegment.Value.ToString(), ODataVersion.V4);
            return (TKey)value;
        }
    }
}
 