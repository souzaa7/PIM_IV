import { ScrollView, Text, View } from 'react-native'
import { useApp } from '../contexto/AppContext'
import StatusSincronizacao from '../componentes/StatusSincronizacao'
import { Botao, Linha, estilos } from '../componentes/Componentes'
import { cores } from '../tema'

const COR_SITUACAO = {
  Pendente: cores.caneta,
  'Aguardando correção': cores.suave,
  Avaliada: cores.verde,
  'Prazo encerrado': cores.vermelho,
  'Guardada no celular': '#7A5B00',
}

/** UC04 - Lista de atividades com a situação de cada uma. */
export default function AtividadesTela({ navigation }) {
  const { pacote, fila } = useApp()

  // Respostas feitas sem internet ainda não chegaram na API:
  // marcamos essas atividades como "Guardada no celular".
  const naFila = fila.map((item) => item.idAtividade)
  const atividades = (pacote?.atividades ?? []).map((a) =>
    naFila.includes(a.idAtividade) ? { ...a, situacao: 'Guardada no celular', podeEnviar: false } : a)

  return (
    <View style={estilos.tela}>
      <StatusSincronizacao />
      <ScrollView contentContainerStyle={estilos.conteudo}>
        <Text style={estilos.titulo}>Atividades</Text>
        {atividades.length === 0 && <Text style={estilos.vazio}>Matricule-se em um curso para receber atividades.</Text>}
        {atividades.map((a) => (
          <Linha key={a.idAtividade}>
            <Text style={{ color: COR_SITUACAO[a.situacao] ?? cores.suave, fontWeight: '700', marginBottom: 4 }}>{a.situacao}</Text>
            <Text style={estilos.subtitulo}>{a.titulo}</Text>
            <Text style={estilos.detalhe}>{a.conteudo}, entrega até {a.dataEntrega}</Text>
            {a.nota !== null && (
              <Text style={[estilos.texto, { fontWeight: '700' }]}>
                Nota {String(a.nota).replace('.', ',')} de {a.notaMaxima}{a.feedback ? `: “${a.feedback}”` : ''}
              </Text>
            )}
            {a.podeEnviar && (
              <Botao titulo="Responder" secundario aoPressionar={() => navigation.navigate('Responder', { atividade: a })} />
            )}
          </Linha>
        ))}
      </ScrollView>
    </View>
  )
}
