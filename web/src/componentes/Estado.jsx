// Componentes pequenos reaproveitados em várias telas.

/** Mostra "carregando", erro ou lista vazia. Retorna null quando há dados para exibir. */
export function Estado({ carregando, erro, vazio, mensagemVazio }) {
  if (carregando) return <p className="estado" role="status">Carregando…</p>
  if (erro) return <p className="aviso aviso-erro" role="alert">{erro}</p>
  if (vazio) return <p className="estado">{mensagemVazio}</p>
  return null
}

/** Mensagem de sucesso ou erro depois de uma ação. */
export function Aviso({ mensagem }) {
  if (!mensagem?.texto) return null
  return (
    <p className={`aviso aviso-${mensagem.tipo}`} role={mensagem.tipo === 'erro' ? 'alert' : 'status'}>
      {mensagem.texto}
    </p>
  )
}

/** Barra de progresso no estilo marca-texto. */
export function Progresso({ valor, rotulo }) {
  return (
    <div className="progresso">
      <div
        className="progresso-trilho"
        role="progressbar"
        aria-valuenow={valor}
        aria-valuemin="0"
        aria-valuemax="100"
        aria-label={rotulo}
      >
        <div className="progresso-preenchido" style={{ width: `${valor}%` }} />
      </div>
      <span>{Number(valor).toLocaleString('pt-BR')}%</span>
    </div>
  )
}
