using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using HealthyKitchen.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HealthyKitchen.Models
{
    public class ProductVM
    {
        [Key]
        public int Id { get; set; }


        [Required(ErrorMessage = "This is mandatory!")]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Picture { get; set; }

        public IFormFile PictureFile { get; set; }


        [Required(ErrorMessage = "This is mandatory!")]
        public double Price { get; set; }
        public int Quantity { get; set; }
        public string UserId { get; set; }
    }
}
