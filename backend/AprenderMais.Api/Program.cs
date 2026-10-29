using AprenderMais.Api.Configuracao;
using AprenderMais.Aplicacao.Interfaces;
using AprenderMais.Aplicacao.Servicos;
using AprenderMais.Aplicacao.Sessao;
using AprenderMais.Infraestrutura;
using AprenderMais.Infraestrutura.Relogio;
using AprenderMais.Infraestrutura.Seguranca;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// =====================================================================
// 1) BANCO DE DADOS
//    appsettings.json -> "Banco:Tipo": "Memoria" (demonstração) ou "SqlServer"
// =====================================================================
IPasswordHasher hasher = new BCryptPasswordHasher();
var repositorios = FabricaRepositorios.Criar(
    builder.Configuration["Banco:Tipo"],
    builder.Configuration.GetConnectionString("AprenderMais"),
    hasher);

builder.Services.AddSingleton(hasher);
builder.Services.AddSingleton<IRelogio, RelogioSistema>();
builder.Services.AddSingleton(repositorios.Usuarios);
builder.Services.AddSingleton(repositorios.Conteudos);
builder.Services.AddSingleton(repositorios.Matriculas);
builder.Services.AddSingleton(repositorios.Atividades);
builder.Services.AddSingleton(repositorios.Respostas);
builder.Services.AddSingleton(repositorios.Notas);
builder.Services.AddSingleton(repositorios.Certificados);
builder.Services.AddSingleton(repositorios.Relatorios);

// =====================================================================
// 2) SERVIÇOS DA APLICAÇÃO (regras de negócio) - um por requisição
// =====================================================================
builder.Services.AddScoped<SessaoUsuario>();
builder.Services.AddScoped<AutenticacaoService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<ConteudoService>();
builder.Services.AddScoped<MatriculaService>();
builder.Services.AddScoped<AtividadeService>();
builder.Services.AddScoped<RespostaService>();
builder.Services.AddScoped<NotaService>();
builder.Services.AddScoped<CertificadoService>();
builder.Services.AddScoped<RelatorioService>();

// =====================================================================
// 3) AUTENTICAÇÃO (token Bearer) E AUTORIZAÇÃO (perfis)
// =====================================================================
builder.Services
    .AddAuthentication(BearerTokenDefaults.AuthenticationScheme)
    .AddBearerToken(opcoes => opcoes.BearerTokenExpiration = TimeSpan.FromHours(8));
builder.Services.AddAuthorization();

// =====================================================================
// 4) CORS - permite que o site React (outra porta) chame a API
// =====================================================================
var origens = builder.Configuration.GetSection("Cors:OrigensPermitidas").Get<string[]>() ?? [];
builder.Services.AddCors(opcoes =>
    opcoes.AddDefaultPolicy(politica => politica
        .WithOrigins(origens)
        .AllowAnyHeader()
        .AllowAnyMethod()));

// =====================================================================
// 5) CONTROLLERS, SWAGGER (documentação) E HEALTH CHECK (monitoramento)
// =====================================================================
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opcoes =>
{
    opcoes.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Aprender Mais - API",
        Version = "v1",
        Description = "API REST da plataforma educacional Aprender Mais (PIM IV - UNIP)."
    });

    // Botão "Authorize" do Swagger para colar o token.
    var esquema = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        In = ParameterLocation.Header,
        Description = "Cole aqui o accessToken recebido em /api/auth/login",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    opcoes.AddSecurityDefinition("Bearer", esquema);
    opcoes.AddSecurityRequirement(new OpenApiSecurityRequirement { { esquema, Array.Empty<string>() } });

    var xml = Path.Combine(AppContext.BaseDirectory, "AprenderMais.Api.xml");
    if (File.Exists(xml))
        opcoes.IncludeXmlComments(xml);
});

var app = builder.Build();

// =====================================================================
// PIPELINE: a ordem importa!
// =====================================================================
app.UseMiddleware<TratamentoDeErros>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/saude");
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Logger.LogInformation("Banco de dados em uso: {Banco}", builder.Configuration["Banco:Tipo"] ?? "Memoria");

app.Run();
