using FreightFlow.Models;
using FreightFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FreightFlow.Web.Controllers
{
    public class LoadsController : Controller
    {
        private readonly FreightStore store;

        public LoadsController(FreightStore store)
        {
            this.store = store;
        }

        public IActionResult Index(CargoType? type, decimal? minRate)
        {
            ViewBag.Type = type;
            ViewBag.MinRate = minRate;
            List<Load> loads = store.Broker.GetAvailableLoads(type, minRate ?? 0);
            return View(loads);
        }

        [HttpPost]
        public IActionResult Add(string from, string to, double distance, double weight,
                                 CargoType type, decimal shipperPrice)
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
            {
                TempData["Error"] = "Вкажіть звідки і куди";
                return RedirectToAction("Index");
            }

            try
            {
                Load load = store.Broker.PostLoad(from.Trim(), to.Trim(), distance, weight, type, shipperPrice);
                store.Save();
                TempData["Message"] = $"Вантаж #{load.Id} викладено, ставка для перевізника {Ui.Money(load.CarrierRate)}";
            }
            catch (ArgumentException)
            {
                TempData["Error"] = "Відстань, вага і ціна мають бути більше 0";
            }

            return RedirectToAction("Index");
        }
    }
}
