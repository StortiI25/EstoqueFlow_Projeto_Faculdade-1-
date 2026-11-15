# Modelo de dados — EstoqueFlow ERP

## Product
Produto, categoria, preço, quantidade atual e estoque mínimo.

## StockMovement
Movimentação de entrada ou saída, produto relacionado, quantidade, observação e data.

## Supplier / Customer
Dados de identificação e contato de fornecedores e clientes.

## PurchaseOrder
Pedido de compra com fornecedor, produto, quantidade, preço unitário, observação, status e datas. Ao registrar recebimento, o sistema atualiza o saldo do produto e grava uma movimentação `ENTRY`.

## SalesOrder
Pedido de venda com cliente, produto, quantidade, preço unitário, observação, status e datas. Ao confirmar, o sistema verifica saldo disponível, reduz o estoque e grava uma movimentação `EXIT`.

## Relacionamentos
- StockMovement → Product (N:1)
- PurchaseOrder → Supplier (N:1)
- PurchaseOrder → Product (N:1)
- SalesOrder → Customer (N:1)
- SalesOrder → Product (N:1)
