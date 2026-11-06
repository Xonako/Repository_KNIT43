using FreightFlow.Models;
using FreightFlow.Vehicles;
using FreightFlow.Wpf.Services;

namespace FreightFlow.Wpf.ViewModels
{
    public class VehicleRow
    {
        public string Id { get; }
        public string Type { get; }
        public string Model { get; }
        public string MaxWeight { get; }
        public double Mpg { get; }
        public string Fuel100 { get; }
        public string Cost100 { get; }
        public string Status { get; }

        public VehicleRow(IVehicle v)
        {
            Id = v.Id;
            Model = v.Model;
            if (v is RefrigeratedTruck reefer)
                Type = $"Рефрижератор ({reefer.Temperature} °F)";
            else
                Type = "Трак";
            MaxWeight = v.MaxWeight + " lbs";
            Mpg = ((Vehicle)v).FuelEconomy;
            Fuel100 = v.CalculateFuelNeeded(100) + " gal";
            Cost100 = Ui.Money(v.CalculateCost(100));
            Status = v.IsAvailable ? "вільний" : "у рейсі";
        }
    }

    public class LoadRow
    {
        public Load Load { get; }
        public int Id { get; }
        public string From { get; }
        public string To { get; }
        public string Route { get; }
        public string Distance { get; }
        public string Weight { get; }
        public string Type { get; }
        public string ShipperPrice { get; }
        public string CarrierRate { get; }
        public decimal RatePerMile { get; }

        public string Vehicle { get; set; }
        public string Cost { get; set; }
        public string RequiredRate { get; set; }
        public string Fuel { get; set; }
        public string Time { get; set; }
        public string BrokerCommission { get; set; }
        public string DispatcherCommission { get; set; }
        public string Profit { get; set; }

        public LoadRow(Load l)
        {
            Load = l;
            Id = l.Id;
            From = l.From;
            To = l.To;
            Route = $"{l.From} → {l.To}";
            Distance = l.Distance + " mi";
            Weight = l.Weight + " lbs";
            Type = Ui.Cargo(l.Type);
            ShipperPrice = Ui.Money(l.ShipperPrice);
            CarrierRate = Ui.Money(l.CarrierRate);
            RatePerMile = l.RatePerMile;
        }
    }
}
