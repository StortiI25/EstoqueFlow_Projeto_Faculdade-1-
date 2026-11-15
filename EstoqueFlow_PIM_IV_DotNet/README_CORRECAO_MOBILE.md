# Correção inicial do EstoqueFlow Mobile

Esta atualização corrige a comunicação inicial entre o app Android (emulador) e a API do EstoqueFlow:

- API/servidor configurado para `http://10.0.2.2:8080/` (endereço do computador hospedeiro visto pelo emulador Android; porta publicada pelo Docker Compose).
- Endpoint público `GET /api/mobile/health` para verificar conectividade.
- Endpoint `POST /api/mobile/login` que valida usuário ativo usando o mesmo banco e o mesmo PasswordHasher do Web, emitindo o cookie de autenticação já usado pelo sistema.
- O app mantém os cookies HTTP e solicita login antes de carregar produtos.
- Endpoint `POST /api/mobile/logout` para encerrar a sessão.
- Permissão INTERNET declarada no Manifest Android.

## Como aplicar e testar no Windows

1. Extraia o ZIP e abra a solução no Visual Studio.
2. Na pasta raiz do projeto, abra PowerShell e execute:

   ```powershell
   docker compose down
   docker compose up -d --build
   docker compose ps
   ```

3. No computador, abra `http://localhost:8080/api/mobile/health`. Deve aparecer um JSON com `status: ok`.
4. Inicie o emulador Android e execute o projeto `EstoqueFlow.Mobile` no Visual Studio.
5. Entre com o mesmo e-mail e senha cadastrados no EstoqueFlow Web.
6. Após o login, o app deve carregar a lista de produtos.

## Observações importantes

- `10.0.2.2` é específico do emulador Android. Em um celular físico, será necessário usar o IP local do computador na rede Wi-Fi.
- O Docker Compose monta `./data/estoqueflow.db` em `/app/estoqueflow.db`. Preserve esse arquivo: ele contém os usuários e dados atuais.
- Esta etapa entrega conexão, login e consulta de produtos. Ela **não** implementa ainda todos os módulos Web no Mobile (clientes, fornecedores, compras, vendas, relatórios, funcionários e configurações).
- O ambiente de edição desta atualização não possui o SDK .NET instalado; portanto, o código foi revisado estaticamente, mas a compilação Android e o teste integrado precisam ser executados no Visual Studio do Windows.
