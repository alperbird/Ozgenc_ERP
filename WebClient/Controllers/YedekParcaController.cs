using DataAccessLayer.Data;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebClient.Models;

namespace WebClient.Controllers
{
    public class YedekParcaController : Controller
    {
        private readonly AppDbContext _context;

        public YedekParcaController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            var totalItems = _context.Products.Count();

            var items = _context.Products
                .Include(p => p.ProductMachines)
                .ThenInclude(pm => pm.Machine)
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new PagedResult<Product>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = page,
                PageSize = pageSize
            };

            return View(model);
        } //pagination 

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Machines = _context.Machines.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.MakineAdi
            }).ToList();
            ViewBag.Sinif = GetSinif();
            return View();
        }

        [HttpPost] //Ekleme
        public async Task<IActionResult> Create(Product model, IFormFile foto, List<int> selectedMachines)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Machines = _context.Machines.Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.MakineAdi
                }).ToList();
                ViewBag.Sinif = GetSinif();
                //return View(model);
            }

            // FOTOĞRAF KAYDET
            if (foto != null && foto.Length > 0)
            {
                var dosyaAdi = Guid.NewGuid().ToString() + Path.GetExtension(foto.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/YedekParcaAssets", dosyaAdi);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await foto.CopyToAsync(stream);
                }

                model.ResimYolu = dosyaAdi;
            }

            // MAKİNE ATAMA
            if (selectedMachines != null && selectedMachines.Count > 0)
            {
                foreach (var machineId in selectedMachines)
                {
                    model.ProductMachines.Add(new ProductMachine
                    {
                        MakineId = machineId
                    });
                }
            }

            _context.Products.Add(model);
            await _context.SaveChangesAsync();

            TempData["success"] = "Parça başarıyla eklendi.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductMachines)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpGet] //güncelleme esnasında dolacak olan kısım
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductMachines)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Machines = _context.Machines.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.MakineAdi
            }).ToList();

            ViewBag.Sinif = GetSinif();
            ViewBag.SelectedMachines = product.ProductMachines.Select(pm => pm.MakineId).ToList();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // güncelleme kısmı
        public async Task<IActionResult> Edit(Product model, IFormFile foto, List<int> selectedMachines)
        {
            var product = await _context.Products
                .Include(p => p.ProductMachines)
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            if (product == null)
                return NotFound();

            // Güncelleme
            product.Sinif = model.Sinif;
            product.ParcaAdi = model.ParcaAdi;
            product.FiyatTL = model.FiyatTL;
            product.FiyatEUR = model.FiyatEUR;
            product.Aciklama = model.Aciklama;

            // FOTOĞRAF YENİLENDİ Mİ?
            if (foto != null && foto.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(foto.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/YedekParcaAssets", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await foto.CopyToAsync(stream);
                product.ResimYolu = fileName;
            }

            // ESKİ MAKİNELERİ SİL
            _context.ProductMachines.RemoveRange(product.ProductMachines);

            // YENİLERİ EKLE
            if (selectedMachines != null)
            {
                product.ProductMachines = selectedMachines.Select(mid => new ProductMachine
                {
                    MakineId = mid
                }).ToList();
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "Parça başarıyla güncellendi.";
            return RedirectToAction("Index");
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductMachines)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            _context.ProductMachines.RemoveRange(product.ProductMachines);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["success"] = "Parça başarıyla silindi.";
            return RedirectToAction("Index");
        }

        private List<SelectListItem> GetSinif()
        {
            return new List<SelectListItem>
    {
        new SelectListItem { Text = "MEKANİK", Value = "MEKANİK" },
        new SelectListItem { Text = "PNÖMATİK", Value = "PNÖMATİK" },
        new SelectListItem { Text = "ELEKTRİK", Value = "ELEKTRİK" },
        new SelectListItem { Text = "GENEL", Value = "GENEL" },
        new SelectListItem { Text = "-", Value = "-" }
    };
        }
    }
}

