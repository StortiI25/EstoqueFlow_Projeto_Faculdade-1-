# EstoqueFlow Mobile – melhoria de experiência Android

Esta atualização mantém a estrutura MAUI e a integração existente com a API do EstoqueFlow.

## Melhorias desta entrega
- Tradução dos campos técnicos mais comuns para português.
- Situação de pedidos e tipo de movimentação apresentados em português.
- Datas convertidas para `dd/MM/yyyy às HH:mm`.
- Preços e totais formatados como moeda brasileira (R$).
- Tratamento de nomes de campos de preço unitário e de entidades relacionadas.
- Mantidos login, dashboard, pesquisa, navegação, cadastro/edição/exclusão de produtos e registro de movimentações já existentes.

## Importante
Esta entrega é uma melhoria de apresentação e formatação dos dados. Ela não adiciona operações CRUD novas para clientes, fornecedores, compras, vendas, funcionários ou configurações. Esses módulos continuam dependendo das operações que já estiverem implementadas na API e na interface móvel.

## Compilação no Windows
Na pasta raiz da solução:

```powershell
dotnet build .\src\EstoqueFlow.Mobile\EstoqueFlow.Mobile.csproj -f net8.0-android -p:AndroidSdkDirectory="$env:LOCALAPPDATA\Android\Sdk"
```

Para instalar e iniciar no emulador conectado:

```powershell
dotnet build .\src\EstoqueFlow.Mobile\EstoqueFlow.Mobile.csproj -t:Run -f net8.0-android -p:AndroidSdkDirectory="$env:LOCALAPPDATA\Android\Sdk"
```

Não apague nem substitua `data/estoqueflow.db`.
