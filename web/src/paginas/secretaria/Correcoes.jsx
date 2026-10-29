import { useState } from 'react'
import { respostas } from '../../servicos/api'
import { useCarregar } from '../../servicos/useCarregar'
import { Aviso, Estado } from '../../componentes/Estado'

/** UC06 - Avaliar atividades: lista de respostas aguardando correção. */
export default function Correcoes() {
  const { dados, erro, carregando, recarregar } = useCarregar(respostas.pendentes)
  const [aberta, setAberta] = useState(null)
  const [valor, setValor] = useState('')
  const [feedback, setFeedback] = useState('')
  const [mensagem, setMensagem] = useState(null)

  function abrir(id) {
    setAberta(aberta === id ? null : id)
    setValor('')
    setFeedback('')
  }

  async function salvar(evento, resposta) {
    evento.preventDefault()
    try {
      const resultado = await respostas.avaliar(resposta.idResposta, Number(valor.replace(',', '.')), feedback)
      setMensagem({
        tipo: 'sucesso',
        texto: resultado.conteudoConcluido
          ? `Nota salva. ${resposta.aluno} concluiu ${resposta.conteudo} e recebeu o certificado ${resultado.codigoCertificado}.`
          : `Nota salva. Progresso de ${resposta.aluno} em ${resposta.conteudo}: ${resultado.progressoDoAluno}%.`,
      })
      setAberta(null)
      recarregar()
    } catch (e) {
      setMensagem({ tipo: 'erro', texto: e.message })
    }
  }

  return (
    <>
      <h1>Correções</h1>
      <p>Respostas enviadas pelos alunos, da mais antiga para a mais nova.</p>
      <Aviso mensagem={mensagem} />
      <Estado carregando={carregando} erro={erro} vazio={dados?.length === 0} mensagemVazio="Nenhuma resposta esperando correção." />

      <ul className="lista-atividades">
        {dados?.map((r) => (
          <li key={r.idResposta}>
            <div className="atividade-cabecalho">
              <div>
                <h2>{r.atividade}</h2>
                <p className="detalhes">{r.aluno}, {r.conteudo}, enviada em {r.dataEnvio}</p>
              </div>
              <button type="button" className="botao botao-secundario" onClick={() => abrir(r.idResposta)} aria-expanded={aberta === r.idResposta}>
                {aberta === r.idResposta ? 'Fechar' : 'Corrigir'}
              </button>
            </div>

            {aberta === r.idResposta && (
              <>
                <blockquote className="resposta-aluno">
                  {r.texto}
                  {r.arquivoUrl && <p><a href={r.arquivoUrl} target="_blank" rel="noreferrer">Abrir arquivo enviado</a></p>}
                </blockquote>
                <form className="formulario formulario-linha" onSubmit={(e) => salvar(e, r)}>
                  <label>
                    Nota (0 a {r.notaMaxima})
                    <input inputMode="decimal" value={valor} onChange={(e) => setValor(e.target.value)} required />
                  </label>
                  <label className="campo-largo">
                    Comentário para o aluno
                    <input value={feedback} onChange={(e) => setFeedback(e.target.value)} />
                  </label>
                  <button className="botao">Salvar nota</button>
                </form>
              </>
            )}
          </li>
        ))}
      </ul>
    </>
  )
}
