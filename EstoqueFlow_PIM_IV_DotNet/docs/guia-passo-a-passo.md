# Guia de execução — EstoqueFlow ERP

## Pré-requisitos
- Windows 10/11
- Visual Studio 2022 com workload **Desenvolvimento ASP.NET e Web**
- .NET 8 SDK

## Abrir
Abra `src/EstoqueFlow.Web/EstoqueFlow.Web.csproj` no Visual Studio.

## Restaurar e compilar
No terminal, entre na pasta do projeto:
```powershell
cd .\src\EstoqueFlow.Web
dotnet restore
dotnet build
```

## Atualizar banco existente
Como esta atualização adiciona pedidos de venda, crie e aplique uma nova migração:
```powershell
dotnet ef migrations add AddSalesModule
dotnet ef database update
```
Isso adiciona fornecedores, clientes e pedidos de compra sem excluir os dados anteriores.

Se ainda não existir migração inicial:
```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Se `dotnet ef` não for reconhecido:
```powershell
dotnet tool install --global dotnet-ef --version 8.0.11
```

## Executar
```powershell
dotnet run
```
Abra no navegador o endereço local informado no terminal. Para encerrar, pressione `Ctrl+C`.

## Fluxo de teste sugerido
1. Cadastre um produto com saldo inicial 0.
2. Cadastre um fornecedor.
3. Crie um pedido de compra para o produto.
4. Na listagem de pedidos, clique em **Receber**.
5. Confira que o saldo do produto aumentou e que uma entrada apareceu no histórico.
6. Cadastre um cliente.
7. Abra Relatórios e confira os indicadores e itens com estoque baixo.

## Módulos
- Painel
- Produtos
- Movimentações
- Fornecedores
- Clientes
- Pedidos de compra
- Pedidos de venda
- Relatórios

Ao confirmar uma venda, o sistema valida o saldo disponível e registra a baixa e a movimentação de saída em uma transação de banco de dados.

O recebimento de pedido altera o saldo do produto e registra uma movimentação de entrada em uma transação de banco de dados.
