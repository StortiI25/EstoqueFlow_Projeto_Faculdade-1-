# Roteiro de apresentação — EstoqueFlow (.NET)

## Explicação em 1 minuto

“O EstoqueFlow é um sistema para organizar o estoque de pequenos negócios. A versão anterior foi planejada com Python e Flask. No PIM IV, estamos evoluindo a solução: o sistema Web será desenvolvido em C# com ASP.NET Core MVC, e o aplicativo móvel será feito em .NET MAUI. Os dois conversam com o servidor por uma API REST. O banco SQLite fica no servidor, e o Entity Framework Core ajuda o programa a salvar e consultar os dados. Assim, Web e Mobile trabalham com as mesmas informações.”

## O que cada tecnologia faz

- **C#**: linguagem usada para escrever as regras do sistema.
- **ASP.NET Core MVC**: organiza o sistema Web em Model, View e Controller.
- **API REST**: conjunto de endereços que permite que outros programas consultem e enviem dados.
- **.NET MAUI**: tecnologia para criar a interface do aplicativo móvel.
- **SQLite**: banco de dados que guarda os produtos e movimentações em arquivo.
- **Entity Framework Core**: ferramenta que conecta as classes C# às tabelas do banco.

## Fluxo para demonstrar

1. Abrir o painel Web.
2. Cadastrar um produto com quantidade inicial.
3. Abrir Movimentações e registrar uma entrada.
4. Registrar uma saída menor ou igual ao saldo.
5. Tentar uma saída maior que o saldo: o sistema deve recusar.
6. Consultar `GET /api/products` e mostrar que os dados vêm do mesmo banco.
7. Abrir o aplicativo MAUI, tocar em “Carregar produtos” e explicar que ele consome a API.

## Perguntas comuns

**Por que o SQLite?**  
Porque é leve, simples de configurar e adequado ao protótipo acadêmico. O banco fica no servidor Web. Para uma implantação com muitos acessos simultâneos, podemos avaliar outro SGBD.

**Por que o Mobile não acessa o banco diretamente?**  
Porque isso exporia o arquivo e acoplaria o aplicativo à estrutura interna do banco. O Mobile usa a API, que valida e centraliza as operações.

**O que é MVC?**  
Model representa os dados; View é a tela; Controller recebe a ação do usuário e coordena a resposta.

**Como o estoque é atualizado?**  
O usuário registra uma entrada ou saída. A regra de negócio valida a quantidade, atualiza o saldo e grava a movimentação no mesmo processo.

**O que falta para produção?**  
Autenticação e autorização da API, testes, logs, backup, configuração de HTTPS e armazenamento seguro de credenciais no Mobile. O protótipo não deve ser publicado em produção sem esses controles.
