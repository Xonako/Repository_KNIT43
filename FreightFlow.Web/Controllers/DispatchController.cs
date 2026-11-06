using FreightFlow.Models;
using FreightFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FreightFlow.Web.Controllers
{
    public class DispatchController : Controller
    {
        private readonly FreightStore store;

        public DispatchController(FreightStore store)
        {
            this.store = store;
        }

        public IActionResult Index()
        {
            return View(store);
        }

        [HttpPost]
        public IActionResult Book(int id)
        {
            Load load = store.FindLoad(id);
            if (load == null)
                return NotFound();

            decimal oldRate = load.CarrierRate;
            BookingResult result = store.Dispatcher.BookLoad(store.Broker, load);

            switch (result)
            {
                case BookingResult.Booked:
                    string text = $"Вантаж #{load.Id} заброньовано на {load.AssignedVehicle.Id}";
                    if (load.CarrierRate != oldRate)
                        text += $", після торгу ставка піднята з {Ui.Money(oldRate)} до {Ui.Money(load.CarrierRate)}";
                    TempData["Message"] = text;
                    store.Save();
                    break;
                case BookingResult.NoVehicle:
                    TempData["Error"] = $"Для вантажу #{load.Id} немає вільного трака, який може його везти";
                    break;
                case BookingResult.RateRejected:
                    TempData["Error"] = $"Брокер не погодився підняти ставку для #{load.Id} (максимум {Ui.Money(store.Broker.GetMaxRate(load))})";
                    break;
                default:
                    TempData["Error"] = "Вантаж уже заброньовано";
                    break;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Deliver(int id)
        {
            Load load = store.FindLoad(id);
            if (load == null)
                return NotFound();

            if (load.Status == LoadStatus.Booked)
            {
                store.Dispatcher.StartDelivery(load);
                store.Dispatcher.CompleteDelivery(store.Broker, load);
                store.Save();
                TempData["Message"] = $"Вантаж #{load.Id} доставлено, трак {load.AssignedVehicle.Id} знову вільний";
            }

            return RedirectToAction("Index");
        }
    }
}
