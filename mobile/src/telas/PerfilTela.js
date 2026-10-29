import { ScrollView, Text, View } from 'react-native'
import { useApp } from '../contexto/AppContext'
import StatusSincronizacao from '../componentes/StatusSincronizacao'
import { Botao, Linha, estilos } from '../componentes/Componentes'
import { cores } from '../tema'

/** Perfil do aluno: dados, certificados e sair. */
export default function PerfilTela() {
  const { pacote, sair } = useApp()
  const certificados = pacote?.certificados ?? []

  return (
    <View style={estilos.tela}>
      <StatusSincronizacao />
      <ScrollView contentContainerStyle={estilos.conteudo}>
        <Text style={estilos.titulo}>{pacote?.usuario.nome}</Text>
        <Text style={estilos.detalhe}>{pacote?.usuario.email}</Text>

        <Text style={[estilos.subtitulo, { marginTop: 24 }]}>Certificados</Text>
        {certificados.length === 0 && <Text style={estilos.vazio}>Conclua um curso para receber o certificado.</Text>}
        {certificados.map((c) => (
          <Linha key={c.id}>
            <Text style={estilos.subtitulo}>{c.conteudo}</Text>
            <Text style={estilos.detalhe}>Emitido em {c.dataEmissao}</Text>
            <Text style={{ color: cores.caneta, fontWeight: '700' }}>Código: {c.codigoValidacao}</Text>
          </Linha>
        ))}

        <Botao titulo="Sair da conta" secundario aoPressionar={sair} />
      </ScrollView>
    </View>
  )
}
