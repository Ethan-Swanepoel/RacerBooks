using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using RacerBooks.Models;

namespace RacerBooks.Controllers
{
    public class CartsController : Controller
    {
        private readonly RacerbooksContext _context;

        public CartsController(RacerbooksContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> AddToCart(string itemcode)
        {
            if (itemcode == null)
            {
                return NotFound();
            }

            var email = HttpContext.Session.GetString("LoggedInUser");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Login");
            }

            // Find the item in the database
            var item = await _context.Items.FindAsync(itemcode);
            if (item == null)
            {
                TempData["ErrorMessage"] = "Item not found.";
                return RedirectToAction("Search", "Items");
            }

            // Check if the item is in stock
            if (item.Stock <= 0)
            {
                TempData["ErrorMessage"] = "Item is out of stock.";
                return RedirectToAction("Search", "Items");
            }

            // Attempt to find an existing cart item for the current user
            var existingCartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.ItemCode == itemcode && c.Email == email);

            if (existingCartItem != null)
            {
                // Item exists in cart, increment quantity
                existingCartItem.Quantity += 1;
                _context.Carts.Update(existingCartItem);
            }
            else
            {
                // Item does not exist in cart, add it
                var newCartItem = new Cart
                {
                    ItemCode = itemcode,
                    Email = email,
                    Quantity = 1, // Set the initial quantity to 1
                    ItemCodeNavigation = item,
                    EmailNavigation = await _context.Users.FindAsync(email)
                };
                await _context.Carts.AddAsync(newCartItem);
            }

            // Decrement the stock of the item
            item.Stock -= 1; // Decrement the stock by the quantity added to the cart
            _context.Items.Update(item);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Item added to the cart.";
            return RedirectToAction("Search", "Items");
        }

        public async Task<IActionResult> ViewCart()
        {
            var email = HttpContext.Session.GetString("LoggedInUser");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Login");
            }

            var cartItems = await _context.Carts
                .Where(c => c.Email == email)
                .Include(c => c.ItemCodeNavigation)
                .ToListAsync();

            // Calculate the total price of the cart
            decimal totalPrice = cartItems.Sum(item => item.Quantity * item.ItemCodeNavigation.ItemPrice);

            // Pass the total price to the view using ViewData
            ViewData["TotalPrice"] = totalPrice;

            return View(cartItems);
        }



        // GET: Carts
        public async Task<IActionResult> Index()
        {
            var ebbooksContext = _context.Carts.Include(c => c.ItemCodeNavigation).Include(c => c.EmailNavigation);
            return View(await ebbooksContext.ToListAsync());
        }

        // GET: Carts/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.ItemCodeNavigation)
                .Include(c => c.EmailNavigation)
                .FirstOrDefaultAsync(m => m.Email == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // GET: Carts/Create
        public IActionResult Create()
        {
            ViewData["ItemCode"] = new SelectList(_context.Items, "ItemCode", "ItemCode");
            ViewData["Email"] = new SelectList(_context.Users, "Email", "Email");
            return View();
        }

        // POST: Carts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItemCode,Email,Quantity")] Cart cart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ItemCode"] = new SelectList(_context.Items, "ItemCode", "ItemCode", cart.ItemCode);
            ViewData["Email"] = new SelectList(_context.Users, "Email", "Email", cart.Email);
            return View(cart);
        }

        // GET: Carts/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }
            ViewData["ItemCode"] = new SelectList(_context.Items, "ItemCode", "ItemCode", cart.ItemCode);
            ViewData["Email"] = new SelectList(_context.Users, "Email", "Email", cart.Email);
            return View(cart);
        }

        // POST: Carts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ItemCode,Email,Quantity")] Cart cart)
        {
            if (id != cart.Email)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartExists(cart.Email))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ItemCode"] = new SelectList(_context.Items, "ItemCode", "ItemCode", cart.ItemCode);
            ViewData["Email"] = new SelectList(_context.Users, "Email", "Email", cart.Email);
            return View(cart);
        }

        // GET: Carts/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.ItemCodeNavigation)
                .Include(c => c.EmailNavigation)
                .FirstOrDefaultAsync(m => m.Email == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // POST: Carts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CartExists(string id)
        {
            return _context.Carts.Any(e => e.Email == id);
        }
    }
}
