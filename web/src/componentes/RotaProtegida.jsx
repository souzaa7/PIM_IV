import { Navigate, Outlet } from 'react-router-dom'
import { rotaInicial, useAuth } from '../contexto/AuthContext'

/** Só deixa entrar quem está logado e tem o perfil certo. */
export default function RotaProtegida({ perfil }) {
  const { usuario, verificando } = useAuth()

  if (verificando) return <p className="estado">Carregando…</p>
  if (!usuario) return <Navigate to="/entrar" replace />
  if (usuario.perfil !== perfil) return <Navigate to={rotaInicial(usuario.perfil)} replace />

  return <Outlet />
}
