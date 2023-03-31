using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Api.Repositories
{
    public static class DynamoExtensions
    {
        private const string TICKER = "counter";

        public static async Task Setup(this AmazonDynamoDBClient db)
        {
            if (!await db.TickerTableExists())
            {
                db.CreateTickerTable();
            }
        }

        private static async Task<bool> TableExists(this AmazonDynamoDBClient db, string name)
        {
            var result = false;

            try
            {
                var response = await db.DescribeTableAsync(name);

                if (response != null && response.Table != null)
                {
                    result = response.Table.TableName == name;
                }
            }
            catch (ResourceNotFoundException)
            {
                result = false;
            }

            return result;
        }

        private static async Task<bool> TickerTableExists(this AmazonDynamoDBClient db)
        {
            return await db.TableExists(TICKER);
        }

        private static void CreateTickerTable(this AmazonDynamoDBClient db)
        {
            var schema = new List<KeySchemaElement>
        {
            new KeySchemaElement { AttributeName = "Name", KeyType = KeyType.HASH },
            new KeySchemaElement { AttributeName = "Value", KeyType = KeyType.HASH }
        };

            var attributes = new List<AttributeDefinition>
        {
            new AttributeDefinition { AttributeName = "Name", AttributeType = ScalarAttributeType.S },
            new AttributeDefinition { AttributeName = "Value", AttributeType = ScalarAttributeType.N }
        };

            var response = db.CreateTableAsync(TICKER, schema, attributes, new ProvisionedThroughput(0, 0));
        }
    }
}