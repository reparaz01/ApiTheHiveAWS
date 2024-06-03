using ApiTheHiveAWS.Repositories;
using ApiTheHiveAWS.Data;
using ApiTheHiveAWS.Helpers;
using Microsoft.EntityFrameworkCore;
using NSwag;
using NSwag.Generation.Processors.Security;
using ApiProyectoConjuntoAWSRedSocial.Helpers;
using Newtonsoft.Json;
using ApiProyectoConjuntoAWSRedSocial.Models;

namespace ApiTheHiveAWS;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public IConfiguration Configuration { get; }
    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        string secretKey = "AYMICUKI__348hhdxm3hdqhOCMHSNchcwu920301835245--.546423423!!!!jjhsjcuhjhnguJYEUHDJNUISYIENCNDUIMWECKQEUMCIUEHICMIWENLksh";
        string audience = "ApiTheHive";
        string issuer = "https://localhost:7117";


        string jsonSecrets =
            HelperSecretManager.GetSecretsAsync().GetAwaiter()
            .GetResult();
        KeysModel keysModel =
            JsonConvert.DeserializeObject<KeysModel>(jsonSecrets);
        services.AddSingleton<KeysModel>(x => keysModel);
        string connectionString = keysModel.MySql;


        HelperActionServicesOAuth helper = new HelperActionServicesOAuth(secretKey, audience, issuer);
        services.AddSingleton<HelperActionServicesOAuth>(helper);


        services.AddEndpointsApiExplorer();
        
        services.AddAuthentication
            (helper.GetAuthenticateSchema())
            .AddJwtBearer(helper.GetJwtBearerOptions());
        services.AddAuthorization();
        // ANTIGUO
        // string connectionString = builder.Configuration.GetConnectionString("SqlAzure");
        services.AddTransient<RepositoryTheHive>();
        services.AddDbContext<TheHiveContext>
            (options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        // REGISTRAMOS SWAGGER COMO SERVICIO
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "My API", Version = "v1" });
        });
        services.AddOpenApiDocument(document =>
        {
            document.Title = "API TheHive";
            document.Description = "Una api normal y corriente";
            document.AddSecurity("JWT", Enumerable.Empty<string>(),
                new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Copia y pega el Token en el campo 'Value:' así: Bearer {Token JWT}."
                }
            );
            document.OperationProcessors.Add(
            new AspNetCoreOperationSecurityScopeProcessor("JWT"));
        });


    }
    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseCors(options =>
        {
            options.AllowAnyOrigin();
        });

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(url: "swagger/v1/swagger.json", name: "Api TheHive");
            options.RoutePrefix = "";
        });

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

    }
}