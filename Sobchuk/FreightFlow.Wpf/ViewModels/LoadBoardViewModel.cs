using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using FreightFlow.Models;
using FreightFlow.Wpf.Services;

namespace FreightFlow.Wpf.ViewModels
{
    public class LoadBoardViewModel : ViewModelBase
    {
        private readonly DataService data;
        private readonly MainViewModel main;

        private int filterIndex;
        private string minRateText = "";

        private string newFrom = "";
        private string newTo = "";
        private string newDistance = "";
        private string newWeight = "";
        private int newTypeIndex;
        private string newPrice = "";

        public string[] FilterOptions { get; } = new string[] { "Усі типи", "Звичайний", "Рефрижераторний", "Крихкий" };
        public string[] CargoOptions { get; } = new string[] { "Звичайний", "Рефрижераторний", "Крихкий" };

        public ObservableCollection<LoadRow> Loads { get; } = new ObservableCollection<LoadRow>();
        public ICommand PostLoadCommand { get; }

        public LoadBoardViewModel(DataService data, MainViewModel main)
        {
            this.data = data;
            this.main = main;
            PostLoadCommand = new RelayCommand(PostLoad);
        }

        public int FilterIndex
        {
            get { return filterIndex; }
            set
            {
                if (SetProperty(ref filterIndex, value))
                    Refresh();
            }
        }

        public string MinRateText
        {
            get { return minRateText; }
            set
            {
                if (SetProperty(ref minRateText, value))
                    Refresh();
            }
        }

        public string NewFrom
        {
            get { return newFrom; }
            set { SetProperty(ref newFrom, value); }
        }

        public string NewTo
        {
            get { return newTo; }
            set { SetProperty(ref newTo, value); }
        }

        public string NewDistance
        {
            get { return newDistance; }
            set { SetProperty(ref newDistance, value); }
        }

        public string NewWeight
        {
            get { return newWeight; }
            set { SetProperty(ref newWeight, value); }
        }

        public int NewTypeIndex
        {
            get { return newTypeIndex; }
            set { SetProperty(ref newTypeIndex, value); }
        }

        public string NewPrice
        {
            get { return newPrice; }
            set { SetProperty(ref newPrice, value); }
        }

        public void Refresh()
        {
            CargoType? type = null;
            if (FilterIndex > 0)
                type = (CargoType)(FilterIndex - 1);

            decimal minRate;
            if (!Ui.TryParseDecimal(MinRateText, out minRate))
                minRate = 0;

            Loads.Clear();
            foreach (Load l in data.Broker.GetAvailableLoads(type, minRate))
                Loads.Add(new LoadRow(l));
        }

        private void PostLoad()
        {
            double distance;
            double weight;
            decimal price;

            if (string.IsNullOrWhiteSpace(NewFrom) || string.IsNullOrWhiteSpace(NewTo))
            {
                main.ShowMessage("Вкажіть звідки і куди");
                return;
            }
            if (!Ui.TryParseDouble(NewDistance, out distance) || !Ui.TryParseDouble(NewWeight, out weight)
                || !Ui.TryParseDecimal(NewPrice, out price))
            {
                main.ShowMessage("Відстань, вага і ціна мають бути числами");
                return;
            }

            try
            {
                CargoType type = (CargoType)NewTypeIndex;
                Load load = data.Broker.PostLoad(NewFrom.Trim(), NewTo.Trim(), distance, weight, type, price);
                data.Save();

                NewFrom = "";
                NewTo = "";
                NewDistance = "";
                NewWeight = "";
                NewPrice = "";
                NewTypeIndex = 0;

                main.RefreshAll();
                main.ShowMessage($"Вантаж #{load.Id} викладено, ставка для перевізника {Ui.Money(load.CarrierRate)}");
            }
            catch (ArgumentException)
            {
                main.ShowMessage("Відстань, вага і ціна мають бути більше 0");
            }
        }
    }
}
