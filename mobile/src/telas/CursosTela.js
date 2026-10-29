import { useState } from 'react'
import { ScrollView, Text, View } from 'react-native'
import { useApp } from '../contexto/AppContext'
import StatusSincronizacao from '../componentes/StatusSincronizacao'
import { Aviso, Botao, Linha, estilos } from '../componentes/Componentes'
import { cores } from '../tema'

/** UC03 - Catálogo de cursos com matrícula. */
export default function CursosTela() {
  const { pacote, online, matricular } = useApp()
  const [mensagem, setMensagem] = useState(null)
  const [enviando, setEnviando] = useState(null)

  const matriculados = pacote?.matriculas.filter((m) => m.status !== 'Cancelada').map((m) => m.idConteudo) ?? []

  async function aoMatricular(curso) {
    setMensagem(null)
    setEnviando(curso.id)
    try {
      await matricular(curso.id)
      setMensagem({ tipo: 'sucesso', texto: `Matrícula feita em ${curso.titulo}.` })
    } catch (e) {
      setMensagem({ tipo: 'erro', texto: e.message })
    } finally {
      setEnviando(null)
    }
  }

  return (
    <View style={estilos.tela}>
      <StatusSincronizacao />
      <ScrollView contentContainerStyle={estilos.conteudo}>
        <Text style={estilos.titulo}>Cursos</Text>
        {mensagem && <Aviso tipo={mensagem.tipo} texto={mensagem.texto} />}
        {pacote?.conteudos.map((curso) => (
          <Linha key={curso.id}>
            <Text style={estilos.subtitulo}>{curso.titulo}</Text>
            <Text style={estilos.texto}>{curso.descricao}</Text>
            <Text style={estilos.detalhe}>{curso.categoria}, {curso.nivel.toLowerCase()}, {curso.aulas} aulas ({curso.duracao})</Text>
            {matriculados.includes(curso.id) ? (
              <Text style={{ color: cores.verde, fontWeight: '700', marginTop: 4 }}>Você já está neste curso</Text>
            ) : (
              <Botao
                titulo={online ? 'Matricular-me' : 'Matrícula precisa de internet'}
                desabilitado={!online}
                carregando={enviando === curso.id}
                aoPressionar={() => aoMatricular(curso)}
              />
            )}
          </Linha>
        ))}
      </ScrollView>
    </View>
  )
}
