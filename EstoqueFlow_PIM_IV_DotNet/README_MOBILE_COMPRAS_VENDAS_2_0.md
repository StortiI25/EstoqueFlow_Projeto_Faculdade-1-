# EstoqueFlow Mobile – Compras e Vendas 2.0

## O que esta etapa adiciona

- Botão de novo pedido no módulo Compras.
- Botão de nova venda no módulo Vendas.
- Seleção de fornecedor/cliente e produto já cadastrados no servidor.
- Informar quantidade, preço unitário e observações.
- Prévia do total e confirmação antes de criar o pedido.
- Pedidos são criados como Pendente.
- Ao selecionar uma compra pendente, é possível receber a mercadoria; o endpoint existente registra a entrada no estoque.
- Ao selecionar uma venda pendente, é possível concluir a venda; o endpoint existente valida o saldo e registra a saída no estoque.
- Atualização da lista e do dashboard após as operações.

## Limitação atual do modelo de dados

O modelo atual de `PurchaseOrder` e `SalesOrder` armazena um produto por pedido. Por isso, esta entrega cria um pedido por produto. Carrinho com vários produtos no mesmo pedido exige uma evolução de banco de dados (entidades de pedido e itens) e migração própria; não foi simulada nesta etapa.

## Instalação

1. Faça backup de `data/estoqueflow.db`.
2. Extraia o ZIP e abra `EstoqueFlow.sln` no Visual Studio.
3. Mantenha o Web/API em execução no endereço configurado no `MauiProgram.cs`.
4. Execute o projeto Android no emulador ou dispositivo.

Esta etapa reutiliza os endpoints existentes da API Mobile e não altera o esquema do banco de dados.
