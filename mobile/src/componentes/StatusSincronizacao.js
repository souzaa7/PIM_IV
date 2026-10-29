import { Pressable, Text, View } from 'react-native'
import { useApp } from '../contexto/AppContext'
import { cores } from '../tema'

/** Faixa que mostra se o app está online e se há respostas esperando envio. */
export default function StatusSincronizacao() {
  const { online, pendentes, sincronizando, atualizar, pacote } = useApp()

  const hora = pacote?.dataSincronizacao
    ? new Date(pacote.dataSincronizacao).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })
    : null

  let texto = sincronizando
    ? 'Sincronizando…'
    : online
      ? `Dados atualizados${hora ? ` às ${hora}` : ''}`
      : 'Sem conexão: mostrando os dados salvos no celular'
  if (pendentes > 0) texto += `. ${pendentes} resposta(s) aguardando envio`

  return (
    <View
      style={{
        backgroundColor: online ? cores.folha : cores.marcaTexto,
        borderBottomWidth: 1,
        borderBottomColor: cores.linha,
        paddingHorizontal: 16,
        paddingVertical: 8,
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-between',
      }}
    >
      <Text style={{ color: cores.tinta, flex: 1, fontSize: 14, marginRight: 12 }} accessibilityLiveRegion="polite">{texto}</Text>
      <Pressable onPress={atualizar} disabled={sincronizando} accessibilityRole="button" hitSlop={10}>
        <Text style={{ color: cores.caneta, fontWeight: '700', textDecorationLine: 'underline' }}>Sincronizar</Text>
      </Pressable>
    </View>
  )
}
