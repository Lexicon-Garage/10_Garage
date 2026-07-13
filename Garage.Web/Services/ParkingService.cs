using Garage.Web.Repository;

namespace Garage.Web.Services
{
    public class ParkingService
    {
        private readonly IParkingRepository _repository;
        public ParkingService(
            IParkingRepository repository)
        {
            _repository = repository;
        }

    }
}
