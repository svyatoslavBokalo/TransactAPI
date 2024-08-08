using TransactionAPI.Models;

namespace TransactionAPI.Services.Interfaces
{
    public interface IGeneralTimeService
    {
        public Task AddOrUpdateGeneralTimeAsync(GeneralTimeModel generalTime);
    }
}
