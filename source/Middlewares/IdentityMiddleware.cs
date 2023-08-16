using EnsureThat;
using Flurl;
using Flurl.Http;
using System.Security.Claims;

namespace Api.Middlewares
{
    public class IdentityMiddleware : IMiddleware
    {
        public class User
        {
            public string sub { get; set; }

            public string nickname { get; set; }

            public string name { get; set; }

            public string picture { get; set; }

            public string updated_at { get; set; }
        }

        private IConfiguration Configuration { get; }

        private ILogger<IdentityMiddleware> Logger { get; }

        public IdentityMiddleware(ILogger<IdentityMiddleware> logger, IConfiguration configuration)
        {
            Logger = EnsureArg.IsNotNull(logger);

            Configuration = EnsureArg.IsNotNull(configuration);
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                var headers = context.Request.Headers["Authorization"];

                if (headers.Count.Equals(1))
                {
                    var token = headers.SingleOrDefault();

                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        token = token.Replace("Bearer", string.Empty).Trim();

                        var user = await $"https://{Configuration["Auth0:Domain"]}/".AppendPathSegment("userinfo").WithOAuthBearerToken(token).GetJsonAsync<User>();

                        if (user != null)
                        {
                            context.User = new ClaimsPrincipal(new ClaimsIdentity[]
                            {
                                new ClaimsIdentity(new Claim[]
                                {
                                    new Claim(ClaimTypes.NameIdentifier, user.sub),
                                    new Claim(ClaimTypes.Name, user.name),
                                    new Claim("nickname", user.nickname),
                                    new Claim("picture", user.picture),
                                }, "Basic")
                            });
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Logger.LogError(error.Message, error);
            }
            finally
            {
                await next(context);
            }
        }
    }
}