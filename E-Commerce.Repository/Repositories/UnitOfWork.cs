using E_Commerce.Repository.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Threading;

namespace E_Commerce.Repository.Repositories;
public class UnitOfWork(StoreContext _storeContext, ILogger<StoreContext> logger) : IUnitOfWork
{
    private readonly Hashtable _repositories = [];
    private bool _disposed = false;

    //polymorphism
    CancellationToken _cancellationToken = default;
    public DbConnection Connection { get; set; }//how to create it

    #region Private Variables

    private IsolationLevel _dbTransactionIsolationLevel = IsolationLevel.ReadCommitted;
    private bool _dbConnectionOpenedLocally;
    private DbTransaction _transaction = null;
    private readonly object _lockObject = new();//for multiple users accessing at the same time 
    private int _dbTransactionNestLevel = -1;
    private readonly HashSet<DbContext> _dbContexts = [];
    private const int MaxRecursionDepth = 10; 

    #endregion


    #region IUnitOfWork Implementation

    public async Task<T> UsingStrategy<T>(Func<Task<T>> action)
    {
        var strategy = _dbContexts.First().Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            return await action();
        });
    }
    public void Begin()
    {
        Begin(IsolationLevel.ReadCommitted);
    }

    public void Begin(IsolationLevel isolationLevel)
    {
        lock (_lockObject)
        {
            if (_transaction == null)
            {
                _dbTransactionIsolationLevel = isolationLevel;
                OpenConnection();
                _transaction = Connection.BeginTransaction(_dbTransactionIsolationLevel);
                foreach (var dbContext in _dbContexts) dbContext.Database.UseTransaction(_transaction);
            }

            _dbTransactionNestLevel++;
        }
    }

    public async Task Commit()
    {
        if (_dbTransactionNestLevel > 0)
        {
            _dbTransactionNestLevel--;
            return;
        }

        try
        {
            await _transaction.CommitAsync(_cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "error happened during commiting the changes by unitOfWork , going to rollback now ... ");

            await Rollback();

            throw;
        }
        finally
        {
            Reset();
        }
    }

    public async Task Rollback()
    {
        if (_dbTransactionNestLevel == -1)
        {
            logger.LogInformation("metigate rolling back again");
            return;
        }

        try
        {
            await _transaction.RollbackAsync(_cancellationToken);
        }
        finally
        {
            Reset();
        }
    }

    #endregion




    #region Helpers

    private void Reset()
    {
        _transaction.Dispose();
        _dbTransactionIsolationLevel = IsolationLevel.ReadCommitted;
        _dbTransactionNestLevel = -1;
        CloseConnection();
    }

    private void CloseConnection()
    {
        if (Connection.State == ConnectionState.Open && _dbConnectionOpenedLocally)
        {
            Connection.Close();
            _dbConnectionOpenedLocally = false;
        }
    }

    private void OpenConnection()
    {
        if (Connection.State == ConnectionState.Open) return;

        Connection.Open();
        _dbConnectionOpenedLocally = true;
    }

    public void RegisterContext(DbContext context)
    {
        lock (_lockObject)
        {
            _dbContexts.Add(context);
            if (_transaction != null) context.Database.UseTransaction(_transaction);
        }
    }

    #endregion

    public IGenericRepository<T> Repository<T>() where T : BaseEntity
    {
        var key = typeof(T).Name;

        if (!_repositories.ContainsKey(key))
        {
            var repositoryType = typeof(GenericRepository<>);

            var repository = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _storeContext);

            _repositories.Add(key, repository);
        }

        return (GenericRepository<T>)_repositories[key]!;
    }


    public async Task<int> CompleteAsync() => await _storeContext.SaveChangesAsync();


    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_storeContext != null)
        {
            await _storeContext.DisposeAsync();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _storeContext?.Dispose();
            }

            _disposed = true;
        }
    }

    ~UnitOfWork()
    {
        Dispose(false);
    }
}
