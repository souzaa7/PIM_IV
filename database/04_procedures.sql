/* =============================================================
   APRENDER MAIS - Script 04: stored procedures
   Todas são chamadas pela API (camada Infraestrutura/SqlServer).
   ============================================================= */

USE AprenderMais;
GO

/* -------------------------------------------------------------
   sp_cadastrar_usuario
   Insere o usuário e a linha do perfil (aluno, secretaria ou
   administrador) na MESMA transação: ou grava os dois, ou nenhum.
   Usada em: UC02 (cadastro do aluno) e UC10 (cadastro de secretaria)
   ------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE sp_cadastrar_usuario
    @cpf        VARCHAR(11),
    @nome       NVARCHAR(100),
    @email      VARCHAR(100),
    @senha_hash VARCHAR(255),
    @perfil     VARCHAR(20),
    @id_usuario INT OUTPUT,
    @id_perfil  INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;   -- qualquer erro desfaz a transação inteira

    IF EXISTS (SELECT 1 FROM usuario WHERE email = @email)
        THROW 50001, N'Já existe um usuário com este e-mail.', 1;

    IF EXISTS (SELECT 1 FROM usuario WHERE cpf = @cpf)
        THROW 50002, N'Já existe um usuário com este CPF.', 1;

    BEGIN TRANSACTION;

        INSERT INTO usuario (cpf, nome, email, senha_hash, perfil)
        VALUES (@cpf, @nome, @email, @senha_hash, @perfil);

        SET @id_usuario = SCOPE_IDENTITY();

        IF @perfil = 'aluno'
            INSERT INTO aluno (id_usuario) VALUES (@id_usuario);
        ELSE IF @perfil = 'secretaria'
            INSERT INTO secretaria (id_usuario) VALUES (@id_usuario);
        ELSE
            INSERT INTO administrador (id_usuario) VALUES (@id_usuario);

        SET @id_perfil = SCOPE_IDENTITY();

    COMMIT TRANSACTION;
END
GO

/* -------------------------------------------------------------
   sp_calcular_progresso
   Retorna quantas atividades ATIVAS do conteúdo o aluno já teve
   avaliadas e o total de atividades ativas. A aplicação usa esses
   números para atualizar o progresso da matrícula.
   Usada em: UC06 (avaliar atividades)
   ------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE sp_calcular_progresso
    @id_aluno    INT,
    @id_conteudo INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        (SELECT COUNT(*)
           FROM atividade at
           JOIN resposta r ON r.id_atividade = at.id_atividade AND r.id_aluno = @id_aluno
           JOIN nota n     ON n.id_resposta  = r.id_resposta
          WHERE at.id_conteudo = @id_conteudo
            AND at.ativo = 1)                               AS avaliadas,
        (SELECT COUNT(*)
           FROM atividade at
          WHERE at.id_conteudo = @id_conteudo
            AND at.ativo = 1)                               AS total;
END
GO

/* -------------------------------------------------------------
   sp_emitir_certificado
   Emite o certificado do aluno no conteúdo. Se já existir, apenas
   devolve o certificado existente (pode ser chamada várias vezes).
   ------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE sp_emitir_certificado
    @id_aluno    INT,
    @id_conteudo INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM certificado
                    WHERE id_aluno = @id_aluno AND id_conteudo = @id_conteudo)
    BEGIN
        INSERT INTO certificado (id_aluno, id_conteudo, codigo_validacao)
        VALUES (@id_aluno,
                @id_conteudo,
                CONCAT('AM-', YEAR(SYSUTCDATETIME()), '-',
                       LEFT(REPLACE(CONVERT(VARCHAR(36), NEWID()), '-', ''), 8)));
    END

    SELECT id_certificado, id_aluno, id_conteudo, codigo_validacao, data_emissao
      FROM certificado
     WHERE id_aluno = @id_aluno AND id_conteudo = @id_conteudo;
END
GO

/* -------------------------------------------------------------
   sp_relatorio_resumo
   Indicadores gerais do painel do administrador.
   ------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE sp_relatorio_resumo
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        (SELECT COUNT(*) FROM usuario WHERE perfil = 'aluno')                    AS total_alunos,
        (SELECT COUNT(*) FROM usuario WHERE perfil = 'secretaria')               AS total_secretarias,
        (SELECT COUNT(*) FROM usuario WHERE ativo = 0)                           AS usuarios_inativos,
        (SELECT COUNT(*) FROM conteudo WHERE ativo = 1)                          AS conteudos_ativos,
        (SELECT COUNT(*) FROM matricula WHERE status = 'em_andamento')           AS matriculas_em_andamento,
        (SELECT COUNT(*) FROM matricula WHERE status = 'concluido')              AS matriculas_concluidas,
        (SELECT COUNT(*) FROM vw_respostas_pendentes)                            AS respostas_pendentes,
        (SELECT COUNT(*) FROM certificado)                                       AS certificados_emitidos,
        (SELECT CAST(ISNULL(AVG(valor), 0) AS DECIMAL(5,2)) FROM nota)           AS media_geral;
END
GO

/* -------------------------------------------------------------
   sp_relatorio_por_conteudo
   Desempenho de cada conteúdo: matriculados, concluídos e média.
   ------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE sp_relatorio_por_conteudo
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.id_conteudo,
           c.titulo,
           c.ativo,
           (SELECT COUNT(*) FROM matricula m
             WHERE m.id_conteudo = c.id_conteudo AND m.status <> 'cancelado')  AS matriculados,
           (SELECT COUNT(*) FROM matricula m
             WHERE m.id_conteudo = c.id_conteudo AND m.status = 'concluido')   AS concluidos,
           (SELECT CAST(ISNULL(AVG(b.nota), 0) AS DECIMAL(5,2)) FROM vw_boletim b
             WHERE b.id_conteudo = c.id_conteudo)                             AS media_notas
      FROM conteudo c
     ORDER BY c.titulo;
END
GO
