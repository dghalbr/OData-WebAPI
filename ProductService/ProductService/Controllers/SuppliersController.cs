using ProductService.Models;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;

namespace Supplierservice.Controllers
{
    public class SuppliersController : ODataController
    {
        ProductsContext db = new ProductsContext();

        private bool SupplierExists(int key)
        {
            return db.Suppliers.Any(s => s.Id == key);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [EnableQuery]
        public IQueryable<Supplier> Get()
        {
            return db.Suppliers;
        }

        [EnableQuery]
        public SingleResult<Supplier> Get([FromODataUri] int key)
        {
            IQueryable<Supplier> result = db.Suppliers.Where(s => s.Id == key);
            return SingleResult.Create(result);
        }

        /* 
         * GET /Suppliers(1)/Products 
         * 
         * A navigation property can also return a collection. 
         * The following code gets the products for a supplier.
         * 
         */
        [EnableQuery]
        public IQueryable<Product> GetProducts([FromODataUri] int key)
        {
            return db.Suppliers.Where(m => m.Id.Equals(key)).SelectMany(m => m.Products);
        }

        public async Task<IHttpActionResult> Post(Supplier supplier)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            db.Suppliers.Add(supplier);
            await db.SaveChangesAsync();
            return Created(supplier);
        }

        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<Supplier> supplier)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await db.Suppliers.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            supplier.Patch(entity);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SupplierExists(key))
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

        public async Task<IHttpActionResult> Put([FromODataUri] int key, Supplier update)
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
                if (!SupplierExists(key))
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

        public async Task<IHttpActionResult> Delete([FromODataUri] int key)
        {
            var product = await db.Suppliers.FindAsync(key);
            if (product == null)
            {
                return NotFound();
            }
            db.Suppliers.Remove(product);
            await db.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}