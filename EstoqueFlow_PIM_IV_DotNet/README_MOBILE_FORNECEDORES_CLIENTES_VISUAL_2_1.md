# EstoqueFlow Mobile – Fornecedores, Clientes e ícones 2.1

## O que mudou
- Cadastro de fornecedores e clientes pelo aplicativo.
- Edição de cadastros existentes.
- Exclusão com confirmação; a API impede remover registros vinculados a pedidos/vendas.
- Ação “Ver detalhes” ao tocar em um cadastro.
- Formulários solicitam nome, CPF/CNPJ, e-mail, telefone e endereço.
- Cards de listagem com ícones maiores (48x48) e tipografia maior.
- Atalhos do dashboard com alvos de toque maiores (62 px) e fonte ampliada.
- Navegação inferior com ícones ampliados.

## Como testar
1. Faça backup de `data/estoqueflow.db`.
2. Extraia o ZIP sobre uma pasta nova (não sobreponha o banco de dados de produção).
3. Abra `EstoqueFlow.sln` no Visual Studio.
4. Inicie o servidor Web/API e o emulador Android.
5. Compile e execute:

```powershell
dotnet build .\src\EstoqueFlow.Mobile\EstoqueFlow.Mobile.csproj -t:Run -f net8.0-android -p:AndroidSdkDirectory="$env:LOCALAPPDATA\Android\Sdk"
```

## Observação
A API já existente possui endpoints autenticados de CRUD para `suppliers` e `customers`; o Mobile agora utiliza esses endpoints. A validação deste pacote nesta sessão foi estrutural (XML e conteúdo do ZIP). A compilação Android deve ser confirmada no ambiente Visual Studio/Android SDK do usuário.
