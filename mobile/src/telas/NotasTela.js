import { ScrollView, Text, View } from 'react-native'
import { useApp } from '../contexto/AppContext'
import StatusSincronizacao from '../componentes/StatusSincronizacao'
import { Linha, estilos } from '../componentes/Componentes'
import { cores } from '../tema'

/** UC05 - Acessar notas. */
export default function NotasTela() {
  const { pacote } = useApp()
  const notas = pacote?.notas ?? []
  const media = notas.length ? notas.reduce((s, n) => s + n.valor, 0) / notas.length : null

  return (
    <View style={estilos.tela}>
      <StatusSincronizacao />
      <ScrollView contentContainerStyle={estilos.conteudo}>
        <Text style={estilos.titulo}>Notas</Text>
        {media !== null && (
          <Text style={estilos.texto}>
            Média geral: <Text style={{ fontWeight: '700' }}>{media.toFixed(1).replace('.', ',')}</Text>
            {media >= 6 ? '. Você está apto para as provas.' : '. Revise os conteúdos com nota abaixo de 6.'}
          </Text>
        )}
        {notas.length === 0 && <Text style={estilos.vazio}>Nenhuma atividade corrigida ainda.</Text>}
        {notas.map((n, i) => (
          <Linha key={i}>
            <View style={{ flexDirection: 'row', justifyContent: 'space-between' }}>
              <View style={{ flex: 1, paddingRight: 12 }}>
                <Text style={estilos.subtitulo}>{n.atividade}</Text>
                <Text style={estilos.detalhe}>{n.conteudo}, corrigida em {n.dataAvaliacao}</Text>
              </View>
              <Text style={{ fontSize: 22, fontWeight: '700', color: n.valor >= 6 ? cores.verde : cores.vermelho }}>
                {String(n.valor).replace('.', ',')}
              </Text>
            </View>
            {n.feedback && <Text style={[estilos.texto, { color: cores.suave }]}>“{n.feedback}”</Text>}
          </Linha>
        ))}
      </ScrollView>
    </View>
  )
}
