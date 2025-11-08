using TextCheckIn.Data.Entities;

namespace TextCheckIn.Data.Repositories.Interfaces
{
    public interface IServiceRepository
    {
        List<Service> GetAllServices();
        Service? GetServiceById(int id);
        Service? GetServiceByUuid(Guid uuid);
        bool AddService(Service service);
        bool UpdateService(Service service);
        bool DeleteService(int id);
    }
}
