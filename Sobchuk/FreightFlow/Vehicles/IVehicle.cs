using FreightFlow.Models;

namespace FreightFlow.Vehicles
{
    public interface IVehicle
    {
        string Id { get; }
        string Model { get; }
        double MaxWeight { get; }
        bool IsAvailable { get; }

        bool CanCarry(Load load);
        double CalculateFuelNeeded(double distance);
        double CalculateDeliveryTime(double distance);
        decimal CalculateCost(double distance);

        void AssignLoad(Load load);
        void Release();
        string GetInfo();
    }
}
