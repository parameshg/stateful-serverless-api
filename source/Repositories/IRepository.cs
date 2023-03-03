using Counter.Domain;

namespace Counter.Repositories;

public interface IRepository
{
    Task<Ticker> GetCounter(string name);

    Task<bool> CreateCounter(string name, int value);

    Task<bool> UpdateCounter(string name, int value);

    Task<bool> DeleteCounter(string name);
}