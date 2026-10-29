import { useState } from 'react'
import { certificados } from '../../servicos/api'
import { Aviso } from '../../componentes/Estado'

/** Página pública: qualquer pessoa confere se um certificado é verdadeiro. */
export default function ValidarCertificado() {
  const [codigo, setCodigo] = useState('')
  const [certificado, setCertificado] = useState(null)
  const [mensagem, setMensagem] = useState(null)

  async function aoEnviar(evento) {
    evento.preventDefault()
    setCertificado(null)
    setMensagem(null)
    try {
      setCertificado(await certificados.validar(codigo))
    } catch (e) {
      setMensagem({ tipo: 'erro', texto: e.message })
    }
  }

  return (
    <section className="formulario-central">
      <h1>Validar certificado</h1>
      <p>Digite o código impresso no certificado, por exemplo AM-2026-7F3A9C21.</p>
      <form onSubmit={aoEnviar} className="formulario">
        <label>
          Código do certificado
          <input value={codigo} onChange={(e) => setCodigo(e.target.value)} required />
        </label>
        <button className="botao">Validar</button>
      </form>
      <Aviso mensagem={mensagem} />
      {certificado && (
        <div className="certificado" role="status">
          <p>Certificado válido.</p>
          <p>
            <strong>{certificado.aluno}</strong> concluiu o curso <strong>{certificado.conteudo}</strong> em{' '}
            {certificado.dataEmissao}.
          </p>
        </div>
      )}
    </section>
  )
}
