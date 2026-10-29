import { createContext, useContext, useEffect, useState } from 'react'
import { autenticacao, token } from '../servicos/api'

// Guarda o usuário logado e deixa disponível para todas as telas.
const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [usuario, setUsuario] = useState(null)
  const [verificando, setVerificando] = useState(true)

  // Ao abrir o site: se já existe um token salvo, busca os dados do usuário.
  useEffect(() => {
    if (!token.obter()) {
      setVerificando(false)
      return
    }
    autenticacao
      .eu()
      .then(setUsuario)
      .catch(() => token.remover())
      .finally(() => setVerificando(false))
  }, [])

  // A API avisa quando o token venceu (resposta 401).
  useEffect(() => {
    const aoExpirar = () => setUsuario(null)
    window.addEventListener('sessao-expirada', aoExpirar)
    return () => window.removeEventListener('sessao-expirada', aoExpirar)
  }, [])

  async function entrar(email, senha) {
    const resposta = await autenticacao.login(email, senha)
    token.salvar(resposta.accessToken)
    const dados = await autenticacao.eu()
    setUsuario(dados)
    return dados
  }

  function sair() {
    token.remover()
    setUsuario(null)
  }

  return (
    <AuthContext.Provider value={{ usuario, verificando, entrar, sair }}>
      {children}
    </AuthContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export const useAuth = () => useContext(AuthContext)

/** Página inicial de cada perfil depois do login. */
// eslint-disable-next-line react-refresh/only-export-components
export function rotaInicial(perfil) {
  if (perfil === 'Aluno') return '/aluno'
  if (perfil === 'Secretaria') return '/secretaria'
  return '/admin'
}
