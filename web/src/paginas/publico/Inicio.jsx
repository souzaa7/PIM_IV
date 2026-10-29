import { Link } from 'react-router-dom'
import { conteudos } from '../../servicos/api'
import { useCarregar } from '../../servicos/useCarregar'
import { useAuth, rotaInicial } from '../../contexto/AuthContext'
import { Estado } from '../../componentes/Estado'

export default function Inicio() {
  const { usuario } = useAuth()
  const { dados, erro, carregando } = useCarregar(conteudos.listarAtivos)

  return (
    <>
      <section className="abertura">
        <h1>Aprenda tecnologia no seu ritmo, do primeiro clique ao certificado.</h1>
        <p>
          Cursos de Engenharia de Software, UX/UI Design e Microsoft 365 com atividades corrigidas
          por pessoas, acompanhamento do seu progresso e certificado que qualquer empresa pode validar.
        </p>
        <div className="acoes">
          {usuario ? (
            <Link className="botao" to={rotaInicial(usuario.perfil)}>Continuar estudando</Link>
          ) : (
            <>
              <Link className="botao" to="/cadastro">Criar minha conta</Link>
              <Link className="botao botao-secundario" to="/entrar">Já tenho conta</Link>
            </>
          )}
        </div>
      </section>

      <section aria-labelledby="titulo-cursos">
        <h2 id="titulo-cursos">Cursos disponíveis</h2>
        <Estado carregando={carregando} erro={erro} vazio={dados?.length === 0} mensagemVazio="Nenhum curso publicado ainda." />
        <ul className="lista-cursos">
          {dados?.map((curso) => (
            <li key={curso.id}>
              <h3>{curso.titulo}</h3>
              <p>{curso.descricao}</p>
              <p className="detalhes">
                {curso.categoria}, nível {curso.nivel.toLowerCase()}, {curso.aulas} aulas ({curso.duracao})
              </p>
            </li>
          ))}
        </ul>
      </section>
    </>
  )
}
