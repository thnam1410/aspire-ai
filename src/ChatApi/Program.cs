using ChatApi.Services.ChatCompletion;
using ServiceDefaults;
using Shared.Endpoint;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddServiceDefaults();

//EF

//MediaR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    // cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    // cfg.AddOpenBehavior(typeof(HandlerBehavior<,>));
});

//Validation related
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

//Endpoints related
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
}).EnableApiVersionBinding();

builder.Services.AddEndpoints(typeof(Program).Assembly);

//Metrics related
// builder.Services.AddSingleton<IActivityScope, ActivityScope>();
// builder.Services.AddSingleton<CommandHandlerMetrics>();
// builder.Services.AddSingleton<QueryHandlerMetrics>();

//AI Related
builder.AddChatCompletionService();

//Log related
builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

var versionedGroup = app
    .MapGroup("api/v{version:apiVersion}")
    .WithApiVersionSet(apiVersionSet);

app.MapDefaultEndpoints();
app.MapEndpoints(versionedGroup);

app.UseHttpsRedirection();

app.Run();

