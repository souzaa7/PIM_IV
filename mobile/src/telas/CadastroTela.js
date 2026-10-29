import { useState } from 'react'
import { ScrollView } from 'react-native'
import { useApp } from '../contexto/AppContext'
import { api } from '../servicos/api'
import { Aviso, Botao, Campo, estilos } from '../componentes/Componentes'

/** UC02 - Realizar cadastro. */
export default function CadastroTela() {
  const { entrar } = useApp()
  const [dados, setDados] = useState({ nome: '', cpf: '', email: '', senha: '' })
  const [erro, setErro] = useState('')
  const [carregando, setCarregando] = useState(false)

  const alterar = (campo) => (valor) => setDados({ ...dados, [campo]: valor })

  async function aoCadastrar() {
    setErro('')
    setCarregando(true)
    try {
      await api.cadastrar(dados)
      await entrar(dados.email, dados.senha)
    } catch (e) {
      setErro(e.message)
      setCarregando(false)
    }
  }

  return (
    <ScrollView style={estilos.tela} contentContainerStyle={estilos.conteudo} keyboardShouldPersistTaps="handled">
      <Campo rotulo="Nome completo" value={dados.nome} onChangeText={alterar('nome')} />
      <Campo rotulo="CPF" value={dados.cpf} onChangeText={alterar('cpf')} keyboardType="numeric" />
      <Campo rotulo="E-mail" value={dados.email} onChangeText={alterar('email')} autoCapitalize="none" keyboardType="email-address" />
      <Campo rotulo="Senha (mínimo 8 caracteres)" value={dados.senha} onChangeText={alterar('senha')} secureTextEntry />
      <Aviso texto={erro} />
      <Botao titulo="Criar conta" aoPressionar={aoCadastrar} carregando={carregando} />
    </ScrollView>
  )
}
