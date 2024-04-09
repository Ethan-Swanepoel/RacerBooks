using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RacerBooks.Models;

namespace RacerBooks.Controllers
{
    public class ItemsController : Controller
    {
        private readonly RacerbooksContext _context;

        public ItemsController(RacerbooksContext context)
        {
            _context = context;
        }

        // GET: Items
        public async Task<IActionResult> Index(string searchBy, string search)
        {
            return View(await _context.Items.ToListAsync());
        }


        public async Task<IActionResult> Search(string searchBy, string search)
        {

            if (searchBy == "ItemPrice")
            {
                return View(_context.Items.Where(x => x.ItemPrice.ToString() == search || search == null).ToList());
            }
            else
            {
                return View(_context.Items.Where(x => x.ItemName.StartsWith(search) || search == null).ToList());
            }
        }



        // GET: Items/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .FirstOrDefaultAsync(m => m.ItemCode == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // GET: Items/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Items/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItemCode,Stock,ItemName,ItemPrice,ItemDetails")] Item item)
        {
            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Search));
            }
            return View(item);
        }


        private bool ItemExists(string id)
        {
            return _context.Items.Any(e => e.ItemCode == id);
        }
    }
}
