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
