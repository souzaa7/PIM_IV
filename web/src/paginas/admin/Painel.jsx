import { useCallback } from 'react'
import { relatorios } from '../../servicos/api'
import { useCarregar } from '../../servicos/useCarregar'
import { Estado } from '../../componentes/Estado'

/** Painel do administrador: indicadores gerais e desempenho por curso. */
export default function Painel() {
  const carregar = useCallback(async () => {
    const [resumo, cursos] = await Promise.all([relatorios.resumo(), relatorios.porConteudo()])
    return { resumo, cursos }
  }, [])
  const { dados, erro, carregando } = useCarregar(carregar)

  const indicadores = dados && [
    ['Alunos cadastrados', dados.resumo.totalAlunos],
    ['Secretarias', dados.resumo.totalSecretarias],
    ['Usuários desativados', dados.resumo.usuariosInativos],
    ['Cursos publicados', dados.resumo.conteudosAtivos],
    ['Matrículas em andamento', dados.resumo.matriculasEmAndamento],
    ['Cursos concluídos', dados.resumo.matriculasConcluidas],
    ['Respostas aguardando correção', dados.resumo.respostasPendentes],
    ['Certificados emitidos', dados.resumo.certificadosEmitidos],
    ['Média geral das notas', dados.resumo.mediaGeral.toLocaleString('pt-BR')],
  ]

  return (
    <>
      <h1>Painel da plataforma</h1>
      <Estado carregando={carregando} erro={erro} />
      {dados && (
        <>
          <dl className="indicadores">
            {indicadores.map(([rotulo, valor]) => (
              <div key={rotulo}>
                <dt>{rotulo}</dt>
                <dd>{valor}</dd>
              </div>
            ))}
          </dl>

          <h2>Desempenho por curso</h2>
          <div className="tabela-rolagem">
            <table>
              <thead>
                <tr>
                  <th scope="col">Curso</th>
                  <th scope="col">Situação</th>
                  <th scope="col">Matriculados</th>
                  <th scope="col">Concluíram</th>
                  <th scope="col">Média das notas</th>
                </tr>
              </thead>
              <tbody>
                {dados.cursos.map((c) => (
                  <tr key={c.idConteudo}>
                    <td>{c.titulo}</td>
                    <td>{c.situacao}</td>
                    <td>{c.matriculados}</td>
                    <td>{c.concluidos}</td>
                    <td>{c.mediaNotas.toLocaleString('pt-BR')}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}
    </>
  )
}
