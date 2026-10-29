import { useState } from 'react'
import { ScrollView, Text } from 'react-native'
import { useApp } from '../contexto/AppContext'
import { Aviso, Botao, Campo, estilos } from '../componentes/Componentes'

/** UC04 - Enviar a resposta. Sem internet, a resposta fica guardada e é enviada depois. */
export default function ResponderTela({ route, navigation }) {
  const { atividade } = route.params
  const { responder } = useApp()
  const [texto, setTexto] = useState('')
  const [mensagem, setMensagem] = useState(null)
  const [enviando, setEnviando] = useState(false)

  async function aoEnviar() {
    setEnviando(true)
    setMensagem(null)
    try {
      const situacao = await responder(atividade.idAtividade, texto)
      setMensagem({
        tipo: 'sucesso',
        texto: situacao === 'enviada'
          ? 'Resposta enviada! Agora é só aguardar a correção.'
          : 'Você está sem internet. A resposta foi guardada e será enviada na próxima sincronização.',
      })
      setTimeout(() => navigation.goBack(), 1800)
    } catch (e) {
      setMensagem({ tipo: 'erro', texto: e.message })
      setEnviando(false)
    }
  }

  return (
    <ScrollView style={estilos.tela} contentContainerStyle={estilos.conteudo} keyboardShouldPersistTaps="handled">
      <Text style={estilos.subtitulo}>{atividade.titulo}</Text>
      <Text style={estilos.detalhe}>{atividade.conteudo}, entrega até {atividade.dataEntrega}</Text>
      {atividade.descricao && <Text style={[estilos.texto, { marginTop: 8 }]}>{atividade.descricao}</Text>}

      <Campo
        rotulo="Sua resposta"
        value={texto}
        onChangeText={setTexto}
        multiline
        numberOfLines={8}
        style={[estilos.campo, { minHeight: 160, textAlignVertical: 'top' }]}
      />
      {mensagem && <Aviso tipo={mensagem.tipo} texto={mensagem.texto} />}
      <Botao titulo="Enviar resposta" aoPressionar={aoEnviar} carregando={enviando} desabilitado={!texto.trim()} />
    </ScrollView>
  )
}
