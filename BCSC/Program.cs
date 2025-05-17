using BCSC.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddTransient<IServicioEmail, ServicioEmail>();
builder.Services.AddSingleton<EthereumService>();

var app = builder.Build();

// Inicializar nodos
NodeManager.InitializeNodes(); // Asegurarse de que los nodos existan
var synchronizedBlockchain = NodeManager.SynchronizeBlockchain();
TokenSystem.SetBlockchain(synchronizedBlockchain);


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
