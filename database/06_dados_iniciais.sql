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
