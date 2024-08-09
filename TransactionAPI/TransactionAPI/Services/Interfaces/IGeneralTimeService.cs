using TransactionAPI.Models;

namespace TransactionAPI.Services.Interfaces
{
    //interface for GeneralTimeService
    public interface IGeneralTimeService
    {
        public Task AddOrUpdateGeneralTimeAsync(GeneralTimeModel generalTime);
    }
}
