using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FreightFlow.Models;
using FreightFlow.Vehicles;
using FreightFlow.Wpf.Services;

namespace FreightFlow.Wpf.ViewModels
{
    public class DispatchViewModel : ViewModelBase
    {
        private readonly DataService data;
        private readonly MainViewModel main;

        private LoadRow selectedAvailable;
        private LoadRow selectedBooked;

        public ObservableCollection<LoadRow> AvailableLoads { get; } = new ObservableCollection<LoadRow>();
        public ObservableCollection<LoadRow> BookedLoads { get; } = new ObservableCollection<LoadRow>();

        public ICommand BookCommand { get; }
        public ICommand DeliverCommand { get; }

        public DispatchViewModel(DataService data, MainViewModel main)
        {
            this.data = data;
            this.main = main;
            BookCommand = new RelayCommand(Book, () => SelectedAvailable != null);
            DeliverCommand = new RelayCommand(Deliver, () => SelectedBooked != null);
        }

        public LoadRow SelectedAvailable
        {
            get { return selectedAvailable; }
            set { SetProperty(ref selectedAvailable, value); }
        }

        public LoadRow SelectedBooked
        {
            get { return selectedBooked; }
            set { SetProperty(ref selectedBooked, value); }
        }

        public void Refresh()
        {
            AvailableLoads.Clear();
            foreach (Load l in data.Broker.GetAvailableLoads())
            {
                LoadRow row = new LoadRow(l);
                IVehicle best = data.Dispatcher.FindBestVehicle(l);
                if (best == null)
                {
                    row.Vehicle = "немає вільного";
                    row.Cost = "-";
                    row.RequiredRate = "-";
                }
                else
                {
                    row.Vehicle = $"{best.Id} ({best.Model})";
                    row.Cost = Ui.Money(best.CalculateCost(l.Distance));
                    row.RequiredRate = Ui.Money(data.Dispatcher.CalculateRequiredRate(l, best));
                }
                AvailableLoads.Add(row);
            }

            BookedLoads.Clear();
            foreach (Load l in data.Broker.AllLoads.Where(x => x.Status == LoadStatus.Booked))
            {
                IVehicle v = l.AssignedVehicle;
                LoadRow row = new LoadRow(l);
                row.Vehicle = v.Id;
                row.Fuel = v.CalculateFuelNeeded(l.Distance) + " gal";
                row.Time = v.CalculateDeliveryTime(l.Distance) + " год";
                row.Cost = Ui.Money(v.CalculateCost(l.Distance));
                row.Profit = Ui.Money(data.Dispatcher.CalculateProfit(l, v));
                BookedLoads.Add(row);
            }
        }

        private void Book()
        {
            Load load = SelectedAvailable.Load;
            decimal oldRate = load.CarrierRate;
            BookingResult result = data.Dispatcher.BookLoad(data.Broker, load);

            string message;
            switch (result)
            {
                case BookingResult.Booked:
                    message = $"Вантаж #{load.Id} заброньовано на {load.AssignedVehicle.Id}";
                    if (load.CarrierRate != oldRate)
                        message += $", після торгу ставка піднята з {Ui.Money(oldRate)} до {Ui.Money(load.CarrierRate)}";
                    data.Save();
                    break;
                case BookingResult.NoVehicle:
                    message = $"Для вантажу #{load.Id} немає вільного трака, який може його везти";
                    break;
                case BookingResult.RateRejected:
                    message = $"Брокер не погодився підняти ставку для #{load.Id} (максимум {Ui.Money(data.Broker.GetMaxRate(load))})";
                    break;
                default:
                    message = "Вантаж уже заброньовано";
                    break;
            }

            main.RefreshAll();
            main.ShowMessage(message);
        }

        private void Deliver()
        {
            Load load = SelectedBooked.Load;
            if (load.Status != LoadStatus.Booked)
                return;

            data.Dispatcher.StartDelivery(load);
            data.Dispatcher.CompleteDelivery(data.Broker, load);
            data.Save();

            main.RefreshAll();
            main.ShowMessage($"Вантаж #{load.Id} доставлено, трак {load.AssignedVehicle.Id} знову вільний");
        }
    }
}
