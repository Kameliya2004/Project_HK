using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HealthyKitchen.Data;
using HealthyKitchen.Models;

namespace HealthyKitchen.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly IWebHostEnvironment _iWebHost;

        public ProductsController(ApplicationDbContext context, SignInManager<User> signInManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _signInManager = signInManager;
            _iWebHost = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            TempData.Keep("OrderActive");
            var applicationDbContext = _context.Products;
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            TempData.Keep("OrderActive");

            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            ProductVM model = new ProductVM()
            {
                Id=product.Id,
                Title = product.Title,
                Description = product.Description,
                PictureFile = product.PictureFile,
                Picture = product.Picture,
                Price = product.Price,
                Quantity = product.Quantity
            };
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ProductVM model = new ProductVM();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Quantity,Description,Picture,PictureFile,Price")] ProductVM product)
        {
            if (ModelState.IsValid)
            {
                string file = Path.GetFileNameWithoutExtension(product.PictureFile.FileName);
                string ext = Path.GetExtension(product.PictureFile.FileName);
                product.Picture = file + DateTime.Now.ToString("mmss") + ext;
                string path = "wwwroot/images/" + product.Picture;
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await product.PictureFile.CopyToAsync(fileStream);
                }

                ProductVM model = new ProductVM();
            }
            Product dbModel = new Product
            {
                Title = product.Title,
                Description = product.Description,
                PictureFile = product.PictureFile,
                Picture = product.Picture,
                Price = product.Price,
                Quantity = product.Quantity
            };
            _context.Add(dbModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ProductVM model = new ProductVM
            {
                Title = product.Title,
                Description = product.Description,
                PictureFile = product.PictureFile,
                Picture = product.Picture,
                Price = product.Price,
                Quantity = product.Quantity
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Quantity,Description,PictureFile,Picture,Price")] ProductVM product, IFormFile updateImage)
        {
            Product modelToDb = await _context.Products.FindAsync(id);
            if (modelToDb == null)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                return View();
            }
            try
            {
                _context.Products.Remove(modelToDb);
                FileInfo fileInfo = new FileInfo(product.Picture);
                if(fileInfo.Exists)
                {
                    System.IO.File.Delete(product.Picture);
                    fileInfo.Delete();
                }
                string file = Path.GetFileNameWithoutExtension(updateImage.FileName);
                string ext = Path.GetExtension(updateImage.FileName);
                product.Picture = file + DateTime.Now.ToString("mmss") + ext;
                string path = "wwwroot/images/" + product.Picture;
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await updateImage.CopyToAsync(fileStream);
                }
                modelToDb.Title = product.Title;
                modelToDb.Description = product.Description;
                modelToDb.PictureFile = product.PictureFile;
                modelToDb.Picture = product.Picture;
                modelToDb.Price = product.Price;
                modelToDb.Quantity = product.Quantity;
                _context.Update(modelToDb);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(modelToDb.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("Details", new { id = id });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
