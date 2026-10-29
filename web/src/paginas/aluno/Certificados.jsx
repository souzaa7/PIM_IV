import { certificados } from '../../servicos/api'
import { useCarregar } from '../../servicos/useCarregar'
import { Estado } from '../../componentes/Estado'

/** Certificados de conclusão do aluno. */
export default function Certificados() {
  const { dados, erro, carregando } = useCarregar(certificados.meus)

  return (
    <>
      <h1>Certificados</h1>
      <p>Você recebe o certificado quando todas as atividades de um curso são corrigidas.</p>
      <Estado carregando={carregando} erro={erro} vazio={dados?.length === 0} mensagemVazio="Conclua um curso para receber seu primeiro certificado." />

      <ul className="lista-certificados">
        {dados?.map((c) => (
          <li key={c.id} className="certificado">
            <p>Certificamos que</p>
            <p className="certificado-nome">{c.aluno}</p>
            <p>concluiu o curso <strong>{c.conteudo}</strong> em {c.dataEmissao}.</p>
            <p className="detalhes">Código de validação: {c.codigoValidacao}</p>
            <button type="button" className="botao-texto" onClick={() => window.print()}>Imprimir</button>
          </li>
        ))}
      </ul>
    </>
  )
}
