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
