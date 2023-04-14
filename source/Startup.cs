using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Api.Pipelines;
using Api.Repositories;
using EnsureThat;
using MediatR;

namespace Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = EnsureArg.IsNotNull(configuration);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(cfg =>
            {
                cfg.AddLambdaLogger(new LambdaLoggerOptions
                {
                    IncludeCategory = true,
                    IncludeEventId = true,
                    IncludeException = true,
                    IncludeLogLevel = true,
                    IncludeNewline = true,
                    IncludeScopes = true
                });
            });

            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AWS_REGION")))
            {
                var region = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION"));

                if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")) && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")))
                {
                    services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(new EnvironmentVariablesAWSCredentials(), region));
                }
                else
                {
                    // ServiceURL vs RegionEndpoint are mutually exclusive
                    //services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(new AmazonDynamoDBConfig { ServiceURL = "http://localhost:8000" }));
                    services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(region));
                }

                services.AddSingleton<IDynamoDBContext, DynamoDBContext>(i => new DynamoDBContext(i.GetService<IAmazonDynamoDB>()));

                services.AddTransient<IRepository, DynamoRepository>();
            }
            else
            {
                services.AddTransient<IRepository, NullRepository>();
            }

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining(typeof(Startup)));

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddControllers();

            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger();

                app.UseSwaggerUI();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}