import { RefreshControl, ScrollView, Text, View } from 'react-native'
import { useApp } from '../contexto/AppContext'
import StatusSincronizacao from '../componentes/StatusSincronizacao'
import { Aviso, Linha, Progresso, estilos } from '../componentes/Componentes'

/** Tela inicial: matrículas e progresso de cada curso. */
export default function InicioTela() {
  const { pacote, fila, sincronizando, atualizar, problemas } = useApp()
  const matriculas = pacote?.matriculas.filter((m) => m.status !== 'Cancelada') ?? []
  const naFila = fila.map((item) => item.idAtividade)
  const pendentes = pacote?.atividades.filter((a) => a.podeEnviar && !naFila.includes(a.idAtividade)).length ?? 0

  return (
    <View style={estilos.tela}>
      <StatusSincronizacao />
      <ScrollView
        contentContainerStyle={estilos.conteudo}
        refreshControl={<RefreshControl refreshing={sincronizando} onRefresh={atualizar} />}
      >
        <Text style={estilos.titulo}>Olá, {pacote?.usuario.nome.split(' ')[0] ?? 'aluno'}</Text>
        <Text style={estilos.texto}>
          {pendentes > 0
            ? `Você tem ${pendentes} atividade(s) para responder.`
            : 'Nenhuma atividade pendente. Bom trabalho!'}
        </Text>

        {problemas.map((p, i) => <Aviso key={i} texto={`Uma resposta guardada não foi aceita: ${p}`} />)}

        <Text style={[estilos.subtitulo, { marginTop: 8 }]}>Meus cursos</Text>
        {matriculas.length === 0 && <Text style={estilos.vazio}>Você ainda não tem cursos. Veja a aba Cursos.</Text>}
        {matriculas.map((m) => (
          <Linha key={m.idMatricula}>
            <Text style={estilos.subtitulo}>{m.conteudo}</Text>
            <Text style={estilos.detalhe}>{m.status}, desde {m.dataMatricula}</Text>
            <Progresso valor={m.progresso} />
          </Linha>
        ))}
      </ScrollView>
    </View>
  )
}
