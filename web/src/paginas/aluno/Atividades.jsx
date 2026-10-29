import { useState } from 'react'
import { atividades, respostas } from '../../servicos/api'
import { useCarregar } from '../../servicos/useCarregar'
import { Aviso, Estado } from '../../componentes/Estado'

/** UC04 - Realizar avaliações: o aluno vê as atividades e envia a resposta. */
export default function Atividades() {
  const { dados, erro, carregando, recarregar } = useCarregar(atividades.minhas)
  const [aberta, setAberta] = useState(null)   // id da atividade com o formulário aberto
  const [texto, setTexto] = useState('')
  const [link, setLink] = useState('')
  const [mensagem, setMensagem] = useState(null)

  function abrir(id) {
    setAberta(aberta === id ? null : id)
    setTexto('')
    setLink('')
  }

  async function enviar(evento, atividade) {
    evento.preventDefault()
    try {
      await respostas.enviar(atividade.idAtividade, texto, link || null)
      setMensagem({ tipo: 'sucesso', texto: `Resposta de "${atividade.titulo}" enviada. Agora é só aguardar a correção.` })
      setAberta(null)
      recarregar()
    } catch (e) {
      setMensagem({ tipo: 'erro', texto: e.message })
    }
  }

  return (
    <>
      <h1>Atividades</h1>
      <p>Atividades dos cursos em que você está matriculado.</p>
      <Aviso mensagem={mensagem} />
      <Estado
        carregando={carregando}
        erro={erro}
        vazio={dados?.length === 0}
        mensagemVazio="Nenhuma atividade por enquanto. Matricule-se em um curso para começar."
      />

      <ul className="lista-atividades">
        {dados?.map((a) => (
          <li key={a.idAtividade}>
            <div className="atividade-cabecalho">
              <div>
                <h2>{a.titulo}</h2>
                <p className="detalhes">{a.conteudo}, entrega até {a.dataEntrega}</p>
              </div>
              <span className={`situacao situacao-${a.situacao.split(' ')[0].toLowerCase()}`}>{a.situacao}</span>
            </div>
            {a.descricao && <p>{a.descricao}</p>}

            {a.nota !== null && (
              <p className="nota">
                Nota {a.nota.toLocaleString('pt-BR')} de {a.notaMaxima}
                {a.feedback && <span>: “{a.feedback}”</span>}
              </p>
            )}

            {a.podeEnviar && (
              <button type="button" className="botao botao-secundario" onClick={() => abrir(a.idAtividade)} aria-expanded={aberta === a.idAtividade}>
                {aberta === a.idAtividade ? 'Fechar' : 'Responder'}
              </button>
            )}

            {aberta === a.idAtividade && (
              <form className="formulario" onSubmit={(e) => enviar(e, a)}>
                <label>
                  Sua resposta
                  <textarea rows={5} value={texto} onChange={(e) => setTexto(e.target.value)} required />
                </label>
                <label>
                  Link de um arquivo (opcional)
                  <input type="url" value={link} onChange={(e) => setLink(e.target.value)} placeholder="https://" />
                </label>
                <button className="botao">Enviar resposta</button>
              </form>
            )}
          </li>
        ))}
      </ul>
    </>
  )
}
