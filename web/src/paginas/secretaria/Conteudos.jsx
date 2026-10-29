import { useState } from 'react'
import { conteudos } from '../../servicos/api'
import { useCarregar } from '../../servicos/useCarregar'
import { Aviso, Estado } from '../../componentes/Estado'

const VAZIO = { titulo: '', descricao: '', categoria: '', nivel: 'Iniciante', duracao: '', aulas: 0 }

/** UC07, UC08 e UC09 - Adicionar, editar e desativar conteúdos. */
export default function Conteudos() {
  const { dados, erro, carregando, recarregar } = useCarregar(conteudos.listarTodos)
  const [formulario, setFormulario] = useState(null) // null = fechado; { id?, ...campos }
  const [mensagem, setMensagem] = useState(null)

  const alterar = (campo) => (e) => setFormulario({ ...formulario, [campo]: e.target.value })

  async function salvar(evento) {
    evento.preventDefault()
    const dadosEnvio = { ...formulario, aulas: Number(formulario.aulas) }
    try {
      if (formulario.id) {
        await conteudos.editar(formulario.id, dadosEnvio)
        setMensagem({ tipo: 'sucesso', texto: 'Edições salvas com sucesso.' })
      } else {
        await conteudos.criar(dadosEnvio)
        setMensagem({ tipo: 'sucesso', texto: 'Conteúdo salvo com sucesso.' })
      }
      setFormulario(null)
      recarregar()
    } catch (e) {
      setMensagem({ tipo: 'erro', texto: e.message })
    }
  }

  async function alternar(c) {
    if (c.ativo && !window.confirm(`Desativar "${c.titulo}"? Ele some do catálogo e as atividades dele também são desativadas.`)) return
    try {
      await conteudos.alterarAtivo(c.id, !c.ativo)
      setMensagem({ tipo: 'sucesso', texto: c.ativo ? 'Conteúdo desativado com sucesso.' : 'Conteúdo reativado.' })
      recarregar()
    } catch (e) {
      setMensagem({ tipo: 'erro', texto: e.message })
    }
  }

  return (
    <>
      <div className="titulo-com-acao">
        <h1>Conteúdos</h1>
        <button type="button" className="botao" onClick={() => setFormulario({ ...VAZIO })}>Adicionar conteúdo</button>
      </div>
      <Aviso mensagem={mensagem} />

      {formulario && (
        <form className="formulario painel" onSubmit={salvar}>
          <h2>{formulario.id ? 'Editar conteúdo' : 'Novo conteúdo'}</h2>
          <label>Título<input value={formulario.titulo} onChange={alterar('titulo')} required maxLength={150} /></label>
          <label>Descrição<textarea rows={3} value={formulario.descricao ?? ''} onChange={alterar('descricao')} /></label>
          <div className="formulario-linha">
            <label>Categoria<input value={formulario.categoria} onChange={alterar('categoria')} required /></label>
            <label>
              Nível
              <select value={formulario.nivel} onChange={alterar('nivel')}>
                <option>Iniciante</option>
                <option>Intermediário</option>
                <option>Avançado</option>
              </select>
            </label>
            <label>Duração<input value={formulario.duracao} onChange={alterar('duracao')} placeholder="10h" /></label>
            <label>Aulas<input type="number" min="0" value={formulario.aulas} onChange={alterar('aulas')} /></label>
          </div>
          <div className="acoes">
            <button className="botao">Salvar</button>
            <button type="button" className="botao-texto" onClick={() => setFormulario(null)}>Cancelar</button>
          </div>
        </form>
      )}

      <Estado carregando={carregando} erro={erro} vazio={dados?.length === 0} mensagemVazio="Nenhum conteúdo cadastrado." />
      {dados?.length > 0 && (
        <div className="tabela-rolagem">
          <table>
            <thead>
              <tr>
                <th scope="col">Título</th>
                <th scope="col">Categoria</th>
                <th scope="col">Nível</th>
                <th scope="col">Aulas</th>
                <th scope="col">Situação</th>
                <th scope="col"><span className="visualmente-oculto">Ações</span></th>
              </tr>
            </thead>
            <tbody>
              {dados.map((c) => (
                <tr key={c.id} className={c.ativo ? '' : 'apagado'}>
                  <td>{c.titulo}</td>
                  <td>{c.categoria}</td>
                  <td>{c.nivel}</td>
                  <td>{c.aulas}</td>
                  <td>{c.situacao}</td>
                  <td className="celula-acoes">
                    <button type="button" className="botao-texto" onClick={() => setFormulario({ ...c })}>Editar</button>
                    <button type="button" className="botao-texto" onClick={() => alternar(c)}>{c.ativo ? 'Desativar' : 'Ativar'}</button>
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
