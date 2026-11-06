using System;
using System.Collections.Generic;
using System.Linq;
using FreightFlow.Models;

namespace FreightFlow.Participants
{
    public class Broker : LogisticsParticipant
    {
        private readonly List<Load> loads = new List<Load>();
        private int nextId = 1;

        public decimal CarrierShare { get; }
        public decimal MinMarginPercent { get; }

        public Broker(string name, string phone, decimal carrierShare = 0.85m, decimal minMarginPercent = 0.10m)
            : base(name, phone)
        {
            if (carrierShare <= 0 || carrierShare >= 1)
                throw new ArgumentException("Chastka pereviznyka maye buty vid 0 do 1");
            CarrierShare = carrierShare;
            MinMarginPercent = minMarginPercent;
        }

        public override string Role
        {
            get { return "Broker"; }
        }

        public IReadOnlyList<Load> AllLoads
        {
            get { return loads; }
        }

        public Load PostLoad(string from, string to, double distance, double weight,
                             CargoType type, decimal shipperPrice)
        {
            decimal carrierRate = Math.Round(shipperPrice * CarrierShare, 0);
            Load load = new Load(nextId, from, to, distance, weight, type, shipperPrice, carrierRate);
            nextId++;
            loads.Add(load);
            return load;
        }

        public List<Load> GetAvailableLoads()
        {
            return loads
                .Where(l => l.Status == LoadStatus.Posted)
                .OrderByDescending(l => l.RatePerMile)
                .ToList();
        }

        public List<Load> GetAvailableLoads(CargoType? type, decimal minRatePerMile)
        {
            return GetAvailableLoads()
                .Where(l => (type == null || l.Type == type) && l.RatePerMile >= minRatePerMile)
                .ToList();
        }

        public decimal GetMaxRate(Load load)
        {
            return Math.Round(load.ShipperPrice * (1 - MinMarginPercent), 0);
        }

        public bool NegotiateRate(Load load, decimal requestedRate)
        {
            if (load.Status != LoadStatus.Posted)
                return false;
            if (requestedRate <= load.CarrierRate)
                return true;
            if (requestedRate > GetMaxRate(load))
                return false;

            load.ChangeCarrierRate(requestedRate);
            return true;
        }

        public override decimal CalculateCommission(Load load)
        {
            return load.ShipperPrice - load.CarrierRate;
        }

        public void ConfirmDelivery(Load load)
        {
            if (!loads.Contains(load))
                throw new InvalidOperationException("Tse ne vantazh tsoho brokera");
            load.MarkDelivered();
            AddEarnings(CalculateCommission(load));
        }

        public override string GetInfo()
        {
            int delivered = loads.Count(l => l.Status == LoadStatus.Delivered);
            return base.GetInfo() + $", vantazhiv: {loads.Count}, dostavleno: {delivered}";
        }
    }
}
