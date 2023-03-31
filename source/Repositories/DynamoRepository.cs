using Amazon.DynamoDBv2.DataModel;
using Counter.Domain;
using EnsureThat;

namespace Counter.Repositories;

public class DynamoRepository : IRepository
{
    [DynamoDBTable("statefull-serverless-api")]
    private class Entity
    {
        [DynamoDBHashKey]
        [DynamoDBProperty("name")]
        public string Name { get; set; }

        [DynamoDBProperty("value")]
        public int Value { get; set; }
    }

    private IDynamoDBContext db;

    public DynamoRepository(IDynamoDBContext context)
    {
        db = EnsureArg.IsNotNull(context);
    }

    public async Task<Ticker> GetCounter(string name)
    {
        var result = new Ticker();

        var entity = await db.LoadAsync<Entity>(name);

        if (entity != null)
        {
            result = new Ticker { Name = entity.Name, Value = entity.Value };
        }

        return result;
    }

    public async Task<bool> CreateCounter(string name, int value)
    {
        var result = false;

        await db.SaveAsync(new Entity { Name = name, Value = value });

        result = true;

        return result;
    }

    public async Task<bool> UpdateCounter(string name, int value)
    {
        var result = false;

        await db.SaveAsync(new Entity { Name = name, Value = value });

        result = true;

        return result;
    }

    public async Task<bool> DeleteCounter(string name)
    {
        var result = false;

        await db.DeleteAsync<Entity>(name);

        result = true;

        return result;
    }
}