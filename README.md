# EstoqueFlow ERP — PIM IV

Sistema acadêmico de gestão de estoque desenvolvido como parte do Projeto Integrado Multidisciplinar (PIM IV), do curso de Análise e Desenvolvimento de Sistemas.

O EstoqueFlow tem como objetivo centralizar operações de controle de estoque e gestão comercial em uma aplicação Web, com um protótipo de aplicativo móvel desenvolvido em .NET MAUI.

O projeto utiliza uma interface administrativa inspirada na organização de sistemas ERP. Não utiliza marca, telas proprietárias ou identidade visual oficial da TOTVS ou do Protheus.

---

## 1. Tecnologias utilizadas

| Tecnologia | Aplicação |
|---|---|
| C# | Linguagem principal |
| ASP.NET Core MVC | Desenvolvimento da aplicação Web |
| .NET 8 | Plataforma de desenvolvimento |
| Entity Framework Core 8 | Mapeamento e persistência de dados |
| SQLite | Banco de dados relacional |
| .NET MAUI | Protótipo de aplicativo móvel cliente da API |
| Bootstrap | Estilização da interface Web |

## 2. Funcionalidades da aplicação Web

A aplicação Web possui os seguintes módulos:

### 2.1 Painel inicial
- Visualização de indicadores gerais.
- Resumo da quantidade de produtos.
- Identificação de itens com estoque baixo.
- Acompanhamento de informações da operação.

### 2.2 Produtos
- Cadastro de produtos.
- Consulta dos produtos cadastrados.
- Edição das informações.
- Controle de quantidade e estoque mínimo.

### 2.3 Estoque
- Registro de entradas.
- Registro de saídas.
- Consulta do histórico de movimentações.
- Atualização dos saldos dos produtos.

### 2.4 Fornecedores
- Cadastro de fornecedores.
- Consulta dos registros.
- Edição das informações cadastradas.

### 2.5 Clientes
- Cadastro de clientes.
- Consulta dos registros.
- Edição das informações cadastradas.

### 2.6 Compras
- Criação de pedidos de compra.
- Consulta dos pedidos.
- Recebimento de pedidos pendentes.
- Atualização automática do saldo do produto após o recebimento.
- Registro automático de uma movimentação de entrada no estoque.

### 2.7 Vendas
- Criação de pedidos de venda.
- Associação de clientes e produtos.
- Definição das quantidades e valores.
- Consulta dos pedidos registrados.
- Confirmação de vendas.
- Baixa automática do estoque após a confirmação.
- Registro da saída no histórico de movimentações.

### 2.8 Relatórios
- Consulta de produtos com estoque baixo.
- Visualização do valor estimado do estoque.
- Consulta de movimentações recentes.
- Indicadores consolidados da operação.

---

## 3. Aplicativo móvel

O projeto também possui um protótipo de aplicativo Android desenvolvido com .NET MAUI.

O aplicativo foi planejado como cliente da API da aplicação Web, permitindo evoluir o acesso às informações do EstoqueFlow por dispositivos móveis.

**Tecnologias:**
- .NET MAUI.
- C#.
- API REST.

**Estado atual:** protótipo em desenvolvimento e validação. A cobertura funcional completa e a integração de todos os módulos da aplicação Web ainda não estão concluídas.

---

## 4. Requisitos para execução

Antes de executar o projeto, é necessário instalar:

- .NET 8 SDK.
- Visual Studio 2022 ou versão compatível.
- Workload **Desenvolvimento ASP.NET e Web**.
- Extensão ou ferramenta Entity Framework Core CLI, caso seja necessário criar ou aplicar migrações.

Para desenvolver e executar o aplicativo móvel, também são necessários os workloads e componentes do .NET MAUI e do Android.

---

## 5. Como executar no Windows

### 5.1 Abrir o projeto

Abra a solução no Visual Studio ou acesse diretamente o projeto Web:

`src/EstoqueFlow.Web/EstoqueFlow.Web.csproj`

### 5.2 Acessar a pasta do projeto Web

No terminal, a partir da raiz do repositório, execute:

```powershell
cd .\src\EstoqueFlow.Web
```

### 5.3 Restaurar as dependências

```powershell
dotnet restore
```

### 5.4 Compilar a aplicação

```powershell
dotnet build
```

### 5.5 Executar a aplicação

```powershell
dotnet run
```

Após a inicialização, o terminal exibirá o endereço local da aplicação.

Abra no navegador a URL apresentada, normalmente no formato:

`http://localhost:PORTA`

ou

`https://localhost:PORTA`

Utilize a porta efetivamente informada pelo terminal.

---

## 6. Banco de dados e migrações

O EstoqueFlow utiliza SQLite para armazenar os dados da aplicação.

O Entity Framework Core é utilizado para realizar o mapeamento entre as entidades da aplicação e as tabelas do banco de dados.

### Atualização do módulo de vendas

A inclusão do módulo de vendas adiciona a tabela `SalesOrders`.

Caso a atualização do código ainda não tenha sido aplicada ao banco de dados existente, será necessário criar e aplicar a migração correspondente.

Na pasta do projeto Web, execute:

```powershell
dotnet ef migrations add AddSalesModule
```

Depois, aplique a atualização:

```powershell
dotnet ef database update
```

Se o comando `dotnet ef` não estiver disponível, instale a ferramenta compatível com o projeto:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.11
```

### Atenção: preserve o banco existente

**Não exclua o arquivo SQLite para aplicar uma atualização.**

A migração deve ser utilizada para preservar os dados já cadastrados, incluindo produtos, movimentações, fornecedores, clientes e compras.

Antes de qualquer atualização estrutural, recomenda-se manter uma cópia de segurança do banco de dados.

---

## 7. Estrutura do projeto

```text
EstoqueFlow_PIM_IV_DotNet/
│
├── src/
│   ├── EstoqueFlow.Web/
│   │   └── Aplicação ASP.NET Core MVC
│   │
│   └── EstoqueFlow.Mobile/
│       └── Protótipo .NET MAUI
│
├── database/
│   └── Scripts SQL de referência
│
├── docs/
│   └── Relatórios, diagramas e documentação
│
└── README.md
```

---

## 8. Regras de negócio principais

O sistema possui regras para manter a consistência das operações de estoque:

1. O recebimento de um pedido de compra pendente aumenta o saldo do produto.
2. O recebimento gera automaticamente uma movimentação de entrada.
3. A confirmação de uma venda reduz o saldo disponível do produto.
4. A saída decorrente da venda é registrada no histórico de movimentações.
5. Os relatórios utilizam informações dos registros existentes para apresentar indicadores da operação.

---

## 9. Limitações e pontos de atenção

O projeto possui caráter acadêmico e ainda apresenta pontos que precisam ser considerados antes de uma utilização em ambiente real:

- O protótipo Mobile ainda não possui autenticação e autorização de usuários.
- A cobertura funcional do aplicativo móvel ainda não equivale à aplicação Web.
- A integração completa entre todos os módulos Mobile e a API precisa ser validada.
- A interface Web utiliza Bootstrap por CDN e depende de conexão com a internet para carregar esses recursos.
- São necessários testes automatizados e validações adicionais.
- É importante implementar controles de acesso adequados, backup e tratamento seguro de dados pessoais.
- O uso em produção exige avaliação adicional de segurança, infraestrutura e disponibilidade.

---

## 10. Objetivo acadêmico

O desenvolvimento do EstoqueFlow permite aplicar conhecimentos de programação orientada a objetos, desenvolvimento Web, persistência de dados, engenharia de software, integração entre aplicações e desenvolvimento móvel.

O projeto busca demonstrar, por meio de uma solução integrada, como a tecnologia pode auxiliar na organização de processos de estoque e operações comerciais.

---

**Projeto:** EstoqueFlow ERP — PIM IV  
**Curso:** Análise e Desenvolvimento de Sistemas  
**Instituição:** Universidade Paulista — UNIP  
**Natureza:** Projeto acadêmico