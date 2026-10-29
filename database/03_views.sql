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
