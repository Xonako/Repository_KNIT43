using System;
using System.Collections.Generic;
using System.Linq;
using FreightFlow.Models;
using FreightFlow.Participants;
using FreightFlow.Vehicles;

namespace FreightFlow
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Title("FreightFlow - systema vantazhoperevezen (solo team)");

            Broker broker = new Broker("Ali Hamad", "+3695");
            Dispatcher dispatcher = new Dispatcher("Zach S", "+19712524637");

            Truck cascadia = new Truck("T-101", "Freightliner Cascadia", 45000, 7.0);
            Truck kenworth = new Truck("T-102", "Kenworth T680", 44000, 6.5);
            RefrigeratedTruck reefer = new RefrigeratedTruck("R-201", "Peterbilt 579", 42000, 6.2, 0.8);

            dispatcher.AddVehicle(cascadia);
            dispatcher.AddVehicle(kenworth);
            dispatcher.AddVehicle(reefer);

            Title("1. Park trakiv");
            Console.WriteLine("Vstanovlyuyemo temperaturu refryzheratora 34 F: " + reefer.SetTemperature(34));
            Console.WriteLine("Probuyemo vstanovyty -40 F: " + reefer.SetTemperature(-40));
            Console.WriteLine();
            foreach (IVehicle v in dispatcher.Fleet)
            {
                Console.WriteLine(v.GetInfo());
                Console.WriteLine($"    na 500 mi: palne {v.CalculateFuelNeeded(500)} gal, chas {v.CalculateDeliveryTime(500)} hod, sobivartist ${v.CalculateCost(500):F0}");
            }

            Title("2. Broker vykladaye vantazhi");
            broker.PostLoad("Chicago, IL", "Dallas, TX", 925, 38000, CargoType.General, 2400);
            broker.PostLoad("Miami, FL", "New York, NY", 1280, 40000, CargoType.Refrigerated, 3100);
            broker.PostLoad("Houston, TX", "Phoenix, AZ", 1180, 48000, CargoType.General, 2900);
            broker.PostLoad("Atlanta, GA", "Orlando, FL", 440, 22000, CargoType.Fragile, 1250);
            broker.PostLoad("Denver, CO", "Kansas City, MO", 600, 30000, CargoType.General, 1050);
            broker.PostLoad("Los Angeles, CA", "San Diego, CA", 120, 15000, CargoType.Refrigerated, 550);
            PrintLoads("Doshka vantazhiv (vid naybilshoyi stavky za mylyu):", broker.GetAvailableLoads());

            PrintLoads("Filtr: tilky refryzheratorni vantazhi:", broker.GetAvailableLoads(CargoType.Refrigerated, 0));
            PrintLoads("Filtr: usi typy, stavka vid $2.20/mi:", broker.GetAvailableLoads(null, 2.20m));

            Title("3. Dyspetcher bronyuye vantazhi (raund 1)");
            BookingRound(dispatcher, broker);

            Title("4. Reysy");
            DeliverAll(dispatcher, broker);

            Title("5. Traky zvilnylysya - raund 2");
            BookingRound(dispatcher, broker);
            DeliverAll(dispatcher, broker);

            Title("6. Pidsumky");
            Console.WriteLine(dispatcher.GenerateReport());
            Console.WriteLine();

            List<LogisticsParticipant> participants = new List<LogisticsParticipant> { broker, dispatcher };
            foreach (LogisticsParticipant p in participants)
                Console.WriteLine(p.GetInfo());

            PrintLoads("\nNevykonani vantazhi:", broker.GetAvailableLoads());

            Console.WriteLine("\nNatysnit bud-yaku klavishu...");
            Console.ReadKey();
        }

        static void BookingRound(Dispatcher dispatcher, Broker broker)
        {
            foreach (Load load in broker.GetAvailableLoads())
            {
                BookingResult result = dispatcher.BookLoad(broker, load);
                string text;
                switch (result)
                {
                    case BookingResult.Booked:
                        text = $"zabronovano na {load.AssignedVehicle.Id}, stavka ${load.CarrierRate:F0}";
                        break;
                    case BookingResult.NoVehicle:
                        text = "nemaye vilnoho transportu, yakyy mozhe vezty tsey vantazh";
                        break;
                    case BookingResult.RateRejected:
                        text = $"broker ne pohodyvsya pidnyaty stavku (maksymum ${broker.GetMaxRate(load):F0})";
                        break;
                    default:
                        text = "vantazh uzhe zabronovano";
                        break;
                }
                Console.WriteLine($"Vantazh #{load.Id} ({load.From} -> {load.To}): {text}");
            }
        }

        static void DeliverAll(Dispatcher dispatcher, Broker broker)
        {
            List<Load> booked = broker.AllLoads.Where(l => l.Status == LoadStatus.Booked).ToList();
            if (booked.Count == 0)
            {
                Console.WriteLine("Nemaye zabronovanykh vantazhiv");
                return;
            }

            foreach (Load load in booked)
            {
                IVehicle v = load.AssignedVehicle;
                dispatcher.StartDelivery(load);
                Console.WriteLine($"{v.Id} veze #{load.Id} {load.From} -> {load.To} ({load.Distance} mi, {load.Weight} lbs)");
                Console.WriteLine($"    palne: {v.CalculateFuelNeeded(load.Distance)} gal, chas u dorozi: {v.CalculateDeliveryTime(load.Distance)} hod");
                Console.WriteLine($"    stavka: ${load.CarrierRate:F0}, sobivartist: ${v.CalculateCost(load.Distance):F0}, " +
                                  $"prybutok: ${dispatcher.CalculateProfit(load, v):F0}");

                dispatcher.CompleteDelivery(broker, load);
                Console.WriteLine($"    dostavleno, broker otrymav ${broker.CalculateCommission(load):F0}, " +
                                  $"dyspetcher ${dispatcher.CalculateCommission(load):F0}");
            }
        }

        static void PrintLoads(string header, List<Load> loads)
        {
            Console.WriteLine(header);
            if (loads.Count == 0)
                Console.WriteLine("  (porozhno)");
            foreach (Load l in loads)
                Console.WriteLine("  " + l);
            Console.WriteLine();
        }

        static void Title(string text)
        {
            Console.WriteLine();
            Console.WriteLine(new string('=', 70));
            Console.WriteLine(text);
            Console.WriteLine(new string('=', 70));
        }
    }
}
