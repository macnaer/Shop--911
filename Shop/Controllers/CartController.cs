using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Shop.Data;
using Shop.Models;
using Shop.Models.ViewModels;
using Shop.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Shop.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;

        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }

        public CartController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost()
        {
            return RedirectToAction(nameof(Order));
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Order")]
        public async Task<IActionResult> OrderPost(ProductUserVM productUserVM)
        {
            var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                + "templates" + Path.DirectorySeparatorChar.ToString()
                + "OrderConfirmation.html";

            var Subject = "New order";
            string HtmlBody = "";
            using (StreamReader sr = System.IO.File.OpenText(PathToTemplate))
            {
                HtmlBody = sr.ReadToEnd();
            }


            StringBuilder productListSB = new StringBuilder();
            foreach(var item in productUserVM.ProductList)
            {
                productListSB.Append($" - Name: {item.Name} <span style='font-size: 14px'>ID : {item.Id})</span>");
            }

            string messageBody = string.Format(
                HtmlBody,
                productUserVM.AppUser.FullName,
                productUserVM.AppUser.Email,
                productUserVM.AppUser.PhoneNumber,
                productListSB.ToString());


            await _emailSender.SendEmailAsync(ENV.EmailAdmin, Subject, messageBody);
                return RedirectToAction(nameof(OrderConfirmation));
        }

        public IActionResult OrderConfirmation()
        {
            HttpContext.Session.Clear();
            return View();
        }
    }
}
