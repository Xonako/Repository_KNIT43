using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using FreightFlow.Vehicles;
using FreightFlow.Wpf.Services;

namespace FreightFlow.Wpf.ViewModels
{
    public class FleetViewModel : ViewModelBase
    {
        private readonly DataService data;
        private readonly MainViewModel main;

        private string newId = "";
        private string newModel = "";
        private string newMaxWeight = "";
        private string newFuel = "";
        private bool newIsRefrigerated;
        private string newFridgeFuel = "0.8";

        public ObservableCollection<VehicleRow> Vehicles { get; } = new ObservableCollection<VehicleRow>();
        public ICommand AddTruckCommand { get; }

        public FleetViewModel(DataService data, MainViewModel main)
        {
            this.data = data;
            this.main = main;
            AddTruckCommand = new RelayCommand(AddTruck);
        }

        public string NewId
        {
            get { return newId; }
            set { SetProperty(ref newId, value); }
        }

        public string NewModel
        {
            get { return newModel; }
            set { SetProperty(ref newModel, value); }
        }

        public string NewMaxWeight
        {
            get { return newMaxWeight; }
            set { SetProperty(ref newMaxWeight, value); }
        }

        public string NewFuel
        {
            get { return newFuel; }
            set { SetProperty(ref newFuel, value); }
        }

        public bool NewIsRefrigerated
        {
            get { return newIsRefrigerated; }
            set { SetProperty(ref newIsRefrigerated, value); }
        }

        public string NewFridgeFuel
        {
            get { return newFridgeFuel; }
            set { SetProperty(ref newFridgeFuel, value); }
        }

        public void Refresh()
        {
            Vehicles.Clear();
            foreach (IVehicle v in data.Dispatcher.Fleet)
                Vehicles.Add(new VehicleRow(v));
        }

        private void AddTruck()
        {
            double maxWeight;
            double fuel;
            double fridgeFuel = 0;

            if (string.IsNullOrWhiteSpace(NewId) || string.IsNullOrWhiteSpace(NewModel))
            {
                main.ShowMessage("Вкажіть номер і модель трака");
                return;
            }
            if (!Ui.TryParseDouble(NewMaxWeight, out maxWeight) || !Ui.TryParseDouble(NewFuel, out fuel))
            {
                main.ShowMessage("Вантажопідйомність і витрата пального мають бути числами");
                return;
            }
            if (NewIsRefrigerated && !Ui.TryParseDouble(NewFridgeFuel, out fridgeFuel))
            {
                main.ShowMessage("Витрата холодильника має бути числом");
                return;
            }

            try
            {
                IVehicle vehicle;
                if (NewIsRefrigerated)
                    vehicle = new RefrigeratedTruck(NewId.Trim(), NewModel.Trim(), maxWeight, fuel, fridgeFuel);
                else
                    vehicle = new Truck(NewId.Trim(), NewModel.Trim(), maxWeight, fuel);

                data.Dispatcher.AddVehicle(vehicle);
                data.Save();

                NewId = "";
                NewModel = "";
                NewMaxWeight = "";
                NewFuel = "";
                NewIsRefrigerated = false;

                main.RefreshAll();
                main.ShowMessage($"Трак {vehicle.Id} додано");
            }
            catch (ArgumentException)
            {
                main.ShowMessage("Неправильні дані або трак з таким номером уже є");
            }
        }
    }
}
