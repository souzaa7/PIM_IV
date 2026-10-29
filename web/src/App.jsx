import { Route, Routes } from 'react-router-dom'
import Layout from './componentes/Layout'
import RotaProtegida from './componentes/RotaProtegida'

import Inicio from './paginas/publico/Inicio'
import Entrar from './paginas/publico/Entrar'
import Cadastro from './paginas/publico/Cadastro'
import ValidarCertificado from './paginas/publico/ValidarCertificado'

import MeusEstudos from './paginas/aluno/MeusEstudos'
import Cursos from './paginas/aluno/Cursos'
import Atividades from './paginas/aluno/Atividades'
import Notas from './paginas/aluno/Notas'
import Certificados from './paginas/aluno/Certificados'

import Correcoes from './paginas/secretaria/Correcoes'
import Conteudos from './paginas/secretaria/Conteudos'
import NovaAtividade from './paginas/secretaria/NovaAtividade'
import CadastrarAluno from './paginas/secretaria/CadastrarAluno'

import Painel from './paginas/admin/Painel'
import Usuarios from './paginas/admin/Usuarios'

/** Mapa de rotas (endereços) do site. */
export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        {/* Páginas públicas */}
        <Route path="/" element={<Inicio />} />
        <Route path="/entrar" element={<Entrar />} />
        <Route path="/cadastro" element={<Cadastro />} />
        <Route path="/validar-certificado" element={<ValidarCertificado />} />

        {/* Área do aluno */}
        <Route element={<RotaProtegida perfil="Aluno" />}>
          <Route path="/aluno" element={<MeusEstudos />} />
          <Route path="/aluno/cursos" element={<Cursos />} />
          <Route path="/aluno/atividades" element={<Atividades />} />
          <Route path="/aluno/notas" element={<Notas />} />
          <Route path="/aluno/certificados" element={<Certificados />} />
        </Route>

        {/* Área da secretaria */}
        <Route element={<RotaProtegida perfil="Secretaria" />}>
          <Route path="/secretaria" element={<Correcoes />} />
          <Route path="/secretaria/conteudos" element={<Conteudos />} />
          <Route path="/secretaria/atividades" element={<NovaAtividade />} />
          <Route path="/secretaria/alunos" element={<CadastrarAluno />} />
        </Route>

        {/* Área do administrador */}
        <Route element={<RotaProtegida perfil="Administrador" />}>
          <Route path="/admin" element={<Painel />} />
          <Route path="/admin/usuarios" element={<Usuarios />} />
        </Route>

        <Route path="*" element={<p className="estado">Página não encontrada.</p>} />
      </Route>
    </Routes>
  )
}
