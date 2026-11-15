# EstoqueFlow Mobile — Etapa 3: operações de produtos e estoque

Esta etapa acrescenta operações reais ao aplicativo Android, utilizando a API autenticada do EstoqueFlow e o mesmo banco de dados do sistema Web.

## Recursos adicionados

- Cadastro de produto pelo aplicativo.
- Edição de nome, categoria, preço e estoque mínimo.
- Exclusão de produto quando não houver histórico vinculado (a API valida essa regra).
- Registro de entrada e saída de estoque com observação.
- Atualização do painel após operações.
- Seleção de um produto na lista para abrir ações de edição/exclusão.
- Quantidade de um produto existente não é alterada pelo formulário de edição; alterações de estoque devem ser registradas como movimentações para preservar o histórico.

## Como testar

1. Faça backup do banco `data/estoqueflow.db` antes de testar operações.
2. Inicie o Web/API com Docker como já utilizado no projeto.
3. Abra `EstoqueFlow.sln` no Visual Studio.
4. Execute `EstoqueFlow.Mobile` no emulador Android.
5. Entre com um usuário ativo.
6. Abra **Produtos** e teste `Novo produto`; toque em um produto para editar ou excluir.
7. Use **Movimentar** para registrar uma entrada ou saída. Tente uma saída maior que o estoque disponível para conferir a validação.

## Observações

- O app e o Web compartilham os dados por meio da API; não existe banco separado no Mobile.
- O backend valida quantidade positiva, estoque suficiente e vínculos históricos antes de excluir.
- A compilação Android desta entrega precisa ser confirmada no ambiente Visual Studio/Android SDK do projeto. Este ambiente de preparação não possui o SDK .NET MAUI instalado.
