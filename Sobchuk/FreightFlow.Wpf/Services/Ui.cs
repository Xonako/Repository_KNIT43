using System.Globalization;
using FreightFlow.Models;

namespace FreightFlow.Wpf.Services
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

        public static string Money(decimal value)
        {
            return "$" + value.ToString("N0", CultureInfo.InvariantCulture);
        }

        public static bool TryParseDouble(string text, out double value)
        {
            string s = (text ?? "").Trim().Replace(',', '.');
            return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        public static bool TryParseDecimal(string text, out decimal value)
        {
            string s = (text ?? "").Trim().Replace(',', '.');
            return decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }
    }
}
