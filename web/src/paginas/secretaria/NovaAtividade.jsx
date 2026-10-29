import { useState } from 'react'
import { atividades, conteudos } from '../../servicos/api'
import { useCarregar } from '../../servicos/useCarregar'
import { Aviso, Estado } from '../../componentes/Estado'

/** Secretaria cria uma atividade (avaliação) para um conteúdo. */
export default function NovaAtividade() {
  const { dados: lista, erro, carregando } = useCarregar(conteudos.listarAtivos)
  const [dados, setDados] = useState({ idConteudo: '', titulo: '', descricao: '', dataEntrega: '', notaMaxima: 10 })
  const [mensagem, setMensagem] = useState(null)

  const alterar = (campo) => (e) => setDados({ ...dados, [campo]: e.target.value })

  async function salvar(evento) {
    evento.preventDefault()
    try {
      const criada = await atividades.criar({
        ...dados,
        idConteudo: Number(dados.idConteudo),
        notaMaxima: Number(dados.notaMaxima),
      })
      setMensagem({ tipo: 'sucesso', texto: `Atividade "${criada.titulo}" publicada com entrega até ${criada.dataEntrega}.` })
      setDados({ ...dados, titulo: '', descricao: '', dataEntrega: '' })
    } catch (e) {
      setMensagem({ tipo: 'erro', texto: e.message })
    }
  }

  return (
    <section className="formulario-central formulario-largo">
      <h1>Nova atividade</h1>
      <Estado carregando={carregando} erro={erro} />
      {lista && (
        <form className="formulario" onSubmit={salvar}>
          <label>
            Curso
            <select value={dados.idConteudo} onChange={alterar('idConteudo')} required>
              <option value="">Escolha o curso</option>
              {lista.map((c) => <option key={c.id} value={c.id}>{c.titulo}</option>)}
            </select>
          </label>
          <label>Título<input value={dados.titulo} onChange={alterar('titulo')} required maxLength={150} /></label>
          <label>Enunciado<textarea rows={4} value={dados.descricao} onChange={alterar('descricao')} /></label>
          <div className="formulario-linha">
            <label>Entregar até<input type="datetime-local" value={dados.dataEntrega} onChange={alterar('dataEntrega')} required /></label>
            <label>Nota máxima<input type="number" min="1" max="100" value={dados.notaMaxima} onChange={alterar('notaMaxima')} required /></label>
          </div>
          <Aviso mensagem={mensagem} />
          <button className="botao">Publicar atividade</button>
        </form>
      )}
    </section>
  )
}
