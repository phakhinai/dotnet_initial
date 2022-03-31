namespace dotnet_hero.Installers
{
    public interface IInstallers
    {
         void InstallServices(IServiceCollection services, IConfiguration configuration);
    }
}