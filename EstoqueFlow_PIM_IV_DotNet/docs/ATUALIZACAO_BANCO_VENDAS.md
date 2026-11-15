# Atualização do banco — módulo de vendas

Esta versão adiciona a entidade `SalesOrder` e a tabela `SalesOrders`.

## Para atualizar a instalação que já está funcionando

1. Faça uma cópia de segurança do arquivo `estoqueflow.db`.
2. No projeto Web que já está funcionando, preserve a pasta `Migrations` criada pelas migrações anteriores.
3. Atualize os arquivos do projeto com esta versão, sem apagar a pasta `Migrations`.
4. No terminal, dentro de `src/EstoqueFlow.Web`, execute:
   ```powershell
   dotnet ef migrations add AddSalesModule
   dotnet ef database update
   ```
5. Execute `dotnet run`.

Não rode `migrations add InitialCreate` novamente em um banco que já possui as tabelas anteriores.

Se estiver usando uma cópia limpa do projeto sem a pasta `Migrations`, não aplique `InitialCreate` sobre o banco existente. Recupere primeiro a pasta de migrações da instalação anterior, ou faça uma cópia do banco e solicite orientação para fazer a adoção (baseline) do histórico de migrações.
