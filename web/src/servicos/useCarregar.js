import { useCallback, useEffect, useState } from 'react'

/**
 * Hook para carregar dados da API quando a tela abre.
 * Exemplo: const { dados, erro, carregando, recarregar } = useCarregar(notas.minhas)
 */
export function useCarregar(funcao) {
  const [dados, setDados] = useState(null)
  const [erro, setErro] = useState('')
  const [carregando, setCarregando] = useState(true)

  const recarregar = useCallback(async () => {
    setCarregando(true)
    setErro('')
    try {
      setDados(await funcao())
    } catch (e) {
      setErro(e.message)
    } finally {
      setCarregando(false)
    }
  }, [funcao])

  useEffect(() => {
    recarregar()
  }, [recarregar])

  return { dados, erro, carregando, recarregar }
}
