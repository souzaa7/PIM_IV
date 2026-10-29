import { URL_API } from '../config'
import { armazenamento } from './armazenamento'

/** Erro de rede: a API não respondeu (sem internet ou servidor desligado). */
export class SemConexaoErro extends Error {}

/** Faz a requisição à API e devolve o JSON. Mesma ideia do site React. */
async function requisicao(caminho, { metodo = 'GET', corpo } = {}) {
  const cabecalhos = { 'Content-Type': 'application/json' }
  const token = await armazenamento.lerToken()
  if (token) cabecalhos.Authorization = `Bearer ${token}`

  let resposta
  try {
    resposta = await fetch(URL_API + caminho, {
      method: metodo,
      headers: cabecalhos,
      body: corpo ? JSON.stringify(corpo) : undefined,
    })
  } catch {
    throw new SemConexaoErro('Sem conexão com o servidor.')
  }

  if (resposta.status === 401) throw new Error('Sessão expirada. Entre novamente.')
  if (resposta.status === 204) return null

  const dados = await resposta.json().catch(() => null)
  if (!resposta.ok) throw new Error(dados?.mensagem ?? 'Erro ao processar a solicitação.')
  return dados
}

export const api = {
  login: (email, senha) => requisicao('/api/auth/login', { metodo: 'POST', corpo: { email, senha } }),
  cadastrar: (dados) => requisicao('/api/auth/cadastro', { metodo: 'POST', corpo: dados }),
  eu: () => requisicao('/api/auth/eu'),
  sincronizacao: () => requisicao('/api/sincronizacao'),
  matricular: (idConteudo) => requisicao('/api/matriculas', { metodo: 'POST', corpo: { idConteudo } }),
  enviarResposta: (idAtividade, texto) =>
    requisicao('/api/respostas', { metodo: 'POST', corpo: { idAtividade, texto } }),
}
