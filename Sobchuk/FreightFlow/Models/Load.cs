using System;
using FreightFlow.Vehicles;

namespace FreightFlow.Models
{
    public class Load
    {
        public int Id { get; }
        public string From { get; }
        public string To { get; }
        public double Distance { get; }
        public double Weight { get; }
        public CargoType Type { get; }
        public decimal ShipperPrice { get; }
        public decimal CarrierRate { get; private set; }
        public LoadStatus Status { get; private set; }
        public IVehicle AssignedVehicle { get; private set; }

        public Load(int id, string from, string to, double distance, double weight,
                    CargoType type, decimal shipperPrice, decimal carrierRate)
        {
            if (distance <= 0)
                throw new ArgumentException("Vidstan maye buty bilshe 0");
            if (weight <= 0)
                throw new ArgumentException("Vaha maye buty bilshe 0");
            if (shipperPrice <= 0)
                throw new ArgumentException("Tsina maye buty bilshe 0");

            Id = id;
            From = from;
            To = to;
            Distance = distance;
            Weight = weight;
            Type = type;
            ShipperPrice = shipperPrice;
            CarrierRate = carrierRate;
            Status = LoadStatus.Posted;
        }

        public decimal RatePerMile
        {
            get { return Math.Round(CarrierRate / (decimal)Distance, 2); }
        }

        public void ChangeCarrierRate(decimal newRate)
        {
            if (Status != LoadStatus.Posted)
                throw new InvalidOperationException("Stavku mozhna zminyty lyshe dlya vilnoho vantazhu");
            CarrierRate = newRate;
        }

        public void Book(IVehicle vehicle)
        {
            if (Status != LoadStatus.Posted)
                throw new InvalidOperationException("Vantazh uzhe zabronovano");
            AssignedVehicle = vehicle;
            Status = LoadStatus.Booked;
        }

        public void StartTransit()
        {
            if (Status != LoadStatus.Booked)
                throw new InvalidOperationException("Vantazh ne zabronovano");
            Status = LoadStatus.InTransit;
        }

        public void MarkDelivered()
        {
            if (Status != LoadStatus.InTransit)
                throw new InvalidOperationException("Vantazh ne v dorozi");
            Status = LoadStatus.Delivered;
        }

        public override string ToString()
        {
            return $"#{Id} {From} -> {To}, {Distance} mi, {Weight} lbs, {Type}, stavka ${CarrierRate:F0} (${RatePerMile}/mi), {Status}";
        }
    }
}
