using System.Data.Entity;
namespace ProductService.Models
{
    public class ProductsContext : DbContext
    {
        //name=ProductsContext gives the name of the connection string in the Web.config
        public ProductsContext() : base("name=ProductsContext")
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
    }
}