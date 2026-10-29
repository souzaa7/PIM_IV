import { ActivityIndicator, View } from 'react-native'
import { StatusBar } from 'expo-status-bar'
import { NavigationContainer } from '@react-navigation/native'
import { createNativeStackNavigator } from '@react-navigation/native-stack'
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs'
import { AppProvider, useApp } from './src/contexto/AppContext'
import { cores } from './src/tema'

import LoginTela from './src/telas/LoginTela'
import CadastroTela from './src/telas/CadastroTela'
import InicioTela from './src/telas/InicioTela'
import CursosTela from './src/telas/CursosTela'
import AtividadesTela from './src/telas/AtividadesTela'
import ResponderTela from './src/telas/ResponderTela'
import NotasTela from './src/telas/NotasTela'
import PerfilTela from './src/telas/PerfilTela'

const Pilha = createNativeStackNavigator()
const Abas = createBottomTabNavigator()

/** Abas da parte de baixo da tela (depois do login). */
function AbasDoAluno() {
  return (
    <Abas.Navigator
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: cores.caneta,
        tabBarInactiveTintColor: cores.suave,
        tabBarLabelStyle: { fontSize: 13, fontWeight: '700' },
        tabBarIconStyle: { display: 'none' },
        tabBarActiveBackgroundColor: cores.marcaTexto,
        tabBarItemStyle: { justifyContent: 'center' },
      }}
    >
      <Abas.Screen name="Início" component={InicioTela} />
      <Abas.Screen name="Cursos" component={CursosTela} />
      <Abas.Screen name="Atividades" component={AtividadesTela} />
      <Abas.Screen name="Notas" component={NotasTela} />
      <Abas.Screen name="Perfil" component={PerfilTela} />
    </Abas.Navigator>
  )
}

/** Fluxo de navegação: sem login -> Login/Cadastro; com login -> abas do aluno. */
function Navegacao() {
  const { logado, iniciando } = useApp()

  if (iniciando) {
    return (
      <View style={{ flex: 1, justifyContent: 'center' }}>
        <ActivityIndicator size="large" color={cores.caneta} />
      </View>
    )
  }

  return (
    <Pilha.Navigator screenOptions={{ headerTintColor: cores.caneta }}>
      {logado ? (
        <>
          <Pilha.Screen name="Aluno" component={AbasDoAluno} options={{ title: 'Aprender Mais' }} />
          <Pilha.Screen name="Responder" component={ResponderTela} options={{ title: 'Responder atividade' }} />
        </>
      ) : (
        <>
          <Pilha.Screen name="Login" component={LoginTela} options={{ headerShown: false }} />
          <Pilha.Screen name="Cadastro" component={CadastroTela} options={{ title: 'Criar conta' }} />
        </>
      )}
    </Pilha.Navigator>
  )
}

export default function App() {
  return (
    <AppProvider>
      <NavigationContainer>
        <Navegacao />
        <StatusBar style="dark" />
      </NavigationContainer>
    </AppProvider>
  )
}
