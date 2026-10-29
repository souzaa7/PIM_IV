// =====================================================================
// Comunicação com a API REST (.NET).
// Todas as telas usam as funções daqui; nenhuma tela chama fetch direto.
// =====================================================================

const URL_API = import.meta.env.VITE_API_URL ?? 'http://localhost:5000'
const CHAVE_TOKEN = 'aprendermais.token'

export const token = {
  obter: () => localStorage.getItem(CHAVE_TOKEN),
  salvar: (valor) => localStorage.setItem(CHAVE_TOKEN, valor),
  remover: () => localStorage.removeItem(CHAVE_TOKEN),
}

/**
 * Faz a requisição HTTP e devolve o JSON da resposta.
 * Se a API responder com erro, lança um Error com a mensagem enviada por ela.
 */
async function requisicao(caminho, { metodo = 'GET', corpo } = {}) {
  const cabecalhos = { 'Content-Type': 'application/json' }
  const tokenAtual = token.obter()
  if (tokenAtual) cabecalhos.Authorization = `Bearer ${tokenAtual}`

  let resposta
  try {
    resposta = await fetch(URL_API + caminho, {
      method: metodo,
      headers: cabecalhos,
      body: corpo ? JSON.stringify(corpo) : undefined,
    })
  } catch {
    throw new Error('Não foi possível conectar à API. Verifique se ela está rodando.')
  }

  if (resposta.status === 401) {
    token.remover()
    window.dispatchEvent(new Event('sessao-expirada'))
    throw new Error('Sua sessão expirou. Entre novamente.')
  }
  if (resposta.status === 403) throw new Error('Seu perfil não tem acesso a esta função.')
  if (resposta.status === 204) return null

  const dados = await resposta.json().catch(() => null)
  if (!resposta.ok) throw new Error(dados?.mensagem ?? 'Erro ao processar a solicitação.')
  return dados
}

// ------------------------------------------------------------ autenticação
export const autenticacao = {
  login: (email, senha) => requisicao('/api/auth/login', { metodo: 'POST', corpo: { email, senha } }),
  cadastrar: (dados) => requisicao('/api/auth/cadastro', { metodo: 'POST', corpo: dados }),
  eu: () => requisicao('/api/auth/eu'),
}

// ------------------------------------------------------------ conteúdos e atividades
export const conteudos = {
  listarAtivos: () => requisicao('/api/conteudos'),
  listarTodos: () => requisicao('/api/conteudos/todos'),
  criar: (dados) => requisicao('/api/conteudos', { metodo: 'POST', corpo: dados }),
  editar: (id, dados) => requisicao(`/api/conteudos/${id}`, { metodo: 'PUT', corpo: dados }),
  alterarAtivo: (id, ativo) => requisicao(`/api/conteudos/${id}/ativo`, { metodo: 'PATCH', corpo: { ativo } }),
  atividades: (id) => requisicao(`/api/conteudos/${id}/atividades`),
}

export const atividades = {
  minhas: () => requisicao('/api/atividades/minhas'),
  criar: (dados) => requisicao('/api/atividades', { metodo: 'POST', corpo: dados }),
}

// ------------------------------------------------------------ aluno
export const matriculas = {
  minhas: () => requisicao('/api/matriculas/minhas'),
  matricular: (idConteudo) => requisicao('/api/matriculas', { metodo: 'POST', corpo: { idConteudo } }),
  cancelar: (id) => requisicao(`/api/matriculas/${id}`, { metodo: 'DELETE' }),
}

export const respostas = {
  enviar: (idAtividade, texto, arquivoUrl) =>
    requisicao('/api/respostas', { metodo: 'POST', corpo: { idAtividade, texto, arquivoUrl } }),
  pendentes: () => requisicao('/api/respostas/pendentes'),
  avaliar: (id, valor, feedback) =>
    requisicao(`/api/respostas/${id}/nota`, { metodo: 'POST', corpo: { valor, feedback } }),
}

export const notas = {
  minhas: () => requisicao('/api/notas/minhas'),
}

export const certificados = {
  meus: () => requisicao('/api/certificados/meus'),
  validar: (codigo) => requisicao(`/api/certificados/validar/${encodeURIComponent(codigo)}`),
}

// ------------------------------------------------------------ administração
export const usuarios = {
  listar: () => requisicao('/api/usuarios'),
  criar: (dados) => requisicao('/api/usuarios', { metodo: 'POST', corpo: dados }),
  alterarAtivo: (id, ativo) => requisicao(`/api/usuarios/${id}/ativo`, { metodo: 'PATCH', corpo: { ativo } }),
}

export const relatorios = {
  resumo: () => requisicao('/api/relatorios/resumo'),
  porConteudo: () => requisicao('/api/relatorios/conteudos'),
}
