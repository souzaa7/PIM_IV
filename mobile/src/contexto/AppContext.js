import { createContext, useCallback, useContext, useEffect, useState } from 'react'
import { api } from '../servicos/api'
import { armazenamento } from '../servicos/armazenamento'
import { enviarResposta, sincronizar } from '../servicos/sincronizacao'

// Estado global do app: login, dados do aluno e situação da sincronização.
const AppContext = createContext(null)

export function AppProvider({ children }) {
  const [logado, setLogado] = useState(false)
  const [iniciando, setIniciando] = useState(true)
  const [pacote, setPacote] = useState(null)        // dados do aluno (da API ou do celular)
  const [online, setOnline] = useState(true)
  const [fila, setFila] = useState([])              // respostas guardadas sem internet
  const [sincronizando, setSincronizando] = useState(false)
  const [problemas, setProblemas] = useState([])

  const atualizar = useCallback(async () => {
    setSincronizando(true)
    try {
      const resultado = await sincronizar()
      if (resultado.pacote) setPacote(resultado.pacote)
      setOnline(resultado.online)
      setProblemas(resultado.problemas)
    } catch {
      // token vencido: volta para o login
      await armazenamento.limparTudo()
      setLogado(false)
    } finally {
      setFila(await armazenamento.lerFila())
      setSincronizando(false)
    }
  }, [])

  // Ao abrir o app: mostra o que está salvo e sincroniza em seguida.
  useEffect(() => {
    ;(async () => {
      if (await armazenamento.lerToken()) {
        setPacote(await armazenamento.lerPacote())
        setLogado(true)
        atualizar()
      }
      setIniciando(false)
    })()
  }, [atualizar])

  async function entrar(email, senha) {
    const resposta = await api.login(email, senha)
    await armazenamento.salvarToken(resposta.accessToken)
    const usuario = await api.eu()
    if (usuario.perfil !== 'Aluno') {
      await armazenamento.limparTudo()
      throw new Error('O aplicativo é para alunos. Secretaria e administrador usam o site.')
    }
    setLogado(true)
    await atualizar()
  }

  async function sair() {
    await armazenamento.limparTudo()
    setPacote(null)
    setFila([])
    setLogado(false)
  }

  async function responder(idAtividade, texto) {
    const situacao = await enviarResposta(idAtividade, texto)
    await atualizar()
    return situacao
  }

  async function matricular(idConteudo) {
    await api.matricular(idConteudo)
    await atualizar()
  }

  return (
    <AppContext.Provider
      value={{
        logado, iniciando, pacote, online, fila, pendentes: fila.length, sincronizando, problemas,
        entrar, sair, atualizar, responder, matricular,
      }}
    >
      {children}
    </AppContext.Provider>
  )
}

export const useApp = () => useContext(AppContext)
