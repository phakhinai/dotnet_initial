using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using dotnet_hero.Installers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.InstallServiceInAssembly(builder.Configuration);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{
    builder.RegisterAssemblyTypes(Assembly.GetEntryAssembly())
    .Where(t => t.Name.EndsWith("Service"))
    .AsImplementedInterfaces();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowAllOrigins");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
