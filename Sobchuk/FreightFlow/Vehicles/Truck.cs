using System;
using FreightFlow.Models;

namespace FreightFlow.Vehicles
{
    public class Truck : Vehicle
    {
        public const decimal CostPerMile = 0.90m;

        public Truck(string id, string model, double maxWeight, double fuelEconomy, double averageSpeed = 55)
            : base(id, model, maxWeight, fuelEconomy, averageSpeed)
        {
        }

        public override string TypeName
        {
            get { return "Trak"; }
        }

        public override bool CanCarry(Load load)
        {
            if (load.Weight > MaxWeight)
                return false;
            return load.Type != CargoType.Refrigerated;
        }

        public override decimal CalculateCost(double distance)
        {
            decimal fuelCost = (decimal)CalculateFuelNeeded(distance) * FuelPrice;
            decimal otherCost = (decimal)distance * CostPerMile;
            return Math.Round(fuelCost + otherCost, 2);
        }

        public double GetFreeCapacity(Load load)
        {
            return MaxWeight - load.Weight;
        }
    }
}
