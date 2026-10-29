import { ActivityIndicator, Pressable, StyleSheet, Text, TextInput, View } from 'react-native'
import { cores, espaco } from '../tema'

/** Botão principal (preenchido) ou secundário (contorno). */
export function Botao({ titulo, aoPressionar, secundario, desabilitado, carregando }) {
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityState={{ disabled: desabilitado || carregando }}
      onPress={aoPressionar}
      disabled={desabilitado || carregando}
      style={({ pressed }) => [
        estilos.botao,
        secundario && estilos.botaoSecundario,
        (pressed || desabilitado) && { opacity: 0.7 },
      ]}
    >
      {carregando ? (
        <ActivityIndicator color={secundario ? cores.caneta : cores.folha} />
      ) : (
        <Text style={[estilos.botaoTexto, secundario && { color: cores.caneta }]}>{titulo}</Text>
      )}
    </Pressable>
  )
}

/** Campo de texto com rótulo visível (melhor para acessibilidade que só placeholder). */
export function Campo({ rotulo, ...props }) {
  return (
    <View style={{ marginBottom: espaco.m }}>
      <Text style={estilos.rotulo}>{rotulo}</Text>
      <TextInput accessibilityLabel={rotulo} style={estilos.campo} placeholderTextColor={cores.suave} {...props} />
    </View>
  )
}

/** Mensagem de erro ou sucesso. */
export function Aviso({ tipo = 'erro', texto }) {
  if (!texto) return null
  const erro = tipo === 'erro'
  return (
    <Text
      accessibilityLiveRegion="polite"
      style={[estilos.aviso, {
        borderColor: erro ? cores.vermelho : cores.verde,
        backgroundColor: erro ? cores.vermelhoFundo : cores.verdeFundo,
        color: erro ? cores.vermelho : cores.verde,
      }]}
    >
      {texto}
    </Text>
  )
}

/** Barra de progresso amarela (marca-texto), igual à do site. */
export function Progresso({ valor }) {
  return (
    <View style={estilos.progressoLinha} accessibilityRole="progressbar" accessibilityValue={{ min: 0, max: 100, now: valor }}>
      <View style={estilos.progressoTrilho}>
        <View style={[estilos.progressoPreenchido, { width: `${valor}%` }]} />
      </View>
      <Text style={estilos.progressoTexto}>{valor}%</Text>
    </View>
  )
}

/** Linha de lista separada por uma "pauta" de caderno. */
export function Linha({ children }) {
  return <View style={estilos.linha}>{children}</View>
}

export const estilos = StyleSheet.create({
  tela: { flex: 1, backgroundColor: cores.papel },
  conteudo: { padding: espaco.m, paddingBottom: espaco.g * 2 },
  titulo: { fontSize: 26, fontWeight: '700', color: cores.tinta, marginBottom: espaco.p },
  subtitulo: { fontSize: 18, fontWeight: '700', color: cores.tinta, marginBottom: 4 },
  texto: { fontSize: 16, color: cores.tinta, lineHeight: 23, marginBottom: espaco.p },
  detalhe: { fontSize: 14, color: cores.suave, marginBottom: 4 },
  vazio: { fontSize: 16, color: cores.suave, marginVertical: espaco.m },
  botao: {
    backgroundColor: cores.caneta,
    borderWidth: 2,
    borderColor: cores.caneta,
    borderRadius: 6,
    paddingVertical: 12,
    paddingHorizontal: 18,
    alignItems: 'center',
    marginTop: espaco.p,
  },
  botaoSecundario: { backgroundColor: 'transparent' },
  botaoTexto: { color: cores.folha, fontWeight: '700', fontSize: 16 },
  rotulo: { fontSize: 15, fontWeight: '700', color: cores.tinta, marginBottom: 4 },
  campo: {
    backgroundColor: cores.folha,
    borderWidth: 1,
    borderColor: '#8A97AB',
    borderRadius: 6,
    paddingHorizontal: 12,
    paddingVertical: 10,
    fontSize: 16,
    color: cores.tinta,
  },
  aviso: { borderLeftWidth: 4, padding: 10, marginVertical: espaco.p, fontSize: 15 },
  linha: { borderBottomWidth: 1, borderBottomColor: cores.linha, paddingVertical: espaco.m },
  progressoLinha: { flexDirection: 'row', alignItems: 'center', marginTop: 6 },
  progressoTrilho: {
    flex: 1,
    height: 14,
    backgroundColor: cores.folha,
    borderWidth: 1,
    borderColor: cores.linha,
    borderRadius: 3,
    overflow: 'hidden',
  },
  progressoPreenchido: { height: '100%', backgroundColor: cores.marcaTexto },
  progressoTexto: { marginLeft: 8, fontWeight: '700', color: cores.tinta, minWidth: 44 },
})
