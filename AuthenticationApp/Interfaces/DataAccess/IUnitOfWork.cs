using MongoDB.Driver;

namespace AuthenticationApp.Interfaces.DataAccess
{
    public interface IUnitOfWork : IDisposable
    {
        Task CommitAsync();
        void StartTransaction();
        IUserRepository Users { get; }
        IClientSessionHandle Session { get; }
    }
}
