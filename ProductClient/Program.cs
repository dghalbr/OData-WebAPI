using System;

namespace ProductClient
{
    class Program
    {
        // Get an entire entity set.
        static void ListAllProducts(Default.Container container)
        {
            foreach (var p in container.Products)
            {
                //TODO: Make it actually print out supplier properties instead of just the Name
                Console.WriteLine("{0} {1} {2} {3}", p.Name, p.Price, p.Category, p.Supplier.Name);
            }
        }

        static void AddProduct(Default.Container container)
        {
            var serviceResponse = container.SaveChanges();
            foreach (var operationResponse in serviceResponse)
            {
                Console.WriteLine("Response: {0}", operationResponse.StatusCode);
            }
        }

        static void Main(string[] args)
        {
            // TODO: Replace with your local URI.
            string serviceUri = "http://localhost:18003/";
            var container = new Default.Container(new Uri(serviceUri));

            var supplier = ProductService.Models.Supplier.CreateSupplier(1);
            supplier.Name = "Duncan";
            container.AddToSuppliers(supplier);

            var product = new ProductService.Models.Product()
            {
                Name = "Yo-yo",
                Category = "Toys",
                Price = 4.95M,
                Supplier = supplier,
                SupplierId = supplier.Id
            };
            container.AddToProducts(product);

            AddProduct(container);

            ListAllProducts(container);
            Console.ReadKey();
        }
    }
}