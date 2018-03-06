using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Models
{
    /*
     * The Id property is the entity key. Clients can query entities by key. 
     * For example, to get the product with ID of 5, the URI is /Products(5). 
     * The Id property will also be the primary key in the back-end database.
     */
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }

        // Navigation Property
        [ForeignKey("Supplier")]
        public int? SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}