import { Platform } from 'react-native'

// =====================================================================
// ENDEREÇO DA API
// - Navegador (npm run web) ou emulador iOS: http://localhost:5000
// - Emulador Android: http://10.0.2.2:5000 (é o "localhost" do computador)
// - Celular de verdade (Expo Go): use o IP do computador na rede Wi-Fi,
//   por exemplo http://192.168.0.10:5000 (veja com o comando ipconfig)
// =====================================================================
const IP_DO_COMPUTADOR = null // ex.: '192.168.0.10'

export const URL_API = IP_DO_COMPUTADOR
  ? `http://${IP_DO_COMPUTADOR}:5000`
  : Platform.OS === 'android'
    ? 'http://10.0.2.2:5000'
    : 'http://localhost:5000'
