using System.Globalization;
using FreightFlow.Models;

namespace FreightFlow.Web.Services
{
    public static class Ui
    {
        public static string Cargo(CargoType type)
        {
            switch (type)
            {
                case CargoType.Refrigerated: return "Рефрижераторний";
                case CargoType.Fragile: return "Крихкий";
                default: return "Звичайний";
            }
        }

        public static string Status(LoadStatus status)
        {
            switch (status)
            {
                case LoadStatus.Booked: return "Заброньовано";
                case LoadStatus.InTransit: return "В дорозі";
                case LoadStatus.Delivered: return "Доставлено";
                default: return "На дошці";
            }
        }

        public static string Money(decimal value)
        {
            return "$" + value.ToString("N0", CultureInfo.InvariantCulture);
        }
    }
}
