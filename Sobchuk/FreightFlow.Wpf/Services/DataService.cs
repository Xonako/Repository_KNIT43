using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using FreightFlow.Models;
using FreightFlow.Participants;
using FreightFlow.Vehicles;

namespace FreightFlow.Wpf.Services
{
    public class DataService
    {
        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public string FilePath { get; }
        public Broker Broker { get; private set; }
        public Dispatcher Dispatcher { get; private set; }

        public DataService()
        {
            FilePath = Path.Combine(AppContext.BaseDirectory, "data.json");

            StoreData data = null;
            if (File.Exists(FilePath))
            {
                try
                {
                    data = JsonSerializer.Deserialize<StoreData>(File.ReadAllText(FilePath), jsonOptions);
                }
                catch (JsonException)
                {
                    data = null;
                }
            }

            Build(data ?? CreateSeedData());
            Save();
        }

        public IVehicle FindVehicle(string id)
        {
            return Dispatcher.Fleet.FirstOrDefault(v => v.Id == id);
        }

        public void Save()
        {
            File.WriteAllText(FilePath, JsonSerializer.Serialize(ToData(), jsonOptions));
        }

        public void Reset()
        {
            Build(CreateSeedData());
            Save();
        }

        private void Build(StoreData data)
        {
            Broker = new Broker("Ali Hamad", "+3695");
            Dispatcher = new Dispatcher("Zach S", "+19712524637");

            foreach (VehicleData v in data.Vehicles)
            {
                if (v.IsRefrigerated)
                {
                    RefrigeratedTruck reefer = new RefrigeratedTruck(v.Id, v.Model, v.MaxWeight,
                        v.FuelEconomy, v.FridgeFuelPerHour, v.AverageSpeed);
                    reefer.SetTemperature(v.Temperature);
                    Dispatcher.AddVehicle(reefer);
                }
                else
                {
                    Dispatcher.AddVehicle(new Truck(v.Id, v.Model, v.MaxWeight, v.FuelEconomy, v.AverageSpeed));
                }
            }

            List<(Load load, LoadData data)> pairs = new List<(Load, LoadData)>();
            foreach (LoadData d in data.Loads)
            {
                Load load = Broker.PostLoad(d.From, d.To, d.Distance, d.Weight, d.Type, d.ShipperPrice);
                if (d.CarrierRate > 0 && d.CarrierRate != load.CarrierRate)
                    load.ChangeCarrierRate(d.CarrierRate);
                pairs.Add((load, d));
            }

            foreach (var p in pairs.Where(x => x.data.Status == LoadStatus.Delivered))
            {
                IVehicle vehicle = FindVehicle(p.data.VehicleId);
                if (vehicle != null && Dispatcher.BookLoad(Broker, p.load, vehicle) == BookingResult.Booked)
                {
                    Dispatcher.StartDelivery(p.load);
                    Dispatcher.CompleteDelivery(Broker, p.load);
                }
            }

            foreach (var p in pairs.Where(x => x.data.Status == LoadStatus.Booked || x.data.Status == LoadStatus.InTransit))
            {
                IVehicle vehicle = FindVehicle(p.data.VehicleId);
                if (vehicle != null && Dispatcher.BookLoad(Broker, p.load, vehicle) == BookingResult.Booked
                    && p.data.Status == LoadStatus.InTransit)
                {
                    Dispatcher.StartDelivery(p.load);
                }
            }
        }

        private StoreData ToData()
        {
            StoreData data = new StoreData();

            foreach (IVehicle v in Dispatcher.Fleet)
            {
                Vehicle vehicle = (Vehicle)v;
                VehicleData vd = new VehicleData
                {
                    Id = vehicle.Id,
                    Model = vehicle.Model,
                    MaxWeight = vehicle.MaxWeight,
                    FuelEconomy = vehicle.FuelEconomy,
                    AverageSpeed = vehicle.AverageSpeed
                };
                if (vehicle is RefrigeratedTruck reefer)
                {
                    vd.IsRefrigerated = true;
                    vd.FridgeFuelPerHour = reefer.FridgeFuelPerHour;
                    vd.Temperature = reefer.Temperature;
                }
                data.Vehicles.Add(vd);
            }

            foreach (Load l in Broker.AllLoads)
            {
                data.Loads.Add(new LoadData
                {
                    From = l.From,
                    To = l.To,
                    Distance = l.Distance,
                    Weight = l.Weight,
                    Type = l.Type,
                    ShipperPrice = l.ShipperPrice,
                    CarrierRate = l.CarrierRate,
                    Status = l.Status,
                    VehicleId = l.AssignedVehicle?.Id
                });
            }

            return data;
        }

        private static StoreData CreateSeedData()
        {
            StoreData data = new StoreData();

            data.Vehicles.Add(new VehicleData { Id = "T-101", Model = "Freightliner Cascadia", MaxWeight = 45000, FuelEconomy = 7.0, AverageSpeed = 55 });
            data.Vehicles.Add(new VehicleData { Id = "T-102", Model = "Kenworth T680", MaxWeight = 44000, FuelEconomy = 6.5, AverageSpeed = 55 });
            data.Vehicles.Add(new VehicleData { Id = "R-201", Model = "Peterbilt 579", MaxWeight = 42000, FuelEconomy = 6.2, AverageSpeed = 55,
                                                IsRefrigerated = true, FridgeFuelPerHour = 0.8, Temperature = 34 });

            data.Loads.Add(new LoadData { From = "Chicago, IL", To = "Dallas, TX", Distance = 925, Weight = 38000, Type = CargoType.General, ShipperPrice = 2400 });
            data.Loads.Add(new LoadData { From = "Miami, FL", To = "New York, NY", Distance = 1280, Weight = 40000, Type = CargoType.Refrigerated, ShipperPrice = 3100 });
            data.Loads.Add(new LoadData { From = "Houston, TX", To = "Phoenix, AZ", Distance = 1180, Weight = 48000, Type = CargoType.General, ShipperPrice = 2900 });
            data.Loads.Add(new LoadData { From = "Atlanta, GA", To = "Orlando, FL", Distance = 440, Weight = 22000, Type = CargoType.Fragile, ShipperPrice = 1250 });
            data.Loads.Add(new LoadData { From = "Denver, CO", To = "Kansas City, MO", Distance = 600, Weight = 30000, Type = CargoType.General, ShipperPrice = 1050 });
            data.Loads.Add(new LoadData { From = "Los Angeles, CA", To = "San Diego, CA", Distance = 120, Weight = 15000, Type = CargoType.Refrigerated, ShipperPrice = 550 });

            return data;
        }
    }
}
