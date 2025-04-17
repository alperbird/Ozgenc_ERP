using DataAccessLayer.Data;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebClient.Models;

namespace WebClient.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AppDbContext _context;

        public CustomerController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10) //pagination
        {
            var totalItems = await _context.Customers.CountAsync();

            var items = await _context.Customers
                .Include(c => c.Country)
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new PagedResult<Customer>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = page,
                PageSize = pageSize
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create() //burası ülkeleri getirdiğimiz yer
        {
            ViewBag.Country = await _context.Countries.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Customer c)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Countries = await _context.Countries.ToListAsync();
                //return View(c);
            }

            c.Date = DateTime.Now;
            c.LastUpdated = DateTime.Now;

            await _context.Customers.AddAsync(c);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id) //güncellenecek müşteriyi getir
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            ViewBag.Country = await _context.Countries.ToListAsync();
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Customer c) //güncelleme işlemi
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Country = await _context.Countries.ToListAsync();
                //return View(c);
            }

            _context.Customers.Update(c);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> Delete(int id) //adı üstünde Delete aq
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound();

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        } 
    }
}
