import { useCallback, useState } from 'react'
import { conteudos, matriculas } from '../../servicos/api'
import { useCarregar } from '../../servicos/useCarregar'
import { Aviso, Estado } from '../../componentes/Estado'

/** UC03 - Catálogo de cursos com botão de matrícula. */
export default function Cursos() {
  // Carrega o catálogo e as matrículas juntos para saber em quais cursos o aluno já está.
  const carregar = useCallback(async () => {
    const [catalogo, minhas] = await Promise.all([conteudos.listarAtivos(), matriculas.minhas()])
    const ativas = minhas.filter((m) => m.status !== 'Cancelada').map((m) => m.idConteudo)
    return catalogo.map((c) => ({ ...c, matriculado: ativas.includes(c.id) }))
  }, [])

  const { dados, erro, carregando, recarregar } = useCarregar(carregar)
  const [mensagem, setMensagem] = useState(null)

  async function matricular(curso) {
    try {
      await matriculas.matricular(curso.id)
      setMensagem({ tipo: 'sucesso', texto: `Matrícula feita em ${curso.titulo}. Bons estudos!` })
      recarregar()
    } catch (e) {
      setMensagem({ tipo: 'erro', texto: e.message })
    }
  }

  return (
    <>
      <h1>Cursos</h1>
      <Aviso mensagem={mensagem} />
      <Estado carregando={carregando} erro={erro} vazio={dados?.length === 0} mensagemVazio="Nenhum curso disponível." />

      <ul className="lista-cursos">
        {dados?.map((curso) => (
          <li key={curso.id}>
            <h2>{curso.titulo}</h2>
            <p>{curso.descricao}</p>
            <p className="detalhes">
              {curso.categoria}, nível {curso.nivel.toLowerCase()}, {curso.aulas} aulas ({curso.duracao})
            </p>
            {curso.matriculado ? (
              <p className="selo">Você já está neste curso</p>
            ) : (
              <button type="button" className="botao" onClick={() => matricular(curso)}>
                Matricular-me
              </button>
            )}
          </li>
        ))}
      </ul>
    </>
  )
}
