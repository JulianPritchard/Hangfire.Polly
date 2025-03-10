using Hangfire;
using Hangfire.Common;
using Hangfire.Polly.Example;
using Hangfire.Polly.Example.Services;
using Hangfire.PostgreSql;
// using AutomaticRetryAttribute = Hangfire.Polly.Example.AutomaticRetryAttribute;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IContainer, Container>();
builder.Services.AddSingleton<TestService>();
builder.Services.AddSingleton<JobActivator, InjectContextJobActivator>();
builder.Services.AddControllers();
builder.Services.AddSingleton<AutomaticRetryAttribute>();
builder.Services.AddSingleton<AutoRetryAttribute>();
builder.Services.AddSingleton<JobFilterAttributeFilterProvider>();
builder.Services.AddHangfire((provider, config) =>
        config.UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseFilterProvider(provider.GetRequiredService<JobFilterAttributeFilterProvider>())
            .UseFilter(provider.GetRequiredService<AutoRetryAttribute>())
            .UsePostgreSqlStorage(x
                => x.UseNpgsqlConnection(provider.GetRequiredService<IContainer>().ConnectionString()))
    );

builder.Services.AddHangfireServer(x => x.SchedulePollingInterval = TimeSpan.FromSeconds(1));

var app = builder.Build();
app.UseHangfireDashboard();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapControllers();

app.Run();
