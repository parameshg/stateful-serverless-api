using Api.Domain;
using Api.Errors;

namespace Api.Repositories
{
    public class NullRepository : IRepository
    {
        private static readonly string NULL_STORAGE = "No storage configured";

        public Task<Ticker> GetCounter(string name)
        {
            throw new RepositoryException(NULL_STORAGE);
        }

        public Task<bool> CreateCounter(string name, int value)
        {
            throw new RepositoryException(NULL_STORAGE);
        }

        public Task<bool> UpdateCounter(string name, int value)
        {
            throw new RepositoryException(NULL_STORAGE);
        }

        public Task<bool> DeleteCounter(string name)
        {
            throw new RepositoryException(NULL_STORAGE);
        }
    }
}