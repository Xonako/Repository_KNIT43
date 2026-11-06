using FreightFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FreightFlow.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly FreightStore store;

        public HomeController(FreightStore store)
        {
            this.store = store;
        }

        public IActionResult Index()
        {
            return View(store);
        }

        [HttpPost]
        public IActionResult Reset()
        {
            store.Reset();
            TempData["Message"] = "Дані скинуто до початкових";
            return RedirectToAction("Index");
        }
    }
}
