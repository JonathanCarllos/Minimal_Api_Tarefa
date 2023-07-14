using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("TarefasDb"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}

app.MapGet("/", () => "Olá mundo");

app.MapGet("frases", async () => await new HttpClient().GetStringAsync("https://www.pensador.com/pensamento_aleatorio/"));

app.MapGet("/tarefas", async (AppDbContext db) => { return await db.tarefas.ToListAsync();});
app.MapGet("/tarefas/{id}", async(int id,AppDbContext db) => await db.tarefas.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa) : Results.NotFound());
app.MapGet("/tarefas/concluida", async (AppDbContext db) =>  await db.tarefas.Where(t => t.ISConcluida).ToListAsync());
app.MapPost("/tarefas",async(Tarefa tarefa,AppDbContext db) => { db.tarefas.Add(tarefa); await db.SaveChangesAsync();
    return Results.Created($"/tarefas/{tarefa.Id}",tarefa);

});
app.MapPut("/tarefas/{id}", async (int id, Tarefa inputTarefa, AppDbContext db) =>
{
    var tarefa = await db.tarefas.FindAsync(id);
    if(tarefa is  null) return Results.NotFound();
    tarefa.Nome = inputTarefa.Nome;
    tarefa.ISConcluida = tarefa.ISConcluida;
    await db.SaveChangesAsync();
    return Results.NoContent();
}
);
app.MapDelete("/tarefas/{id}", async (int id, AppDbContext db) =>
{
    if (await db.tarefas.FindAsync(id) is Tarefa tarefa)
    {
        db.tarefas.Remove(tarefa);
        await db.SaveChangesAsync();
        Results.Ok(tarefa);
    }
    return Results.NotFound();
}
);


app.Run();

class Tarefa
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public bool ISConcluida { get; set; }
}

class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    public DbSet<Tarefa> tarefas => Set<Tarefa>();
}