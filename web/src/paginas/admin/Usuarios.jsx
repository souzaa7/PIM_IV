import { useState } from 'react'
import { usuarios } from '../../servicos/api'
import { useCarregar } from '../../servicos/useCarregar'
import { useAuth } from '../../contexto/AuthContext'
import { Aviso, Estado } from '../../componentes/Estado'
import { FormularioUsuario } from '../secretaria/CadastrarAluno'

/** UC10 (cadastrar secretaria) e UC11 (desativar cadastros). */
export default function Usuarios() {
  const { usuario: eu } = useAuth()
  const { dados, erro, carregando, recarregar } = useCarregar(usuarios.listar)
  const [busca, setBusca] = useState('')
  const [mensagem, setMensagem] = useState(null)
  const [cadastrando, setCadastrando] = useState(false)

  const filtrados = dados?.filter((u) =>
    `${u.nome} ${u.email}`.toLowerCase().includes(busca.toLowerCase()))

  async function alternar(u) {
    if (u.ativo && !window.confirm(`Desativar ${u.nome}? Ele não conseguirá mais entrar.`)) return
    try {
      await usuarios.alterarAtivo(u.id, !u.ativo)
      setMensagem({ tipo: 'sucesso', texto: u.ativo ? 'Usuário desativado com sucesso.' : 'Usuário reativado.' })
      recarregar()
    } catch (e) {
      setMensagem({ tipo: 'erro', texto: e.message })
    }
  }

  return (
    <>
      <div className="titulo-com-acao">
        <h1>Usuários</h1>
        <button type="button" className="botao" onClick={() => setCadastrando(!cadastrando)} aria-expanded={cadastrando}>
          {cadastrando ? 'Fechar cadastro' : 'Cadastrar secretaria'}
        </button>
      </div>

      {cadastrando && <FormularioUsuario titulo="Nova secretaria" perfil="secretaria" aoCadastrar={recarregar} />}

      <Aviso mensagem={mensagem} />
      <label className="busca">
        Pesquisar por nome ou e-mail
        <input type="search" value={busca} onChange={(e) => setBusca(e.target.value)} />
      </label>

      <Estado carregando={carregando} erro={erro} vazio={filtrados?.length === 0} mensagemVazio="Nenhum usuário encontrado." />
      {filtrados?.length > 0 && (
        <div className="tabela-rolagem">
          <table>
            <thead>
              <tr>
                <th scope="col">Nome</th>
                <th scope="col">E-mail</th>
                <th scope="col">Perfil</th>
                <th scope="col">Situação</th>
                <th scope="col"><span className="visualmente-oculto">Ações</span></th>
              </tr>
            </thead>
            <tbody>
              {filtrados.map((u) => (
                <tr key={u.id} className={u.ativo ? '' : 'apagado'}>
                  <td>{u.nome}</td>
                  <td>{u.email}</td>
                  <td>{u.perfil}</td>
                  <td>{u.situacao}</td>
                  <td className="celula-acoes">
                    {u.id !== eu.id && (
                      <button type="button" className="botao-texto" onClick={() => alternar(u)}>
                        {u.ativo ? 'Desativar' : 'Ativar'}
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  )
}
