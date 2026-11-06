using System;
using FreightFlow.Models;

namespace FreightFlow.Vehicles
{
    public abstract class Vehicle : IVehicle
    {
        public const decimal FuelPrice = 3.80m;
        public const double MaxDrivingPerDay = 11;
        public const double DailyRest = 10;
        public const double DrivingBeforeBreak = 8;
        public const double BreakTime = 0.5;

        protected double fuelEconomy;
        protected double averageSpeed;

        public string Id { get; }
        public string Model { get; }
        public double MaxWeight { get; }
        public bool IsAvailable { get; private set; }
        public Load CurrentLoad { get; private set; }

        protected Vehicle(string id, string model, double maxWeight, double fuelEconomy, double averageSpeed)
        {
            if (maxWeight <= 0)
                throw new ArgumentException("Vantazhopidyomnist maye buty bilshe 0");
            if (fuelEconomy <= 0 || averageSpeed <= 0)
                throw new ArgumentException("Nekorektni parametry transportu");

            Id = id;
            Model = model;
            MaxWeight = maxWeight;
            this.fuelEconomy = fuelEconomy;
            this.averageSpeed = averageSpeed;
            IsAvailable = true;
        }

        public double FuelEconomy
        {
            get { return fuelEconomy; }
        }

        public double AverageSpeed
        {
            get { return averageSpeed; }
        }

        public abstract string TypeName { get; }

        public abstract bool CanCarry(Load load);

        public double GetDrivingHours(double distance)
        {
            return distance / averageSpeed;
        }

        public virtual double CalculateFuelNeeded(double distance)
        {
            return Math.Round(distance / fuelEconomy, 1);
        }

        public double CalculateDeliveryTime(double distance)
        {
            double driving = GetDrivingHours(distance);
            int breaks = (int)(driving / DrivingBeforeBreak);
            int rests = (int)Math.Ceiling(driving / MaxDrivingPerDay) - 1;
            if (rests < 0)
                rests = 0;
            return Math.Round(driving + breaks * BreakTime + rests * DailyRest, 1);
        }

        public abstract decimal CalculateCost(double distance);

        public void AssignLoad(Load load)
        {
            if (!IsAvailable)
                throw new InvalidOperationException($"Transport {Id} zaraz zaynyatyy");
            if (!CanCarry(load))
                throw new InvalidOperationException($"Transport {Id} ne mozhe vezty tsey vantazh");
            CurrentLoad = load;
            IsAvailable = false;
        }

        public void Release()
        {
            CurrentLoad = null;
            IsAvailable = true;
        }

        public virtual string GetInfo()
        {
            string status = IsAvailable ? "vilnyy" : "v reysi";
            return $"[{Id}] {TypeName} {Model}, do {MaxWeight} lbs, {fuelEconomy} MPG, {status}";
        }
    }
}
