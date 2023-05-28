using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HealthyKitchen.Data
{
    public class Product
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Picture { get; set; }

        [NotMapped]
        public IFormFile PictureFile { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }

        public ICollection<OrderDetails> OrderDetails { get; set; }
    }
}
