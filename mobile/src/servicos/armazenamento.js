import AsyncStorage from '@react-native-async-storage/async-storage'

// AsyncStorage é a "memória" do celular: os dados continuam lá
// mesmo se o app for fechado ou se a internet cair.
const CHAVES = {
  token: 'am.token',
  pacote: 'am.pacote',         // última cópia dos dados vinda da API
  fila: 'am.filaRespostas',    // respostas feitas sem internet, esperando envio
}

async function ler(chave, padrao) {
  const texto = await AsyncStorage.getItem(chave)
  return texto ? JSON.parse(texto) : padrao
}

const gravar = (chave, valor) => AsyncStorage.setItem(chave, JSON.stringify(valor))

export const armazenamento = {
  lerToken: () => AsyncStorage.getItem(CHAVES.token),
  salvarToken: (token) => AsyncStorage.setItem(CHAVES.token, token),

  lerPacote: () => ler(CHAVES.pacote, null),
  salvarPacote: (pacote) => gravar(CHAVES.pacote, pacote),

  lerFila: () => ler(CHAVES.fila, []),
  salvarFila: (fila) => gravar(CHAVES.fila, fila),

  /** Ao sair da conta, apaga tudo do aluno que estava no celular. */
  limparTudo: () => AsyncStorage.multiRemove(Object.values(CHAVES)),
}
