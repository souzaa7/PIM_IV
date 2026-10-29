using AprenderMais.Aplicacao.Interfaces;
using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Enums;

namespace AprenderMais.Infraestrutura.Memoria;

/// <summary>
/// Dados de demonstração. São os MESMOS do script database/06_dados_iniciais.sql,
/// para que o sistema se comporte igual no modo memória e no SQL Server.
/// Todos os usuários usam a senha 123456.
/// </summary>
public static class DadosIniciais
{
    public const string SenhaDemonstracao = "123456";

    private static DateTime Utc(int mes, int dia, int hora = 12, int minuto = 0) =>
        new(2026, mes, dia, hora, minuto, 0, DateTimeKind.Utc);

    /// <summary>Prazo de entrega: horário de Brasília (igual à coluna data_entrega).</summary>
    private static DateTime Prazo(int mes, int dia) => new(2026, mes, dia, 23, 59, 0);

    public static void Popular(BancoEmMemoria banco, IPasswordHasher hasher)
    {
        var hash = hasher.Hash(SenhaDemonstracao);

        // ---------------- Usuários (a ordem reproduz os ids do script SQL)
        AdicionarUsuario(banco, new Aluno { IdAluno = banco.ProximoId("aluno") },
            "12345678910", "Marina Costa", "aluno@aprendermais.com", hash, true);
        AdicionarUsuario(banco, new Secretaria { IdSecretaria = banco.ProximoId("secretaria") },
            "22233344455", "Rafael Martins", "secretaria@aprendermais.com", hash, true);
        AdicionarUsuario(banco, new Administrador { IdAdmin = banco.ProximoId("admin") },
            "99988877766", "Camila Rocha", "admin@aprendermais.com", hash, true);
        AdicionarUsuario(banco, new Aluno { IdAluno = banco.ProximoId("aluno") },
            "55512398700", "Bruno Lima", "bruno@aprendermais.com", hash, true);
        AdicionarUsuario(banco, new Aluno { IdAluno = banco.ProximoId("aluno") },
            "44432178999", "Ana Souza", "ana@aprendermais.com", hash, false);

        // ---------------- Conteúdos
        AdicionarConteudo(banco, "Engenharia de Software",
            "Requisitos, modelagem, DER, casos de uso, prototipação e qualidade de software.",
            "Engenharia", "Iniciante", "18h", 28, true);
        AdicionarConteudo(banco, "UX/UI Design",
            "Arquitetura de informação, usabilidade, acessibilidade e interfaces responsivas.",
            "Design", "Intermediário", "14h", 24, true);
        AdicionarConteudo(banco, "Microsoft 365",
            "Produtividade, colaboração, organização de documentos e boas práticas profissionais.",
            "Produtividade", "Iniciante", "10h", 18, true);
        AdicionarConteudo(banco, "Fundamentos de T.I.",
            "Base técnica para estudantes iniciantes em tecnologia da informação.",
            "Base técnica", "Iniciante", "12h", 21, false);

        // ---------------- Atividades
        AdicionarAtividade(banco, 1, "Modelagem de requisitos",
            "Relacionar caso de uso, DER e entidades principais do sistema.", Prazo(10, 10));
        AdicionarAtividade(banco, 1, "Diagrama de casos de uso",
            "Desenhar os casos de uso de um sistema de biblioteca.", Prazo(10, 20));
        AdicionarAtividade(banco, 2, "Análise de usabilidade",
            "Avaliar navegação, acessibilidade e clareza visual de uma tela.", Prazo(11, 20));
        AdicionarAtividade(banco, 2, "Protótipo de tela acessível",
            "Criar uma tela seguindo as diretrizes de contraste da WCAG.", Prazo(12, 4));
        AdicionarAtividade(banco, 3, "Organização com Microsoft 365",
            "Estruturar documentos, pastas e materiais de estudo.", Prazo(11, 27));
        AdicionarAtividade(banco, 3, "Planilha de controle de estudos",
            "Montar no Excel uma planilha com metas semanais de estudo.", Prazo(12, 11));

        // ---------------- Matrículas
        AdicionarMatricula(banco, 1, 1, StatusMatricula.Concluido, 100m, Utc(9, 2, 13), Utc(10, 21, 20));
        AdicionarMatricula(banco, 1, 2, StatusMatricula.EmAndamento, 50m, Utc(9, 2, 13, 5), null);
        AdicionarMatricula(banco, 1, 3, StatusMatricula.EmAndamento, 0m, Utc(9, 15), null);
        AdicionarMatricula(banco, 2, 1, StatusMatricula.EmAndamento, 0m, Utc(9, 8, 14), null);

        // ---------------- Respostas (a 4ª, do Bruno, fica pendente de correção)
        AdicionarResposta(banco, 1, 1, "Relacionei os casos de uso UC01 a UC05 com as tabelas usuario, matricula e resposta.", Utc(10, 5, 18));
        AdicionarResposta(banco, 2, 1, "Segue o diagrama com os atores Leitor, Bibliotecário e Administrador.", Utc(10, 18, 21, 30));
        AdicionarResposta(banco, 3, 1, "A tela de login tem bom contraste, mas o botão \"Entrar\" não tem foco visível.", Utc(10, 25, 19, 10));
        AdicionarResposta(banco, 1, 2, "Mapeei os requisitos funcionais RF01 a RF08 e relacionei com o DER.", Utc(10, 8, 22));

        // ---------------- Notas
        AdicionarNota(banco, 1, 9.5m, "Ótima relação entre requisitos e modelo de dados.", Utc(10, 12, 14));
        AdicionarNota(banco, 2, 8.0m, "Faltou o relacionamento de extensão entre dois casos.", Utc(10, 21, 20));
        AdicionarNota(banco, 3, 7.5m, "Boa observação sobre o foco. Sugira também uma correção.", Utc(10, 27, 15));

        // ---------------- Certificado
        banco.Certificados.Add(new Certificado
        {
            Id = banco.ProximoId("certificado"),
            IdAluno = 1,
            IdConteudo = 1,
            CodigoValidacao = "AM-2026-7F3A9C21",
            DataEmissao = Utc(10, 21, 20)
        });
    }

    private static void AdicionarUsuario(BancoEmMemoria banco, Usuario usuario,
        string cpf, string nome, string email, string hash, bool ativo)
    {
        usuario.Id = banco.ProximoId("usuario");
        usuario.Cpf = cpf;
        usuario.Nome = nome;
        usuario.Email = email;
        usuario.SenhaHash = hash;
        usuario.Ativo = ativo;
        usuario.DataCadastro = Utc(9, 1);
        banco.Usuarios.Add(usuario);
    }

    private static void AdicionarConteudo(BancoEmMemoria banco, string titulo, string descricao,
        string categoria, string nivel, string duracao, int aulas, bool ativo)
    {
        banco.Conteudos.Add(new Conteudo
        {
            Id = banco.ProximoId("conteudo"),
            Titulo = titulo,
            Descricao = descricao,
            Categoria = categoria,
            Nivel = nivel,
            Duracao = duracao,
            Aulas = aulas,
            Ativo = ativo,
            DataCriacao = Utc(9, 1),
            IdSecretaria = 1
        });
    }

    private static void AdicionarAtividade(BancoEmMemoria banco, int idConteudo, string titulo,
        string descricao, DateTime prazo)
    {
        banco.Atividades.Add(new Atividade
        {
            Id = banco.ProximoId("atividade"),
            IdConteudo = idConteudo,
            Titulo = titulo,
            Descricao = descricao,
            DataEntrega = prazo,
            NotaMaxima = 10m,
            DataCriacao = Utc(9, 1)
        });
    }

    private static void AdicionarMatricula(BancoEmMemoria banco, int idAluno, int idConteudo,
        StatusMatricula status, decimal progresso, DateTime data, DateTime? conclusao)
    {
        banco.Matriculas.Add(new Matricula
        {
            Id = banco.ProximoId("matricula"),
            IdAluno = idAluno,
            IdConteudo = idConteudo,
            Status = status,
            Progresso = progresso,
            DataMatricula = data,
            DataConclusao = conclusao
        });
    }

    private static void AdicionarResposta(BancoEmMemoria banco, int idAtividade, int idAluno,
        string texto, DateTime envio)
    {
        banco.Respostas.Add(new Resposta
        {
            Id = banco.ProximoId("resposta"),
            IdAtividade = idAtividade,
            IdAluno = idAluno,
            TextoResposta = texto,
            DataEnvio = envio
        });
    }

    private static void AdicionarNota(BancoEmMemoria banco, int idResposta, decimal valor,
        string feedback, DateTime data)
    {
        banco.Notas.Add(new Nota
        {
            Id = banco.ProximoId("nota"),
            IdResposta = idResposta,
            IdSecretaria = 1,
            Valor = valor,
            Feedback = feedback,
            DataAvaliacao = data
        });
    }
}
