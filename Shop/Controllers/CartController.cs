using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shop.Data;
using Shop.Models;
using Shop.Models.ViewModels;
using Shop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shop.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }

        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();

            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(ENV.SessinCart) != null && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(ENV.SessinCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(ENV.SessinCart);
            }

            List<int> productListInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> productList = _db.Product.Where(i => productListInCart.Contains(i.Id));

            return View(productList);
        }

        public IActionResult Remove(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(ENV.SessinCart) != null && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(ENV.SessinCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(ENV.SessinCart);
            }

            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(u => u.ProductId == id));
            HttpContext.Session.Set(ENV.SessinCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Order()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();

            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(ENV.SessinCart) != null && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(ENV.SessinCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(ENV.SessinCart);
            }

            List<int> productListInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> productList = _db.Product.Where(i => productListInCart.Contains(i.Id));

            ProductUserVM = new ProductUserVM()
            {
                AppUser = _db.AppUser.FirstOrDefault(u => u.Id == claim.Value),
                ProductList = productList.ToList()
            };


            return View(ProductUserVM);
        }
    }
}
