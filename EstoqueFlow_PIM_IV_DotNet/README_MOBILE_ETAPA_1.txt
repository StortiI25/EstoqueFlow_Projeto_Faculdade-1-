ESTOQUEFLOW - EXPANSAO DA API MOBILE (ETAPA 1)

Esta etapa adiciona endpoints autenticados para os módulos do aplicativo, mantendo o banco SQLite compartilhado com o Web.

Rotas adicionadas sob /api/mobile/data:
GET  dashboard
GET/POST/PUT/DELETE products (PUT e DELETE por id)
GET/POST movements
GET/POST/PUT/DELETE suppliers
GET/POST/PUT/DELETE customers
GET/POST purchases e POST purchases/{id}/receive
GET/POST sales e POST sales/{id}/complete
GET employees (somente Administrador)
GET/PUT settings (somente Administrador)
GET reports

A autenticação usa o cookie emitido por POST /api/mobile/login. O endpoint /api/mobile/health permanece público.

IMPORTANTE:
- Esta é uma etapa de API, não a versão final do aplicativo completo.
- A interface Mobile ainda precisa de telas e formulários para consumir todos esses endpoints.
- Não foi possível compilar .NET neste ambiente; compile e teste no Visual Studio antes de substituir o projeto em uso.
- Faça backup de data/estoqueflow.db antes de qualquer atualização. Não execute migrations/InitialCreate sobre o banco existente.
