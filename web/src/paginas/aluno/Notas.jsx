import { notas } from '../../servicos/api'
import { useCarregar } from '../../servicos/useCarregar'
import { Estado } from '../../componentes/Estado'

/** UC05 - Acessar notas (boletim). */
export default function Notas() {
  const { dados, erro, carregando } = useCarregar(notas.minhas)

  const media = dados?.length
    ? dados.reduce((soma, n) => soma + n.valor, 0) / dados.length
    : null

  return (
    <>
      <h1>Notas</h1>
      {media !== null && (
        <p>
          Sua média geral é <strong>{media.toLocaleString('pt-BR', { maximumFractionDigits: 1 })}</strong>.
          {media >= 6 ? ' Você está apto para as provas.' : ' Revise os conteúdos com nota abaixo de 6.'}
        </p>
      )}
      <Estado carregando={carregando} erro={erro} vazio={dados?.length === 0} mensagemVazio="Nenhuma atividade corrigida ainda." />

      {dados?.length > 0 && (
        <div className="tabela-rolagem">
          <table>
            <caption>Boletim</caption>
            <thead>
              <tr>
                <th scope="col">Curso</th>
                <th scope="col">Atividade</th>
                <th scope="col">Nota</th>
                <th scope="col">Comentário da correção</th>
                <th scope="col">Corrigida em</th>
              </tr>
            </thead>
            <tbody>
              {dados.map((n, i) => (
                <tr key={i}>
                  <td>{n.conteudo}</td>
                  <td>{n.atividade}</td>
                  <td>{n.valor.toLocaleString('pt-BR')} / {n.notaMaxima}</td>
                  <td>{n.feedback ?? '-'}</td>
                  <td>{n.dataAvaliacao}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  )
}
