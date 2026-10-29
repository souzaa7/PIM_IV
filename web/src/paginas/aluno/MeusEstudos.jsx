import { Link } from 'react-router-dom'
import { matriculas } from '../../servicos/api'
import { useCarregar } from '../../servicos/useCarregar'
import { useAuth } from '../../contexto/AuthContext'
import { useState } from 'react'
import { Aviso, Estado, Progresso } from '../../componentes/Estado'

/** Área do aluno: cursos em que está matriculado e o progresso de cada um. */
export default function MeusEstudos() {
  const { usuario } = useAuth()
  const { dados, erro, carregando, recarregar } = useCarregar(matriculas.minhas)
  const [mensagem, setMensagem] = useState(null)

  async function cancelar(matricula) {
    if (!window.confirm(`Cancelar a matrícula em "${matricula.conteudo}"?`)) return
    try {
      await matriculas.cancelar(matricula.idMatricula)
      setMensagem({ tipo: 'sucesso', texto: 'Matrícula cancelada.' })
      recarregar()
    } catch (e) {
      setMensagem({ tipo: 'erro', texto: e.message })
    }
  }

  return (
    <>
      <h1>Olá, {usuario.nome.split(' ')[0]}</h1>
      <p>Seus cursos e quanto falta para o certificado. O progresso sobe quando a secretaria corrige suas atividades.</p>

      <Aviso mensagem={mensagem} />
      <Estado
        carregando={carregando}
        erro={erro}
        vazio={dados?.length === 0}
        mensagemVazio="Você ainda não está matriculado em nenhum curso."
      />
      {dados?.length === 0 && <Link className="botao" to="/aluno/cursos">Escolher um curso</Link>}

      <ul className="lista-matriculas">
        {dados?.map((m) => (
          <li key={m.idMatricula} className={m.status === 'Cancelada' ? 'apagado' : ''}>
            <div>
              <h2>{m.conteudo}</h2>
              <p className="detalhes">{m.status}, desde {m.dataMatricula}</p>
            </div>
            <Progresso valor={m.progresso} rotulo={`Progresso em ${m.conteudo}`} />
            {m.podeCancelar && (
              <button type="button" className="botao-texto" onClick={() => cancelar(m)}>
                Cancelar matrícula
              </button>
            )}
          </li>
        ))}
      </ul>
    </>
  )
}
