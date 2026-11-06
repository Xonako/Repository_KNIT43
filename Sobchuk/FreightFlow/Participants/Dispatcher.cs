using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreightFlow.Models;
using FreightFlow.Vehicles;

namespace FreightFlow.Participants
{
    public class Dispatcher : LogisticsParticipant
    {
        private readonly List<IVehicle> fleet = new List<IVehicle>();
        private readonly List<Load> bookedLoads = new List<Load>();

        public decimal CommissionPercent { get; }
        public decimal MinProfitPercent { get; }

        public Dispatcher(string name, string phone, decimal commissionPercent = 0.08m, decimal minProfitPercent = 0.15m)
            : base(name, phone)
        {
            CommissionPercent = commissionPercent;
            MinProfitPercent = minProfitPercent;
        }

        public override string Role
        {
            get { return "Dyspetcher"; }
        }

        public IReadOnlyList<IVehicle> Fleet
        {
            get { return fleet; }
        }

        public void AddVehicle(IVehicle vehicle)
        {
            if (fleet.Any(v => v.Id == vehicle.Id))
                throw new ArgumentException($"Transport {vehicle.Id} uzhe ye v parku");
            fleet.Add(vehicle);
        }

        public IVehicle FindBestVehicle(Load load)
        {
            return fleet
                .Where(v => v.IsAvailable && v.CanCarry(load))
                .OrderBy(v => v.CalculateCost(load.Distance))
                .ThenBy(v => v.CalculateDeliveryTime(load.Distance))
                .FirstOrDefault();
        }

        public override decimal CalculateCommission(Load load)
        {
            return Math.Round(load.CarrierRate * CommissionPercent, 2);
        }

        public decimal CalculateProfit(Load load, IVehicle vehicle)
        {
            return load.CarrierRate - vehicle.CalculateCost(load.Distance) - CalculateCommission(load);
        }

        public decimal CalculateRequiredRate(Load load, IVehicle vehicle)
        {
            decimal cost = vehicle.CalculateCost(load.Distance);
            return Math.Ceiling(cost * (1 + MinProfitPercent) / (1 - CommissionPercent));
        }

        public BookingResult BookLoad(Broker broker, Load load)
        {
            if (load.Status != LoadStatus.Posted)
                return BookingResult.AlreadyBooked;

            IVehicle vehicle = FindBestVehicle(load);
            if (vehicle == null)
                return BookingResult.NoVehicle;

            return BookLoad(broker, load, vehicle);
        }

        public BookingResult BookLoad(Broker broker, Load load, IVehicle vehicle)
        {
            if (load.Status != LoadStatus.Posted)
                return BookingResult.AlreadyBooked;
            if (!fleet.Contains(vehicle) || !vehicle.IsAvailable || !vehicle.CanCarry(load))
                return BookingResult.NoVehicle;

            decimal required = CalculateRequiredRate(load, vehicle);
            if (load.CarrierRate < required && !broker.NegotiateRate(load, required))
                return BookingResult.RateRejected;

            load.Book(vehicle);
            vehicle.AssignLoad(load);
            bookedLoads.Add(load);
            return BookingResult.Booked;
        }

        public void StartDelivery(Load load)
        {
            if (!bookedLoads.Contains(load))
                throw new InvalidOperationException("Tsey vantazh ne bronyuvav tsey dyspetcher");
            load.StartTransit();
        }

        public void CompleteDelivery(Broker broker, Load load)
        {
            broker.ConfirmDelivery(load);
            load.AssignedVehicle.Release();
            AddEarnings(CalculateCommission(load));
        }

        public decimal GetTotalCarrierProfit()
        {
            return bookedLoads
                .Where(l => l.Status == LoadStatus.Delivered)
                .Sum(l => CalculateProfit(l, l.AssignedVehicle));
        }

        public string GenerateReport()
        {
            StringBuilder sb = new StringBuilder();
            List<Load> delivered = bookedLoads.Where(l => l.Status == LoadStatus.Delivered).ToList();

            sb.AppendLine($"Zvit dyspetchera {Name}");
            sb.AppendLine($"Vykonano reysiv: {delivered.Count}");
            foreach (Load l in delivered)
            {
                IVehicle v = l.AssignedVehicle;
                sb.AppendLine($"  #{l.Id} {l.From} -> {l.To} ({v.Id}): stavka ${l.CarrierRate:F0}, " +
                              $"sobivartist ${v.CalculateCost(l.Distance):F0}, komisiya ${CalculateCommission(l):F0}, " +
                              $"prybutok ${CalculateProfit(l, v):F0}");
            }
            sb.AppendLine($"Prybutok vlasnyka trakiv: ${GetTotalCarrierProfit():F2}");
            sb.Append($"Komisiya dyspetchera: ${TotalEarned:F2}");
            return sb.ToString();
        }
    }
}
