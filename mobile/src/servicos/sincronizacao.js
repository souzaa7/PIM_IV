import { api, SemConexaoErro } from './api'
import { armazenamento } from './armazenamento'

// =====================================================================
// SINCRONIZAÇÃO DE DADOS (funcionar sem internet)
//
// 1) Ao abrir o app, as telas mostram a última cópia salva no celular.
// 2) Respostas feitas sem internet entram numa FILA no celular.
// 3) Ao sincronizar: primeiro envia a fila para a API, depois baixa
//    tudo de novo (GET /api/sincronizacao) e salva no celular.
// =====================================================================

/** Envia uma resposta. Sem internet, guarda na fila e devolve 'fila'. */
export async function enviarResposta(idAtividade, texto) {
  try {
    await api.enviarResposta(idAtividade, texto)
    return 'enviada'
  } catch (erro) {
    if (!(erro instanceof SemConexaoErro)) throw erro
    const fila = await armazenamento.lerFila()
    fila.push({ idAtividade, texto, criadaEm: new Date().toISOString() })
    await armazenamento.salvarFila(fila)
    return 'fila'
  }
}

/** Tenta enviar as respostas que ficaram na fila. Devolve as mensagens de erro, se houver. */
async function enviarFila() {
  const fila = await armazenamento.lerFila()
  const restantes = []
  const problemas = []

  for (const item of fila) {
    try {
      await api.enviarResposta(item.idAtividade, item.texto)
    } catch (erro) {
      if (erro instanceof SemConexaoErro) {
        restantes.push(item) // continua sem internet: tenta na próxima vez
      } else {
        problemas.push(erro.message) // a API recusou (ex.: prazo encerrado): sai da fila
      }
    }
  }

  await armazenamento.salvarFila(restantes)
  return problemas
}

/**
 * Sincroniza com a API. Retorna { pacote, online, problemas }.
 * Se estiver sem internet, devolve a cópia salva e online = false.
 */
export async function sincronizar() {
  try {
    const problemas = await enviarFila()
    const pacote = await api.sincronizacao()
    await armazenamento.salvarPacote(pacote)
    return { pacote, online: true, problemas }
  } catch (erro) {
    if (!(erro instanceof SemConexaoErro)) throw erro
    return { pacote: await armazenamento.lerPacote(), online: false, problemas: [] }
  }
}
