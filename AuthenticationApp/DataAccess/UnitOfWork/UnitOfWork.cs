using AuthenticationApp.DataAccess.Context;
using AuthenticationApp.Interfaces.DataAccess;
using MongoDB.Driver;

namespace AuthenticationApp.DataAccess.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IMongoDatabase _database;
        private readonly IUserRepository _userRepository;
        private IClientSessionHandle _session;

        public IUserRepository Users => _userRepository;
        public IClientSessionHandle Session => _session;

        public UnitOfWork(IMongoDbContext context, IUserRepository userRepository)
        {
            _database = context.Database;
            _userRepository = userRepository;
        }

        public void StartTransaction()
        {
            _session = _database.Client.StartSession();
            _session.StartTransaction();
        }

        public async Task CommitAsync()
        {
            if (_session != null)
            {
                try
                {
                    await _session.CommitTransactionAsync();
                }
                catch (Exception e)
                {
                    await _session.AbortTransactionAsync();
                    throw new Exception("Transaction failed", e);
                }
                finally
                {
                    _session.Dispose();
                    _session = null;
                }
            }
        }

        public void Dispose()
        {
            _session?.Dispose();
        }

    }
}
