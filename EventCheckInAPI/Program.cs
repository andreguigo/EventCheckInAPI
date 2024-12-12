using Microsoft.OpenApi.Models;
using Dapper;
using MySqlConnector;
using EventCheckInAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Config CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:8080") // URL do frontend
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Event Check-In API", Version = "v1" });
});

builder.Services.AddScoped<MySqlConnection>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("EventCheckin");
    return new MySqlConnection(connectionString);
});

var app = builder.Build();

// Activate CORS
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Check-In API v1"));
}

app.MapGet("/api/eventos", async (MySqlConnection db) =>
{
    const string query = "SELECT * FROM Eventos";
    var eventos = await db.QueryAsync<Evento>(query);
    return Results.Ok(eventos);
});

app.MapGet("/api/eventos/{id}", async (Guid id, MySqlConnection db) =>
{
    const string query = "SELECT * FROM Eventos WHERE Id = @Id";
    var evento = await db.QueryAsync<Evento>(query, new { Id = id });
    return Results.Ok(evento);
});

app.MapPost("/api/eventos", async (Evento evento, MySqlConnection db) =>
{
    evento.Id = Guid.NewGuid();
    const string query = @"INSERT INTO Eventos (Id, Codigo, Nome, DataHoraInicio, DataHoraFim, PeriodoContinuo, Local, Pago, LimiteInscricoes) 
                          VALUES (@Id, @Codigo, @Nome, @DataHoraInicio, @DataHoraFim, @PeriodoContinuo, @Local, @Pago, @LimiteInscricoes)";
    await db.ExecuteAsync(query, evento);
    return Results.Created($"/api/eventos/{evento.Id}", evento);
});

app.MapGet("/api/eventos/{id}/disponibilidade", async (Guid id, MySqlConnection db) =>
{
    const string query = @"SELECT COUNT(*) FROM Inscricoes WHERE EventoId = @Id";
    var inscritos = await db.ExecuteScalarAsync<int>(query, new { Id = id });

    const string eventoQuery = @"SELECT LimiteInscricoes FROM Eventos WHERE Id = @Id";
    var limite = await db.ExecuteScalarAsync<int>(eventoQuery, new { Id = id });

    var disponibilidade = limite > inscritos;
    return Results.Ok(disponibilidade);
});

app.MapGet("/api/eventos/{id}/relatorio", async (Guid id, MySqlConnection db) =>
{
    const string query = @"SELECT u.NomeCompleto, u.Email, u.Telefone FROM Inscricoes i 
                          INNER JOIN Usuarios u ON i.UsuarioId = u.Id WHERE i.EventoId = @Id";
    var participantes = await db.QueryAsync<Usuario>(query, new { Id = id });
    return Results.Ok(participantes);
});

app.MapGet("/api/usuarios", async (MySqlConnection db) =>
{
    const string query = "SELECT * FROM Usuarios";
    var usuarios = await db.QueryAsync<Usuario>(query);
    return Results.Ok(usuarios);
});

app.MapGet("/api/usuarios/{email}", async (string email, MySqlConnection db) =>
{
    const string query = @"SELECT * FROM Usuarios WHERE Email = @Email";
    var usuario = await db.QueryAsync<Usuario>(query, new { Email = email });
    return Results.Ok(usuario);
});

app.MapPost("/api/usuarios", async (Usuario usuario, MySqlConnection db) =>
{
    usuario.Id = Guid.NewGuid();
    if (!usuario.ValidarDadosInscricao())
    {
        return Results.BadRequest("Dados do usuário inválidos.");
    }

    const string query = @"INSERT INTO Usuarios (Id, NomeCompleto, Email, Telefone) 
                          VALUES (@Id, @NomeCompleto, @Email, @Telefone)";
    await db.ExecuteAsync(query, usuario);
    return Results.Created($"/api/usuarios/{usuario.Id}", usuario);
});

app.MapPost("/api/inscricoes", async (Inscricao inscricao, MySqlConnection db) =>
{
    const string checkQuery = @"SELECT COUNT(*) FROM Inscricoes WHERE EventoId = @EventoId AND UsuarioId = @UsuarioId";
    var inscrito = await db.ExecuteScalarAsync<int>(checkQuery, new { inscricao.EventoId, inscricao.UsuarioId});
    if (inscrito > 0)
    {
        const string selectPinQuery = @"SELECT Pin FROM Inscricoes WHERE EventoId = @EventoId AND UsuarioId = @UsuarioId";
        var pin = await db.QueryFirstOrDefaultAsync<string>(selectPinQuery, new { inscricao.EventoId, inscricao.UsuarioId });
        return Results.Conflict($"Você já se inscreveu para este evento. Seu PIN é: {pin}");
    }

    inscricao.Id = Guid.NewGuid();
    inscricao.Pin = inscricao.CriarPin();
    const string insertQuery = @"INSERT INTO Inscricoes (Id, QrCode, Pin, EventoId, UsuarioId) 
                          VALUES (@Id, @QrCode, @Pin, @EventoId, @UsuarioId)";
    await db.ExecuteAsync(insertQuery, inscricao);
    return Results.Created($"/api/inscricoes/{inscricao.Id}", inscricao);
});

app.MapPost("/api/checkin", async (CheckInRequest request, MySqlConnection db) =>
{
    const string query = "SELECT * FROM Inscricoes WHERE EventoId = @EventoId AND (QrCode = @QrCode OR Pin = @Pin)";
    var inscricao = (await db.QueryAsync<Inscricao>(query, request)).FirstOrDefault();

    if (inscricao == null || !inscricao.RealizarCheckin(request.QrCode!, request.Pin!))
    {
        return Results.NotFound("Check-in failed: Invalid QrCode or Pin.");
    }

    return Results.Ok("Check-in successful.");
});

app.Run();
