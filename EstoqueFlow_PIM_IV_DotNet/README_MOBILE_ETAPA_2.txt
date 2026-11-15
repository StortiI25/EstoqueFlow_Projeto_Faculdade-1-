ESTOQUEFLOW MOBILE - ETAPA 2: NAVEGACAO E PAINEL

Esta etapa atualiza a interface do aplicativo MAUI com:
- Painel inicial com indicadores consultados pela API.
- Navegacao inferior para Inicio, Produtos, Estoque, Vendas e Mais.
- Listagem de Produtos, Movimentacoes, Fornecedores, Clientes, Compras, Vendas e Relatorios.
- Acesso a Funcionarios e Configuracoes respeitando as autorizacoes retornadas pela API.
- Botao de atualizacao das listagens e encerramento de sessao.

IMPORTANTE - ESCOPO DESTA ENTREGA
Esta etapa implementa painel, navegacao e consulta/listagem dos modulos existentes na API.
Ainda nao implementa formularios de inclusao/edicao/exclusao, nem acoes de confirmar venda ou receber compra pela interface mobile. Portanto, nao e ainda a versao final com paridade total com o Web.

REQUISITOS
- .NET SDK 8.0.420 (conforme global.json)
- Workload MAUI Android
- Android SDK API 34
- Docker Compose do projeto em execucao na porta 8080

EXECUTAR NO EMULADOR (PowerShell, na pasta raiz do projeto)
1) Iniciar o servidor: docker compose up -d --build
2) Compilar e executar:
   dotnet build .\src\EstoqueFlow.Mobile\EstoqueFlow.Mobile.csproj -t:Run -f net8.0-android -p:AndroidSdkDirectory="$env:LOCALAPPDATA\Android\Sdk"

O emulador Android acessa o host pelo endereco 10.0.2.2:8080, configurado em MauiProgram.cs.
O banco de dados compartilhado permanece no projeto Web/Docker. Nao apagar nem recriar o arquivo estoqueflow.db.
