import { useState } from 'react'
import { usuarios } from '../../servicos/api'
import { Aviso } from '../../componentes/Estado'

/** Secretaria cadastra um aluno (por exemplo, quem pediu ajuda presencialmente). */
export default function CadastrarAluno() {
  return (
    <>
      <h1>Cadastrar aluno</h1>
      <FormularioUsuario perfil="aluno" />
    </>
  )
}

/** Formulário reaproveitado também pelo administrador para cadastrar secretaria (UC10). */
export function FormularioUsuario({ titulo, perfil, aoCadastrar }) {
  const inicial = { nome: '', cpf: '', email: '', senha: '' }
  const [dados, setDados] = useState(inicial)
  const [mensagem, setMensagem] = useState(null)

  const alterar = (campo) => (e) => setDados({ ...dados, [campo]: e.target.value })

  async function salvar(evento) {
    evento.preventDefault()
    try {
      const criado = await usuarios.criar(dados)
      setMensagem({ tipo: 'sucesso', texto: `${criado.nome} cadastrado como ${criado.perfil.toLowerCase()}. Repasse o e-mail e a senha provisória.` })
      setDados(inicial)
      aoCadastrar?.()
    } catch (e) {
      setMensagem({ tipo: 'erro', texto: e.message })
    }
  }

  return (
    <section className="formulario-central">
      {titulo && <h2>{titulo}</h2>}
      <form className="formulario" onSubmit={salvar}>
        <label>Nome completo<input value={dados.nome} onChange={alterar('nome')} required /></label>
        <label>CPF<input value={dados.cpf} onChange={alterar('cpf')} required inputMode="numeric" /></label>
        <label>E-mail de acesso<input type="email" value={dados.email} onChange={alterar('email')} required /></label>
        <label>Senha provisória<input value={dados.senha} onChange={alterar('senha')} required minLength={8} /></label>
        <Aviso mensagem={mensagem} />
        <button className="botao">Cadastrar {perfil}</button>
      </form>
    </section>
  )
}
