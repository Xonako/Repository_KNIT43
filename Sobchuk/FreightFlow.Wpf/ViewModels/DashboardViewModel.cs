using System.Collections.ObjectModel;
using System.Linq;
using FreightFlow.Models;
using FreightFlow.Wpf.Services;

namespace FreightFlow.Wpf.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly DataService data;

        private int truckCount;
        private int freeTrucks;
        private int postedCount;
        private int bookedCount;
        private int deliveredCount;
        private string carrierProfit;
        private string brokerInfo;
        private string dispatcherInfo;

        public ObservableCollection<LoadRow> Delivered { get; } = new ObservableCollection<LoadRow>();

        public DashboardViewModel(DataService data)
        {
            this.data = data;
        }

        public int TruckCount
        {
            get { return truckCount; }
            set { SetProperty(ref truckCount, value); }
        }

        public int FreeTrucks
        {
            get { return freeTrucks; }
            set { SetProperty(ref freeTrucks, value); }
        }

        public int PostedCount
        {
            get { return postedCount; }
            set { SetProperty(ref postedCount, value); }
        }

        public int BookedCount
        {
            get { return bookedCount; }
            set { SetProperty(ref bookedCount, value); }
        }

        public int DeliveredCount
        {
            get { return deliveredCount; }
            set { SetProperty(ref deliveredCount, value); }
        }

        public string CarrierProfit
        {
            get { return carrierProfit; }
            set { SetProperty(ref carrierProfit, value); }
        }

        public string BrokerInfo
        {
            get { return brokerInfo; }
            set { SetProperty(ref brokerInfo, value); }
        }

        public string DispatcherInfo
        {
            get { return dispatcherInfo; }
            set { SetProperty(ref dispatcherInfo, value); }
        }

        public void Refresh()
        {
            var fleet = data.Dispatcher.Fleet;
            var loads = data.Broker.AllLoads;

            TruckCount = fleet.Count;
            FreeTrucks = fleet.Count(v => v.IsAvailable);
            PostedCount = loads.Count(l => l.Status == LoadStatus.Posted);
            BookedCount = loads.Count(l => l.Status == LoadStatus.Booked);
            DeliveredCount = loads.Count(l => l.Status == LoadStatus.Delivered);
            CarrierProfit = Ui.Money(data.Dispatcher.GetTotalCarrierProfit());

            BrokerInfo = $"Брокер: {data.Broker.Name}, {data.Broker.Phone} - заробив {Ui.Money(data.Broker.TotalEarned)}";
            DispatcherInfo = $"Диспетчер: {data.Dispatcher.Name}, {data.Dispatcher.Phone} - заробив {Ui.Money(data.Dispatcher.TotalEarned)}";

            Delivered.Clear();
            foreach (Load l in loads.Where(x => x.Status == LoadStatus.Delivered))
            {
                LoadRow row = new LoadRow(l);
                row.Vehicle = l.AssignedVehicle.Id;
                row.Cost = Ui.Money(l.AssignedVehicle.CalculateCost(l.Distance));
                row.BrokerCommission = Ui.Money(data.Broker.CalculateCommission(l));
                row.DispatcherCommission = Ui.Money(data.Dispatcher.CalculateCommission(l));
                row.Profit = Ui.Money(data.Dispatcher.CalculateProfit(l, l.AssignedVehicle));
                Delivered.Add(row);
            }
        }
    }
}
