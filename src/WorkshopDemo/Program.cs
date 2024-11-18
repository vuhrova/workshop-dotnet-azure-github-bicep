using Azure.Identity;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using WorkshopDemo.Core.Common;
using WorkshopDemo.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

try
{
	Log.Information("Starting web host");

	// Add services to the container.
	builder.Host.UseSerilog((ctx, lc) => lc
		.MinimumLevel.Information()
		.MinimumLevel.Override("WorkshopDemo", LogEventLevel.Debug)
		.MinimumLevel.Override("System", LogEventLevel.Error)
		.WriteTo.Console(
			outputTemplate:
			"[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
			theme: AnsiConsoleTheme.Literate)
		.WriteTo.ApplicationInsights(
			new TelemetryConfiguration
			{ ConnectionString = ctx.Configuration.GetConnectionString("ApplicationInsights") },
			TelemetryConverter.Traces)
		.Enrich.FromLogContext());

	builder.Services.AddEndpointsApiExplorer();
	builder.Services.AddOpenApi();
	builder.Services.AddControllers();
	builder.Services.AddHealthChecks()
		.AddCheck<WorkshopDemoHealthCheck>(nameof(WorkshopDemoHealthCheck));
	builder.Services.AddSingleton<IFileService, FileService>();
	builder.Services.AddSingleton<IVersionService, VersionService>();

	builder.Configuration.AddAzureKeyVault(
		new Uri($"https://kv-vuhrova-{builder.Environment.EnvironmentName}.vault.azure.net/"),
		new DefaultAzureCredential());

	builder.Services.AddApplicationInsightsTelemetry(options =>
	{
		options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
	});



	var app = builder.Build();

	// Configure the HTTP request pipeline.
	if (app.Environment.IsDevelopment())
	{
		app.MapOpenApi();
	}

	app.UseHttpsRedirection();

	app.MapHealthChecks("/api/healthz");

	app.MapControllers();

	app.Run();
}
catch (Exception ex)
{
	Log.Fatal(ex, "Application terminated");
}
finally
{
	Log.CloseAndFlush();
}

