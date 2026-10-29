# Mobile – React Native + Expo (JavaScript)

App do aluno, com **sincronização offline**. Explicação completa em [`docs/05-solucao-mobile.md`](../docs/05-solucao-mobile.md).

## Como rodar

Pré-requisitos: [Node.js 22](https://nodejs.org/), a **API rodando** e um destes:

- o app **Expo Go** no celular (Android ou iOS), na mesma rede Wi-Fi do computador;
- um emulador Android (Android Studio);
- ou só o navegador.

```bash
cd mobile
npm install
npx expo start
```

| Onde | Como abrir | Ajuste em `src/config.js` |
|---|---|---|
| Navegador | tecla `w` no terminal | nenhum (`localhost`) |
| Emulador Android | tecla `a` | nenhum (`10.0.2.2` automático) |
| Celular com Expo Go | ler o QR Code | coloque o IP do computador em `IP_DO_COMPUTADOR` (veja com `ipconfig`) |

> No celular, a API precisa aceitar conexões da rede: rode-a com `dotnet run --project AprenderMais.Api --urls http://0.0.0.0:5000` e libere a porta 5000 no firewall do Windows.

Login de teste: `aluno@aprendermais.com` / `123456`.

## Testando o modo offline

1. Entre no app e abra a aba **Atividades**.
2. Desligue a API (ou o Wi-Fi do celular).
3. Responda uma atividade: aparece "Guardada no celular" e a faixa fica amarela.
4. Ligue a API de novo e toque em **Sincronizar**: a resposta é enviada.

## Estrutura

```text
App.js                       Navegação (pilha + abas)
src/
├── config.js                Endereço da API
├── tema.js                  Cores (as mesmas do site)
├── servicos/
│   ├── api.js               Chamadas à API
│   ├── armazenamento.js     AsyncStorage (token, dados, fila)
│   └── sincronizacao.js     Fila offline e download do pacote
├── contexto/AppContext.js   Estado global (login, dados, online/offline)
├── componentes/             Botão, campo, aviso, progresso, faixa de sincronização
└── telas/                   Login, Cadastro, Início, Cursos, Atividades, Responder, Notas, Perfil
```
