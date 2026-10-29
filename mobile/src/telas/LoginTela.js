import { useState } from 'react'
import { KeyboardAvoidingView, Platform, ScrollView, Text, View } from 'react-native'
import { useApp } from '../contexto/AppContext'
import { Aviso, Botao, Campo, estilos } from '../componentes/Componentes'
import { cores } from '../tema'

/** UC01 - Acessar sistema. */
export default function LoginTela({ navigation }) {
  const { entrar } = useApp()
  const [email, setEmail] = useState('')
  const [senha, setSenha] = useState('')
  const [erro, setErro] = useState('')
  const [carregando, setCarregando] = useState(false)

  async function aoEntrar() {
    setErro('')
    setCarregando(true)
    try {
      await entrar(email.trim(), senha)
    } catch (e) {
      setErro(e.message)
      setCarregando(false)
    }
  }

  return (
    <KeyboardAvoidingView style={estilos.tela} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
      <ScrollView contentContainerStyle={[estilos.conteudo, { paddingTop: 72 }]} keyboardShouldPersistTaps="handled">
        <View style={{ flexDirection: 'row', alignItems: 'center', marginBottom: 32 }}>
          <View style={{ width: 40, height: 40, borderRadius: 8, backgroundColor: cores.caneta, justifyContent: 'flex-end', padding: 6 }}>
            <View style={{ height: 8, backgroundColor: cores.marcaTexto, borderRadius: 2 }} />
          </View>
          <Text style={[estilos.titulo, { marginLeft: 12, marginBottom: 0 }]}>Aprender Mais</Text>
        </View>

        <Text style={estilos.texto}>Entre para ver seus cursos, enviar atividades e acompanhar suas notas, mesmo sem internet.</Text>

        <Campo rotulo="E-mail" value={email} onChangeText={setEmail} autoCapitalize="none" keyboardType="email-address" autoComplete="email" />
        <Campo rotulo="Senha" value={senha} onChangeText={setSenha} secureTextEntry autoComplete="password" />
        <Aviso texto={erro} />
        <Botao titulo="Entrar" aoPressionar={aoEntrar} carregando={carregando} desabilitado={!email || !senha} />
        <Botao titulo="Criar minha conta" secundario aoPressionar={() => navigation.navigate('Cadastro')} />

        <Text style={[estilos.detalhe, { marginTop: 24 }]}>Conta de demonstração: aluno@aprendermais.com, senha 123456</Text>
      </ScrollView>
    </KeyboardAvoidingView>
  )
}
