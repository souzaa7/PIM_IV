/* =============================================================
   APRENDER MAIS - SCRIPT COMPLETO DO BANCO (SQL Server)
   Junção dos scripts 01 a 06, nesta ordem. Pode ser executado
   de uma vez no SQL Server Management Studio ou no Azure Data Studio.
   ============================================================= */

-- >>>>>>>>>>>>>>>>>>>>>>>>>>>>>> 01_criar_banco.sql
/* =============================================================
   APRENDER MAIS - Script 01: criação do banco de dados
   SGBD: Microsoft SQL Server 2019+ (ou Azure SQL Managed Instance)

   Azure SQL Database: crie o banco "AprenderMais" pelo portal, conecte
   direto nele e rode os scripts 02 a 06 sem as linhas "USE AprenderMais".
   ============================================================= */

IF DB_ID('AprenderMais') IS NULL
BEGIN
    CREATE DATABASE AprenderMais;
END
GO

USE AprenderMais;
GO

-- >>>>>>>>>>>>>>>>>>>>>>>>>>>>>> 02_tabelas.sql
/* =============================================================
   APRENDER MAIS - Script 02: tabelas, chaves e índices
   Modelo físico do banco (ver docs/07-banco-de-dados.md)
   ============================================================= */

USE AprenderMais;
GO

/* ---------- USUARIO: dados comuns aos três perfis ---------- */
CREATE TABLE usuario (
    id_usuario       INT IDENTITY(1,1) NOT NULL,
    cpf              VARCHAR(11)   NOT NULL,          -- somente números
    nome             NVARCHAR(100) NOT NULL,
    email            VARCHAR(100)  NOT NULL,
    senha_hash       VARCHAR(255)  NOT NULL,          -- BCrypt (nunca a senha pura)
    perfil           VARCHAR(20)   NOT NULL,
    ativo            BIT           NOT NULL CONSTRAINT df_usuario_ativo DEFAULT 1,
    data_cadastro    DATETIME2(0)  NOT NULL CONSTRAINT df_usuario_cadastro DEFAULT SYSUTCDATETIME(),
    data_atualizacao DATETIME2(0)  NOT NULL CONSTRAINT df_usuario_atualizacao DEFAULT SYSUTCDATETIME(),
    CONSTRAINT pk_usuario PRIMARY KEY (id_usuario),
    CONSTRAINT uq_usuario_cpf UNIQUE (cpf),
    CONSTRAINT uq_usuario_email UNIQUE (email),
    CONSTRAINT ck_usuario_perfil CHECK (perfil IN ('aluno', 'secretaria', 'admin'))
);
GO

/* ---------- Especializações (herança Usuario -> Aluno/Secretaria/Admin) ---------- */
CREATE TABLE aluno (
    id_aluno   INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NOT NULL,
    CONSTRAINT pk_aluno PRIMARY KEY (id_aluno),
    CONSTRAINT uq_aluno_usuario UNIQUE (id_usuario),
    CONSTRAINT fk_aluno_usuario FOREIGN KEY (id_usuario) REFERENCES usuario (id_usuario)
);
GO

CREATE TABLE secretaria (
    id_secretaria INT IDENTITY(1,1) NOT NULL,
    id_usuario    INT NOT NULL,
    CONSTRAINT pk_secretaria PRIMARY KEY (id_secretaria),
    CONSTRAINT uq_secretaria_usuario UNIQUE (id_usuario),
    CONSTRAINT fk_secretaria_usuario FOREIGN KEY (id_usuario) REFERENCES usuario (id_usuario)
);
GO

CREATE TABLE administrador (
    id_admin   INT IDENTITY(1,1) NOT NULL,
    id_usuario INT NOT NULL,
    CONSTRAINT pk_administrador PRIMARY KEY (id_admin),
    CONSTRAINT uq_administrador_usuario UNIQUE (id_usuario),
    CONSTRAINT fk_administrador_usuario FOREIGN KEY (id_usuario) REFERENCES usuario (id_usuario)
);
GO

/* ---------- CONTEUDO: cursos publicados pela secretaria ---------- */
CREATE TABLE conteudo (
    id_conteudo      INT IDENTITY(1,1) NOT NULL,
    titulo           NVARCHAR(150) NOT NULL,
    descricao        NVARCHAR(MAX) NULL,
    categoria        NVARCHAR(80)  NOT NULL,
    nivel            NVARCHAR(40)  NOT NULL CONSTRAINT df_conteudo_nivel DEFAULT N'Iniciante',
    duracao          NVARCHAR(30)  NOT NULL CONSTRAINT df_conteudo_duracao DEFAULT N'0h',
    aulas            INT           NOT NULL CONSTRAINT df_conteudo_aulas DEFAULT 0,
    ativo            BIT           NOT NULL CONSTRAINT df_conteudo_ativo DEFAULT 1,
    data_criacao     DATETIME2(0)  NOT NULL CONSTRAINT df_conteudo_criacao DEFAULT SYSUTCDATETIME(),
    data_atualizacao DATETIME2(0)  NOT NULL CONSTRAINT df_conteudo_atualizacao DEFAULT SYSUTCDATETIME(),
    id_secretaria    INT           NOT NULL,
    CONSTRAINT pk_conteudo PRIMARY KEY (id_conteudo),
    CONSTRAINT ck_conteudo_aulas CHECK (aulas >= 0),
    CONSTRAINT fk_conteudo_secretaria FOREIGN KEY (id_secretaria) REFERENCES secretaria (id_secretaria)
);
GO

/* ---------- ATIVIDADE: avaliações de cada conteúdo ---------- */
CREATE TABLE atividade (
    id_atividade INT IDENTITY(1,1) NOT NULL,
    titulo       NVARCHAR(150) NOT NULL,
    descricao    NVARCHAR(MAX) NULL,
    data_entrega DATETIME2(0)  NOT NULL,              -- horário de Brasília
    nota_maxima  DECIMAL(5,2)  NOT NULL CONSTRAINT df_atividade_nota_maxima DEFAULT 10.00,
    ativo        BIT           NOT NULL CONSTRAINT df_atividade_ativo DEFAULT 1,
    data_criacao DATETIME2(0)  NOT NULL CONSTRAINT df_atividade_criacao DEFAULT SYSUTCDATETIME(),
    id_conteudo  INT           NOT NULL,
    CONSTRAINT pk_atividade PRIMARY KEY (id_atividade),
    CONSTRAINT ck_atividade_nota_maxima CHECK (nota_maxima > 0),
    CONSTRAINT fk_atividade_conteudo FOREIGN KEY (id_conteudo) REFERENCES conteudo (id_conteudo)
);
GO

/* ---------- MATRICULA: aluno inscrito em um conteúdo ---------- */
CREATE TABLE matricula (
    id_matricula   INT IDENTITY(1,1) NOT NULL,
    id_aluno       INT          NOT NULL,
    id_conteudo    INT          NOT NULL,
    status         VARCHAR(20)  NOT NULL CONSTRAINT df_matricula_status DEFAULT 'em_andamento',
    progresso      DECIMAL(5,2) NOT NULL CONSTRAINT df_matricula_progresso DEFAULT 0,
    data_matricula DATETIME2(0) NOT NULL CONSTRAINT df_matricula_data DEFAULT SYSUTCDATETIME(),
    data_conclusao DATETIME2(0) NULL,
    CONSTRAINT pk_matricula PRIMARY KEY (id_matricula),
    CONSTRAINT uq_matricula_aluno_conteudo UNIQUE (id_aluno, id_conteudo),
    CONSTRAINT ck_matricula_status CHECK (status IN ('em_andamento', 'concluido', 'cancelado')),
    CONSTRAINT ck_matricula_progresso CHECK (progresso BETWEEN 0 AND 100),
    CONSTRAINT fk_matricula_aluno FOREIGN KEY (id_aluno) REFERENCES aluno (id_aluno),
    CONSTRAINT fk_matricula_conteudo FOREIGN KEY (id_conteudo) REFERENCES conteudo (id_conteudo)
);
GO

/* ---------- RESPOSTA: entrega do aluno para uma atividade ---------- */
CREATE TABLE resposta (
    id_resposta    INT IDENTITY(1,1) NOT NULL,
    texto_resposta NVARCHAR(MAX) NOT NULL,
    arquivo_url    VARCHAR(500)  NULL,
    data_envio     DATETIME2(0)  NOT NULL CONSTRAINT df_resposta_envio DEFAULT SYSUTCDATETIME(),
    id_atividade   INT NOT NULL,
    id_aluno       INT NOT NULL,
    CONSTRAINT pk_resposta PRIMARY KEY (id_resposta),
    CONSTRAINT uq_resposta_aluno_atividade UNIQUE (id_aluno, id_atividade),
    CONSTRAINT fk_resposta_atividade FOREIGN KEY (id_atividade) REFERENCES atividade (id_atividade),
    CONSTRAINT fk_resposta_aluno FOREIGN KEY (id_aluno) REFERENCES aluno (id_aluno)
);
GO

/* ---------- NOTA: correção feita pela secretaria (1 nota por resposta) ---------- */
CREATE TABLE nota (
    id_nota        INT IDENTITY(1,1) NOT NULL,
    valor          DECIMAL(5,2)  NOT NULL,
    feedback       NVARCHAR(MAX) NULL,
    data_avaliacao DATETIME2(0)  NOT NULL CONSTRAINT df_nota_data DEFAULT SYSUTCDATETIME(),
    id_resposta    INT NOT NULL,
    id_secretaria  INT NOT NULL,
    CONSTRAINT pk_nota PRIMARY KEY (id_nota),
    CONSTRAINT uq_nota_resposta UNIQUE (id_resposta),
    CONSTRAINT fk_nota_resposta FOREIGN KEY (id_resposta) REFERENCES resposta (id_resposta),
    CONSTRAINT fk_nota_secretaria FOREIGN KEY (id_secretaria) REFERENCES secretaria (id_secretaria)
);
GO

/* ---------- CERTIFICADO: emitido quando a matrícula é concluída ---------- */
CREATE TABLE certificado (
    id_certificado   INT IDENTITY(1,1) NOT NULL,
    id_aluno         INT          NOT NULL,
    id_conteudo      INT          NOT NULL,
    codigo_validacao VARCHAR(40)  NOT NULL,
    data_emissao     DATETIME2(0) NOT NULL CONSTRAINT df_certificado_data DEFAULT SYSUTCDATETIME(),
    CONSTRAINT pk_certificado PRIMARY KEY (id_certificado),
    CONSTRAINT uq_certificado_codigo UNIQUE (codigo_validacao),
    CONSTRAINT uq_certificado_aluno_conteudo UNIQUE (id_aluno, id_conteudo),
    CONSTRAINT fk_certificado_aluno FOREIGN KEY (id_aluno) REFERENCES aluno (id_aluno),
    CONSTRAINT fk_certificado_conteudo FOREIGN KEY (id_conteudo) REFERENCES conteudo (id_conteudo)
);
GO

/* ---------- LOG_AUDITORIA: preenchida pelos triggers ---------- */
CREATE TABLE log_auditoria (
    id_log      INT IDENTITY(1,1) NOT NULL,
    tabela      VARCHAR(50)   NOT NULL,
    id_registro INT           NOT NULL,
    acao        VARCHAR(50)   NOT NULL,
    descricao   NVARCHAR(300) NULL,
    data_hora   DATETIME2(0)  NOT NULL CONSTRAINT df_log_data DEFAULT SYSUTCDATETIME(),
    CONSTRAINT pk_log_auditoria PRIMARY KEY (id_log)
);
GO

/* ---------- Índices para as consultas mais usadas (otimização) ---------- */
CREATE INDEX ix_usuario_perfil        ON usuario (perfil);
CREATE INDEX ix_conteudo_ativo        ON conteudo (ativo);
CREATE INDEX ix_atividade_conteudo    ON atividade (id_conteudo);
CREATE INDEX ix_matricula_aluno       ON matricula (id_aluno);
CREATE INDEX ix_resposta_atividade    ON resposta (id_atividade);
CREATE INDEX ix_certificado_aluno     ON certificado (id_aluno);
CREATE INDEX ix_log_tabela_registro   ON log_auditoria (tabela, id_registro);
GO

-- >>>>>>>>>>>>>>>>>>>>>>>>>>>>>> 03_views.sql
/* =============================================================
   APRENDER MAIS - Script 03: views (consultas prontas)
   ============================================================= */

USE AprenderMais;
GO

/* Usuário com o id do seu perfil (usado pela API para montar Aluno/Secretaria/Administrador) */
CREATE OR ALTER VIEW vw_usuario_perfil AS
SELECT u.id_usuario,
       u.cpf,
       u.nome,
       u.email,
       u.senha_hash,
       u.perfil,
       u.ativo,
       u.data_cadastro,
       a.id_aluno,
       s.id_secretaria,
       ad.id_admin
FROM usuario u
LEFT JOIN aluno a          ON a.id_usuario  = u.id_usuario
LEFT JOIN secretaria s     ON s.id_usuario  = u.id_usuario
LEFT JOIN administrador ad ON ad.id_usuario = u.id_usuario;
GO

/* Respostas que ainda não receberam nota (fila de correção da secretaria) */
CREATE OR ALTER VIEW vw_respostas_pendentes AS
SELECT r.id_resposta,
       r.texto_resposta,
       r.arquivo_url,
       r.data_envio,
       r.id_aluno,
       u.nome        AS nome_aluno,
       at.id_atividade,
       at.titulo     AS titulo_atividade,
       at.nota_maxima,
       c.id_conteudo,
       c.titulo      AS titulo_conteudo
FROM resposta r
JOIN aluno a      ON a.id_aluno      = r.id_aluno
JOIN usuario u    ON u.id_usuario    = a.id_usuario
JOIN atividade at ON at.id_atividade = r.id_atividade
JOIN conteudo c   ON c.id_conteudo   = at.id_conteudo
WHERE NOT EXISTS (SELECT 1 FROM nota n WHERE n.id_resposta = r.id_resposta);
GO

/* Boletim: todas as notas de todos os alunos */
CREATE OR ALTER VIEW vw_boletim AS
SELECT r.id_aluno,
       u.nome        AS nome_aluno,
       c.id_conteudo,
       c.titulo      AS titulo_conteudo,
       at.id_atividade,
       at.titulo     AS titulo_atividade,
       at.nota_maxima,
       n.valor       AS nota,
       n.feedback,
       n.data_avaliacao
FROM nota n
JOIN resposta r   ON r.id_resposta   = n.id_resposta
JOIN atividade at ON at.id_atividade = r.id_atividade
JOIN conteudo c   ON c.id_conteudo   = at.id_conteudo
JOIN aluno a      ON a.id_aluno      = r.id_aluno
JOIN usuario u    ON u.id_usuario    = a.id_usuario;
GO

-- >>>>>>>>>>>>>>>>>>>>>>>>>>>>>> 04_procedures.sql
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

-- >>>>>>>>>>>>>>>>>>>>>>>>>>>>>> 05_triggers.sql
/* =============================================================
   APRENDER MAIS - Script 05: triggers
   Regras que o próprio banco garante, mesmo que alguém altere os
   dados fora da aplicação (por exemplo, direto no SSMS).
   ============================================================= */

USE AprenderMais;
GO

/* -------------------------------------------------------------
   trg_usuario_auditoria
   Ao atualizar um usuário: atualiza data_atualizacao e registra
   no log quando ele é ATIVADO ou DESATIVADO (UC11).
   ------------------------------------------------------------- */
CREATE OR ALTER TRIGGER trg_usuario_auditoria
ON usuario
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Evita repetição: o UPDATE abaixo na própria tabela não dispara o trigger de novo.
    IF TRIGGER_NESTLEVEL(@@PROCID) > 1 RETURN;

    UPDATE u
       SET data_atualizacao = SYSUTCDATETIME()
      FROM usuario u
      JOIN inserted i ON i.id_usuario = u.id_usuario;

    INSERT INTO log_auditoria (tabela, id_registro, acao, descricao)
    SELECT 'usuario',
           i.id_usuario,
           CASE WHEN i.ativo = 1 THEN 'ATIVADO' ELSE 'DESATIVADO' END,
           CONCAT(N'Usuário ', i.nome, N' (', i.perfil, N') foi ',
                  CASE WHEN i.ativo = 1 THEN N'ativado.' ELSE N'desativado.' END)
      FROM inserted i
      JOIN deleted d ON d.id_usuario = i.id_usuario
     WHERE i.ativo <> d.ativo;
END
GO

/* -------------------------------------------------------------
   trg_conteudo_desativado
   Ao desativar um conteúdo (UC09), as atividades dele também são
   desativadas, e a ação fica registrada no log.
   ------------------------------------------------------------- */
CREATE OR ALTER TRIGGER trg_conteudo_desativado
ON conteudo
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Evita repetição: o UPDATE abaixo na própria tabela não dispara o trigger de novo.
    IF TRIGGER_NESTLEVEL(@@PROCID) > 1 RETURN;

    UPDATE c
       SET data_atualizacao = SYSUTCDATETIME()
      FROM conteudo c
      JOIN inserted i ON i.id_conteudo = c.id_conteudo;

    UPDATE at
       SET ativo = 0
      FROM atividade at
      JOIN inserted i ON i.id_conteudo = at.id_conteudo
      JOIN deleted d  ON d.id_conteudo = i.id_conteudo
     WHERE d.ativo = 1 AND i.ativo = 0;

    INSERT INTO log_auditoria (tabela, id_registro, acao, descricao)
    SELECT 'conteudo',
           i.id_conteudo,
           CASE WHEN i.ativo = 1 THEN 'ATIVADO' ELSE 'DESATIVADO' END,
           CONCAT(N'Conteúdo "', i.titulo, N'" foi ',
                  CASE WHEN i.ativo = 1 THEN N'ativado.' ELSE N'desativado (atividades também).' END)
      FROM inserted i
      JOIN deleted d ON d.id_conteudo = i.id_conteudo
     WHERE i.ativo <> d.ativo;
END
GO

/* -------------------------------------------------------------
   trg_nota_validar
   Impede gravar nota negativa ou maior que a nota máxima da
   atividade (UC06). O THROW cancela o INSERT/UPDATE.
   ------------------------------------------------------------- */
CREATE OR ALTER TRIGGER trg_nota_validar
ON nota
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
          FROM inserted i
          JOIN resposta r   ON r.id_resposta   = i.id_resposta
          JOIN atividade at ON at.id_atividade = r.id_atividade
         WHERE i.valor < 0 OR i.valor > at.nota_maxima
    )
        THROW 50010, N'A nota deve estar entre 0 e a nota máxima da atividade.', 1;
END
GO

/* -------------------------------------------------------------
   trg_resposta_validar
   Só aceita resposta de atividade ativa, dentro do prazo, e de
   aluno com matrícula em andamento no conteúdo (UC04).
   ------------------------------------------------------------- */
CREATE OR ALTER TRIGGER trg_resposta_validar
ON resposta
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- data_entrega está em horário de Brasília (UTC-3)
    IF EXISTS (
        SELECT 1
          FROM inserted i
          JOIN atividade at ON at.id_atividade = i.id_atividade
         WHERE at.ativo = 0
            OR DATEADD(HOUR, -3, SYSUTCDATETIME()) > at.data_entrega
    )
        THROW 50020, N'A atividade está inativa ou o prazo de entrega já terminou.', 1;

    IF EXISTS (
        SELECT 1
          FROM inserted i
          JOIN atividade at ON at.id_atividade = i.id_atividade
         WHERE NOT EXISTS (SELECT 1 FROM matricula m
                            WHERE m.id_aluno = i.id_aluno
                              AND m.id_conteudo = at.id_conteudo
                              AND m.status = 'em_andamento')
    )
        THROW 50021, N'O aluno precisa estar matriculado no conteúdo para enviar a resposta.', 1;
END
GO

-- >>>>>>>>>>>>>>>>>>>>>>>>>>>>>> 06_dados_iniciais.sql
/* =============================================================
   APRENDER MAIS - Script 06: dados iniciais (demonstração)
   Senha de TODOS os usuários: 123456 (guardada como hash BCrypt)
   Os mesmos dados existem em AprenderMais.Infraestrutura/Memoria/DadosIniciais.cs
   ============================================================= */

USE AprenderMais;
GO

DECLARE @hash VARCHAR(255) = '$2a$11$65JrTZP5up8TekPvypiALuUiirrMOCw8REy.G9KL0r1BTCxCC8E6a';
DECLARE @id_usuario INT, @id_perfil INT;

/* ---------- Usuários (a procedure cria usuário + perfil) ---------- */
EXEC sp_cadastrar_usuario '12345678910', N'Marina Costa',   'aluno@aprendermais.com',      @hash, 'aluno',      @id_usuario OUTPUT, @id_perfil OUTPUT; -- aluno 1
EXEC sp_cadastrar_usuario '22233344455', N'Rafael Martins', 'secretaria@aprendermais.com', @hash, 'secretaria', @id_usuario OUTPUT, @id_perfil OUTPUT; -- secretaria 1
EXEC sp_cadastrar_usuario '99988877766', N'Camila Rocha',   'admin@aprendermais.com',      @hash, 'admin',      @id_usuario OUTPUT, @id_perfil OUTPUT; -- admin 1
EXEC sp_cadastrar_usuario '55512398700', N'Bruno Lima',     'bruno@aprendermais.com',      @hash, 'aluno',      @id_usuario OUTPUT, @id_perfil OUTPUT; -- aluno 2
EXEC sp_cadastrar_usuario '44432178999', N'Ana Souza',      'ana@aprendermais.com',        @hash, 'aluno',      @id_usuario OUTPUT, @id_perfil OUTPUT; -- aluno 3

-- Ana fica inativa (o trigger trg_usuario_auditoria registra no log)
UPDATE usuario SET ativo = 0 WHERE email = 'ana@aprendermais.com';
GO

/* ---------- Conteúdos (cadastrados pela secretaria 1) ---------- */
INSERT INTO conteudo (titulo, descricao, categoria, nivel, duracao, aulas, ativo, id_secretaria) VALUES
(N'Engenharia de Software', N'Requisitos, modelagem, DER, casos de uso, prototipação e qualidade de software.', N'Engenharia',    N'Iniciante',     N'18h', 28, 1, 1),
(N'UX/UI Design',           N'Arquitetura de informação, usabilidade, acessibilidade e interfaces responsivas.', N'Design',       N'Intermediário', N'14h', 24, 1, 1),
(N'Microsoft 365',          N'Produtividade, colaboração, organização de documentos e boas práticas profissionais.', N'Produtividade', N'Iniciante', N'10h', 18, 1, 1),
(N'Fundamentos de T.I.',    N'Base técnica para estudantes iniciantes em tecnologia da informação.', N'Base técnica', N'Iniciante',     N'12h', 21, 0, 1);
GO

/* ---------- Atividades (prazo em horário de Brasília) ---------- */
INSERT INTO atividade (titulo, descricao, data_entrega, nota_maxima, id_conteudo) VALUES
(N'Modelagem de requisitos',          N'Relacionar caso de uso, DER e entidades principais do sistema.', '2026-10-10T23:59:00', 10, 1),
(N'Diagrama de casos de uso',         N'Desenhar os casos de uso de um sistema de biblioteca.',          '2026-10-20T23:59:00', 10, 1),
(N'Análise de usabilidade',           N'Avaliar navegação, acessibilidade e clareza visual de uma tela.', '2026-11-20T23:59:00', 10, 2),
(N'Protótipo de tela acessível',      N'Criar uma tela seguindo as diretrizes de contraste da WCAG.',    '2026-12-04T23:59:00', 10, 2),
(N'Organização com Microsoft 365',    N'Estruturar documentos, pastas e materiais de estudo.',           '2026-11-27T23:59:00', 10, 3),
(N'Planilha de controle de estudos',  N'Montar no Excel uma planilha com metas semanais de estudo.',     '2026-12-11T23:59:00', 10, 3);
GO

/* ---------- Matrículas ---------- */
INSERT INTO matricula (id_aluno, id_conteudo, status, progresso, data_matricula, data_conclusao) VALUES
(1, 1, 'concluido',    100, '2026-09-02T13:00:00', '2026-10-21T20:00:00'),
(1, 2, 'em_andamento',  50, '2026-09-02T13:05:00', NULL),
(1, 3, 'em_andamento',   0, '2026-09-15T12:00:00', NULL),
(2, 1, 'em_andamento',   0, '2026-09-08T14:00:00', NULL);
GO

/* ---------- Respostas ----------
   O trigger de prazo é desligado só durante a carga, porque algumas
   respostas de demonstração são de atividades com prazo já encerrado. */
DISABLE TRIGGER trg_resposta_validar ON resposta;

INSERT INTO resposta (texto_resposta, arquivo_url, data_envio, id_atividade, id_aluno) VALUES
(N'Relacionei os casos de uso UC01 a UC05 com as tabelas usuario, matricula e resposta.', NULL, '2026-10-05T18:00:00', 1, 1),
(N'Segue o diagrama com os atores Leitor, Bibliotecário e Administrador.',                NULL, '2026-10-18T21:30:00', 2, 1),
(N'A tela de login tem bom contraste, mas o botão "Entrar" não tem foco visível.',       NULL, '2026-10-25T19:10:00', 3, 1),
(N'Mapeei os requisitos funcionais RF01 a RF08 e relacionei com o DER.',                  NULL, '2026-10-08T22:00:00', 1, 2);

ENABLE TRIGGER trg_resposta_validar ON resposta;
GO

/* ---------- Notas (a resposta 4, do Bruno, fica pendente de correção) ---------- */
INSERT INTO nota (valor, feedback, data_avaliacao, id_resposta, id_secretaria) VALUES
(9.5, N'Ótima relação entre requisitos e modelo de dados.', '2026-10-12T14:00:00', 1, 1),
(8.0, N'Faltou o relacionamento de extensão entre dois casos.', '2026-10-21T20:00:00', 2, 1),
(7.5, N'Boa observação sobre o foco. Sugira também uma correção.', '2026-10-27T15:00:00', 3, 1);
GO

/* ---------- Certificado (Marina concluiu Engenharia de Software) ---------- */
INSERT INTO certificado (id_aluno, id_conteudo, codigo_validacao, data_emissao)
VALUES (1, 1, 'AM-2026-7F3A9C21', '2026-10-21T20:00:00');
GO
