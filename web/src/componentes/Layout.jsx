import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../contexto/AuthContext'
import Acessibilidade from './Acessibilidade'

// Menu de cada perfil.
const MENUS = {
  Aluno: [
    { para: '/aluno', texto: 'Meus estudos', fim: true },
    { para: '/aluno/cursos', texto: 'Cursos' },
    { para: '/aluno/atividades', texto: 'Atividades' },
    { para: '/aluno/notas', texto: 'Notas' },
    { para: '/aluno/certificados', texto: 'Certificados' },
  ],
  Secretaria: [
    { para: '/secretaria', texto: 'Correções', fim: true },
    { para: '/secretaria/conteudos', texto: 'Conteúdos' },
    { para: '/secretaria/atividades', texto: 'Nova atividade' },
    { para: '/secretaria/alunos', texto: 'Cadastrar aluno' },
  ],
  Administrador: [
    { para: '/admin', texto: 'Painel', fim: true },
    { para: '/admin/usuarios', texto: 'Usuários' },
  ],
  Visitante: [
    { para: '/', texto: 'Início', fim: true },
    { para: '/validar-certificado', texto: 'Validar certificado' },
  ],
}

export default function Layout() {
  const { usuario, sair } = useAuth()
  const navegar = useNavigate()
  const menu = MENUS[usuario?.perfil ?? 'Visitante']

  function aoSair() {
    sair()
    navegar('/')
  }

  return (
    <>
      <a className="pular" href="#conteudo">Pular para o conteúdo</a>

      <header className="topo">
        <div className="topo-linha">
          <NavLink to="/" className="marca" aria-label="Aprender Mais - início">
            <img src="/icone.svg" alt="" width="32" height="32" />
            <span>Aprender Mais</span>
          </NavLink>
          <Acessibilidade />
          {usuario ? (
            <div className="sessao">
              <span>
                {usuario.nome} <small>({usuario.perfil})</small>
              </span>
              <button type="button" className="botao-texto" onClick={aoSair}>Sair</button>
            </div>
          ) : (
            <div className="sessao">
              <NavLink to="/entrar" className="botao botao-pequeno">Entrar</NavLink>
            </div>
          )}
        </div>

        <nav aria-label="Menu principal" className="menu">
          {menu.map((item) => (
            <NavLink key={item.para} to={item.para} end={item.fim}>
              {item.texto}
            </NavLink>
          ))}
        </nav>
      </header>

      <main id="conteudo" className="pagina">
        <Outlet />
      </main>

      <footer className="rodape">
        Aprender Mais é o projeto do PIM IV de Análise e Desenvolvimento de Sistemas, UNIP São José dos Campos.
      </footer>
    </>
  )
}
