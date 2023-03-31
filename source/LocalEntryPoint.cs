namespace Counter;

public class LocalEntryPoint
{
    public static void Main(string[] args)
    {
        Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(cfg => cfg.UseStartup<Startup>()).Build().Run();
    }
}