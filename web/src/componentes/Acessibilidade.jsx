import { useEffect, useState } from 'react'

// Preferências de acessibilidade: tamanho da fonte e alto contraste.
// Ficam salvas no navegador para a próxima visita.
const TAMANHOS = [100, 115, 130]

function lerPreferencia(chave, padrao) {
  try {
    return JSON.parse(localStorage.getItem(chave)) ?? padrao
  } catch {
    return padrao
  }
}

export default function Acessibilidade() {
  const [tamanho, setTamanho] = useState(() => lerPreferencia('am.fonte', 100))
  const [contraste, setContraste] = useState(() => lerPreferencia('am.contraste', false))

  useEffect(() => {
    document.documentElement.style.fontSize = `${tamanho}%`
    localStorage.setItem('am.fonte', JSON.stringify(tamanho))
  }, [tamanho])

  useEffect(() => {
    document.documentElement.dataset.contraste = contraste ? 'alto' : 'normal'
    localStorage.setItem('am.contraste', JSON.stringify(contraste))
  }, [contraste])

  const indice = TAMANHOS.indexOf(tamanho)

  return (
    <div className="acessibilidade" role="group" aria-label="Opções de acessibilidade">
      <button
        type="button"
        onClick={() => setTamanho(TAMANHOS[Math.max(indice - 1, 0)])}
        disabled={indice <= 0}
        aria-label="Diminuir o texto"
      >
        A−
      </button>
      <button
        type="button"
        onClick={() => setTamanho(TAMANHOS[Math.min(indice + 1, TAMANHOS.length - 1)])}
        disabled={indice >= TAMANHOS.length - 1}
        aria-label="Aumentar o texto"
      >
        A+
      </button>
      <button type="button" aria-pressed={contraste} onClick={() => setContraste(!contraste)}>
        Alto contraste
      </button>
    </div>
  )
}
