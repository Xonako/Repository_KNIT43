using System.Collections.Generic;
using FreightFlow.Models;

namespace FreightFlow.Wpf.Services
{
    public class StoreData
    {
        public List<VehicleData> Vehicles { get; set; } = new List<VehicleData>();
        public List<LoadData> Loads { get; set; } = new List<LoadData>();
    }

    public class VehicleData
    {
        public string Id { get; set; }
        public string Model { get; set; }
        public bool IsRefrigerated { get; set; }
        public double MaxWeight { get; set; }
        public double FuelEconomy { get; set; }
        public double AverageSpeed { get; set; }
        public double FridgeFuelPerHour { get; set; }
        public double Temperature { get; set; }
    }

    public class LoadData
    {
        public string From { get; set; }
        public string To { get; set; }
        public double Distance { get; set; }
        public double Weight { get; set; }
        public CargoType Type { get; set; }
        public decimal ShipperPrice { get; set; }
        public decimal CarrierRate { get; set; }
        public LoadStatus Status { get; set; }
        public string VehicleId { get; set; }
    }
}
