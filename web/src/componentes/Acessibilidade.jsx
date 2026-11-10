import { useEffect, useState } from 'react'

// Preferências de acessibilidade: tamanho da fonte e alto contraste.
// Ficam salvas no navegador para a próxima visita.
const TAMANHOS = [100, 115, 130]
const CHAVE_FONTE = 'am.fonte'
const CHAVE_CONTRASTE = 'am.contraste'

function lerPreferencia(chave, padrao) {
  try {
    return JSON.parse(localStorage.getItem(chave)) ?? padrao
  } catch {
    return padrao
  }
}

// Navegação privada ou armazenamento bloqueado pode lançar erro; a preferência só não é salva.
function salvarPreferencia(chave, valor) {
  try {
    localStorage.setItem(chave, JSON.stringify(valor))
  } catch {
    // sem armazenamento disponível: segue sem persistir
  }
}

export default function Acessibilidade() {
  const [tamanho, setTamanho] = useState(() => lerPreferencia(CHAVE_FONTE, TAMANHOS[0]))
  const [contraste, setContraste] = useState(() => lerPreferencia(CHAVE_CONTRASTE, false))

  useEffect(() => {
    document.documentElement.style.fontSize = `${tamanho}%`
    salvarPreferencia(CHAVE_FONTE, tamanho)
  }, [tamanho])

  useEffect(() => {
    document.documentElement.dataset.contraste = contraste ? 'alto' : 'normal'
    salvarPreferencia(CHAVE_CONTRASTE, contraste)
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
