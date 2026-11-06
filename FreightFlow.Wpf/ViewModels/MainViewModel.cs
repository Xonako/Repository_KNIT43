using System.Windows.Input;
using FreightFlow.Wpf.Services;

namespace FreightFlow.Wpf.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly DataService data;
        private string statusMessage;

        public DashboardViewModel Dashboard { get; }
        public FleetViewModel Fleet { get; }
        public LoadBoardViewModel LoadBoard { get; }
        public DispatchViewModel Dispatch { get; }

        public ICommand ResetCommand { get; }

        public MainViewModel()
        {
            data = new DataService();

            Dashboard = new DashboardViewModel(data);
            Fleet = new FleetViewModel(data, this);
            LoadBoard = new LoadBoardViewModel(data, this);
            Dispatch = new DispatchViewModel(data, this);

            ResetCommand = new RelayCommand(Reset);

            RefreshAll();
            ShowMessage("Дані завантажено: " + data.FilePath);
        }

        public string StatusMessage
        {
            get { return statusMessage; }
            set { SetProperty(ref statusMessage, value); }
        }

        public void ShowMessage(string text)
        {
            StatusMessage = text;
        }

        public void RefreshAll()
        {
            Dashboard.Refresh();
            Fleet.Refresh();
            LoadBoard.Refresh();
            Dispatch.Refresh();
        }

        private void Reset()
        {
            data.Reset();
            RefreshAll();
            ShowMessage("Дані скинуто до початкових");
        }
    }
}
