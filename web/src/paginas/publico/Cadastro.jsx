import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { autenticacao } from '../../servicos/api'
import { useAuth } from '../../contexto/AuthContext'
import { Aviso } from '../../componentes/Estado'

/** UC02 - Realizar cadastro (o visitante cria a própria conta de aluno). */
export default function Cadastro() {
  const { entrar } = useAuth()
  const navegar = useNavigate()
  const [dados, setDados] = useState({ nome: '', cpf: '', email: '', senha: '' })
  const [mensagem, setMensagem] = useState(null)
  const [enviando, setEnviando] = useState(false)

  const alterar = (campo) => (e) => setDados({ ...dados, [campo]: e.target.value })

  async function aoEnviar(evento) {
    evento.preventDefault()
    setEnviando(true)
    setMensagem(null)
    try {
      await autenticacao.cadastrar(dados)
      await entrar(dados.email, dados.senha) // já entra direto após o cadastro
      navegar('/aluno/cursos')
    } catch (e) {
      setMensagem({ tipo: 'erro', texto: e.message })
      setEnviando(false)
    }
  }

  return (
    <section className="formulario-central">
      <h1>Criar conta de aluno</h1>
      <form onSubmit={aoEnviar} className="formulario">
        <label>
          Nome completo
          <input value={dados.nome} onChange={alterar('nome')} required autoComplete="name" />
        </label>
        <label>
          CPF
          <input value={dados.cpf} onChange={alterar('cpf')} required inputMode="numeric" placeholder="000.000.000-00" />
        </label>
        <label>
          E-mail
          <input type="email" value={dados.email} onChange={alterar('email')} required autoComplete="email" />
        </label>
        <label>
          Senha (mínimo 8 caracteres)
          <input type="password" value={dados.senha} onChange={alterar('senha')} required minLength={8} autoComplete="new-password" />
        </label>
        <Aviso mensagem={mensagem} />
        <button className="botao" disabled={enviando}>{enviando ? 'Criando conta…' : 'Criar conta'}</button>
      </form>
      <p>
        Já tem conta? <Link to="/entrar">Entrar</Link>
      </p>
    </section>
  )
}
