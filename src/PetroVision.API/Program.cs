using PetroVision.BLL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddBusinessLayer(builder.Configuration.GetConnectionString("DefaultConnection")!);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.Services.InitializeDatabaseAsync();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
