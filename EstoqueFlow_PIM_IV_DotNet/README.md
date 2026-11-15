# EstoqueFlow ERP — PIM IV

Projeto acadêmico de gestão de estoque com interface administrativa inspirada na organização de sistemas ERP. Não utiliza marca, telas proprietárias ou identidade visual oficial da TOTVS/Protheus.

## Tecnologias
- ASP.NET Core MVC / C# / .NET 8
- Entity Framework Core 8
- SQLite
- .NET MAUI (aplicativo móvel cliente da API; protótipo)

## Módulos Web disponíveis
1. Painel inicial com indicadores
2. Produtos: cadastro e edição
3. Estoque: entradas, saídas e histórico
4. Fornecedores: cadastro, consulta e edição
5. Clientes: cadastro, consulta e edição
6. Compras: pedido de compra e recebimento
7. Relatórios: estoque baixo, valor estimado do estoque e movimentações recentes
8. Vendas: pedidos por cliente e produto; confirmação da venda baixa o saldo e registra saída no histórico

Ao receber um pedido de compra pendente, o sistema aumenta o saldo do produto e cria automaticamente uma movimentação de entrada.

## Como executar no Windows / Visual Studio
1. Instale o .NET 8 SDK e o workload **Desenvolvimento ASP.NET e Web** do Visual Studio 2022.
2. Abra `src/EstoqueFlow.Web/EstoqueFlow.Web.csproj`.
3. No terminal do Visual Studio, entre na pasta `src/EstoqueFlow.Web`.
4. Execute `dotnet restore` e `dotnet build`.
5. Se você já aplicou `AddErpModules`, crie a migração desta atualização:
   `dotnet ef migrations add AddSalesModule`
6. Aplique as alterações ao SQLite:
   `dotnet ef database update`
7. Inicie:
   `dotnet run`
8. Abra a URL `http://localhost:...` ou `https://localhost:...` exibida no terminal.

Se o comando `dotnet ef` não existir:
`dotnet tool install --global dotnet-ef --version 8.0.11`

### Atenção ao banco de dados
A atualização de vendas adiciona a tabela `SalesOrders`. Não apague o arquivo SQLite existente. Aplique uma nova migração para preservar os produtos, movimentações, fornecedores, clientes e compras já cadastrados.

## Estrutura principal
- `src/EstoqueFlow.Web`: aplicação Web
- `src/EstoqueFlow.Mobile`: protótipo .NET MAUI
- `database`: scripts SQL de referência
- `docs`: relatório, diagramas e guias do PIM IV

## Limitações e próximos passos
- O protótipo não possui autenticação/autorização de usuários.
- A interface utiliza Bootstrap por CDN e precisa de internet para carregar esses recursos.
- O aplicativo MAUI é um cliente inicial e precisa ser validado em ambiente com workloads MAUI instalados.
- Antes de uso real, incluir controle de acesso, validações adicionais, testes automatizados, backup e tratamento de dados pessoais.
