using FreightFlow.Vehicles;
using FreightFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FreightFlow.Web.Controllers
{
    public class TrucksController : Controller
    {
        private readonly FreightStore store;

        public TrucksController(FreightStore store)
        {
            this.store = store;
        }

        public IActionResult Index()
        {
            return View(store.Dispatcher.Fleet);
        }

        [HttpPost]
        public IActionResult Add(string id, string model, double maxWeight, double fuelEconomy,
                                 bool isRefrigerated, double fridgeFuelPerHour)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(model))
            {
                TempData["Error"] = "Вкажіть номер і модель трака";
                return RedirectToAction("Index");
            }

            try
            {
                IVehicle vehicle;
                if (isRefrigerated)
                    vehicle = new RefrigeratedTruck(id.Trim(), model.Trim(), maxWeight, fuelEconomy, fridgeFuelPerHour);
                else
                    vehicle = new Truck(id.Trim(), model.Trim(), maxWeight, fuelEconomy);

                store.Dispatcher.AddVehicle(vehicle);
                store.Save();
                TempData["Message"] = $"Трак {vehicle.Id} додано";
            }
            catch (ArgumentException)
            {
                TempData["Error"] = "Неправильні дані або трак з таким номером уже є";
            }

            return RedirectToAction("Index");
        }
    }
}
