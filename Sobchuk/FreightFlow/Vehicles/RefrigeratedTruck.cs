using System;
using FreightFlow.Models;

namespace FreightFlow.Vehicles
{
    public class RefrigeratedTruck : Truck
    {
        public const double MinTemperature = -20;
        public const double MaxTemperature = 70;
        public const decimal FridgeCostPerMile = 0.15m;

        private double fridgeFuelPerHour;

        public double Temperature { get; private set; }

        public double FridgeFuelPerHour
        {
            get { return fridgeFuelPerHour; }
        }

        public RefrigeratedTruck(string id, string model, double maxWeight, double fuelEconomy,
                                 double fridgeFuelPerHour, double averageSpeed = 55)
            : base(id, model, maxWeight, fuelEconomy, averageSpeed)
        {
            this.fridgeFuelPerHour = fridgeFuelPerHour;
            Temperature = 36;
        }

        public override string TypeName
        {
            get { return "Refryzherator"; }
        }

        public bool SetTemperature(double temperature)
        {
            if (temperature < MinTemperature || temperature > MaxTemperature)
                return false;
            Temperature = temperature;
            return true;
        }

        public override bool CanCarry(Load load)
        {
            return load.Weight <= MaxWeight;
        }

        public override double CalculateFuelNeeded(double distance)
        {
            double fridgeFuel = GetDrivingHours(distance) * fridgeFuelPerHour;
            return Math.Round(base.CalculateFuelNeeded(distance) + fridgeFuel, 1);
        }

        public override decimal CalculateCost(double distance)
        {
            return base.CalculateCost(distance) + (decimal)distance * FridgeCostPerMile;
        }

        public override string GetInfo()
        {
            return base.GetInfo() + $", temperatura {Temperature} F";
        }
    }
}
