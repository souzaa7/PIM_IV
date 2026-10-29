import { useState } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { rotaInicial, useAuth } from '../../contexto/AuthContext'
import { Aviso } from '../../componentes/Estado'

/** UC01 - Acessar sistema. */
export default function Entrar() {
  const { usuario, entrar } = useAuth()
  const navegar = useNavigate()
  const [email, setEmail] = useState('')
  const [senha, setSenha] = useState('')
  const [mensagem, setMensagem] = useState(null)
  const [enviando, setEnviando] = useState(false)

  if (usuario) return <Navigate to={rotaInicial(usuario.perfil)} replace />

  async function aoEnviar(evento) {
    evento.preventDefault()
    setEnviando(true)
    setMensagem(null)
    try {
      const logado = await entrar(email, senha)
      navegar(rotaInicial(logado.perfil))
    } catch (e) {
      setMensagem({ tipo: 'erro', texto: e.message })
    } finally {
      setEnviando(false)
    }
  }

  return (
    <section className="formulario-central">
      <h1>Entrar</h1>
      <form onSubmit={aoEnviar} className="formulario">
        <label>
          E-mail
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required autoComplete="email" />
        </label>
        <label>
          Senha
          <input type="password" value={senha} onChange={(e) => setSenha(e.target.value)} required autoComplete="current-password" />
        </label>
        <Aviso mensagem={mensagem} />
        <button className="botao" disabled={enviando}>{enviando ? 'Entrando…' : 'Entrar'}</button>
      </form>
      <p>
        Ainda não tem conta? <Link to="/cadastro">Cadastre-se</Link>
      </p>
      <details className="dica">
        <summary>Contas de demonstração</summary>
        <p>Senha de todas: 123456</p>
        <ul>
          <li>Aluno: aluno@aprendermais.com</li>
          <li>Secretaria: secretaria@aprendermais.com</li>
          <li>Administrador: admin@aprendermais.com</li>
        </ul>
      </details>
    </section>
  )
}
