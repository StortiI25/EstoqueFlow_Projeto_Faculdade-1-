using System.Text.Json;
using EstoqueFlow.Mobile.Services;

namespace EstoqueFlow.Mobile;

public partial class MainPage : ContentPage
{
    private readonly ApiService _api;
    private string _currentEndpoint = "";
    private string _currentTitle = "";
    private List<ModuleRow> _moduleRows = new();

    public MainPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        _ = CheckConnectionAsync();
    }

    private async Task CheckConnectionAsync()
    {
        try { StatusLabel.Text = await _api.CheckConnectionAsync(); }
        catch (Exception ex) { StatusLabel.Text = $"Servidor indisponível: {ex.Message}"; }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            SetLoading(true, "Validando acesso...");
            var result = await _api.LoginAsync(EmailEntry.Text ?? "", PasswordEntry.Text ?? "");
            StatusLabel.Text = result.Message;
            if (result.Success)
            {
                LoginPanel.IsVisible = false;
                AppPanel.IsVisible = true;
                HeaderLogout.IsVisible = true;
                BottomNav.IsVisible = true;
                WelcomeLabel.Text = "Bem-vindo(a)!";
                PasswordEntry.Text = string.Empty;
                await LoadDashboardAsync();
            }
        }
        catch (Exception ex) { StatusLabel.Text = $"Falha ao entrar: {ex.Message}"; }
        finally { SetLoading(false); }
    }

    private async void OnRefreshDashboardClicked(object sender, EventArgs e) => await LoadDashboardAsync();

    private async Task LoadDashboardAsync()
    {
        try
        {
            var data = await _api.GetDataAsync("dashboard");
            KpiProducts.Text = Read(data, "totalProducts");
            KpiLowStock.Text = Read(data, "lowStock");
            KpiUnits.Text = Read(data, "stockUnits");
            var pending = int.TryParse(Read(data, "pendingPurchases"), out var p) ? p : 0;
            pending += int.TryParse(Read(data, "pendingSales"), out var s) ? s : 0;
            KpiOrders.Text = pending.ToString();
            StatusLabel.Text = "Painel atualizado com os dados do servidor.";
        }
        catch (Exception ex) { StatusLabel.Text = $"Não foi possível carregar o painel: {ex.Message}"; }
    }

    private static string Read(JsonElement element, string property)
        => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(property, out var value) ? value.ToString() : "0";

    private async Task OpenModuleAsync(string title, string endpoint, string description)
    {
        _currentTitle = title;
        _currentEndpoint = endpoint;
        LoginPanel.IsVisible = false;
        AppPanel.IsVisible = false;
        ModulePanel.IsVisible = true;
        ModuleTitle.Text = title;
        ModuleDescription.Text = description;
        ModuleSearch.Text = string.Empty;
        ProductActions.IsVisible = endpoint == "products";
        MovementAction.IsVisible = endpoint == "movements";
        OrderActions.IsVisible = endpoint is "purchases" or "sales";
        ContactActions.IsVisible = endpoint is "suppliers" or "customers";
        CreateContactButton.Text = endpoint == "suppliers" ? "＋ Novo fornecedor" : "＋ Novo cliente";
        CreateOrderButton.Text = endpoint == "purchases" ? "＋ Novo pedido de compra" : "＋ Nova venda";
        await LoadModuleAsync();
    }

    private async Task LoadModuleAsync()
    {
        try
        {
            SetLoading(true, $"Carregando {_currentTitle.ToLowerInvariant()}...");
            var data = await _api.GetDataAsync(_currentEndpoint);
            var items = new List<ModuleRow>();
            if (data.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in data.EnumerateArray()) items.Add(ToRow(item));
            }
            else if (data.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in data.EnumerateObject())
                {
                    if (_currentEndpoint == "reports" && property.Name == "lowStock")
                    {
                        var count = property.Value.ValueKind == JsonValueKind.Array ? property.Value.GetArrayLength() : 0;
                        var names = property.Value.ValueKind == JsonValueKind.Array
                            ? string.Join(", ", property.Value.EnumerateArray().Select(x => x.TryGetProperty("name", out var n) ? n.GetString() : null).Where(x => !string.IsNullOrWhiteSpace(x)))
                            : string.Empty;
                        items.Add(new ModuleRow { Title = "Produtos com estoque baixo", Subtitle = count == 0 ? "Nenhum produto precisa de reposição." : $"{count} produto(s): {names}", Icon = "!" });
                        continue;
                    }
                    if (_currentEndpoint == "settings" && property.Name is "id" or "updatedAt") continue;
                    var display = FormatObjectValue(property.Name, property.Value);
                    items.Add(new ModuleRow { Title = Friendly(property.Name), Subtitle = display, Icon = _currentEndpoint == "settings" ? "⚙" : "◇" });
                }
            }
            _moduleRows = items;
            ApplyModuleFilter();
            StatusLabel.Text = $"{items.Count} registro(s) carregado(s).";
        }
        catch (Exception ex)
        {
            _moduleRows = new List<ModuleRow>();
            ModuleList.ItemsSource = null;
            ModuleCount.Text = "Não foi possível carregar";
            EmptyState.Text = ex is UnauthorizedAccessException ? "Acesso não autorizado. Verifique seu perfil." : "Não foi possível carregar os dados. Tente atualizar.";
            EmptyState.IsVisible = true;
            StatusLabel.Text = ex is UnauthorizedAccessException ? "Acesso não autorizado. Verifique seu perfil." : $"Falha ao carregar: {ex.Message}";
        }
        finally { SetLoading(false); }
    }

    private void OnModuleSearchTextChanged(object sender, TextChangedEventArgs e) => ApplyModuleFilter();

    private void ApplyModuleFilter()
    {
        var term = ModuleSearch?.Text?.Trim() ?? string.Empty;
        var filtered = string.IsNullOrWhiteSpace(term)
            ? _moduleRows
            : _moduleRows.Where(x => x.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
                                  || x.Subtitle.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        ModuleList.ItemsSource = filtered;
        ModuleCount.Text = $"{filtered.Count} de {_moduleRows.Count} registro(s)";
        EmptyState.Text = _moduleRows.Count == 0 ? "Ainda não há registros neste módulo." : "Nenhum resultado para sua pesquisa.";
        EmptyState.IsVisible = filtered.Count == 0;
    }

    private static string FormatObjectValue(string key, JsonElement value)
    {
        if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined) return "Não informado";
        if (value.ValueKind == JsonValueKind.Array) return value.GetArrayLength().ToString();
        if (value.ValueKind == JsonValueKind.Object) return "Detalhes disponíveis";
        var raw = value.ToString();
        if (key is "stockValue" or "completedSalesValue" && decimal.TryParse(raw, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var money))
            return money.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
        if (key == "updatedAt" && DateTimeOffset.TryParse(raw, out var date))
            return date.ToLocalTime().ToString("dd/MM/yyyy 'às' HH:mm", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
        return string.IsNullOrWhiteSpace(raw) ? "Não informado" : raw;
    }

    private static ModuleRow ToRow(JsonElement item)
    {
        if (item.ValueKind != JsonValueKind.Object)
            return new ModuleRow { Title = item.ToString(), Subtitle = "" };

        string Get(params string[] names)
        {
            foreach (var name in names)
                if (item.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null)
                    return value.ToString();
            return "";
        }

        var title = Get("name", "fullName", "companyName", "product", "supplier", "customer", "title", "id");
        var details = new List<string>();
        AddDetail(details, "category", Get("category"));
        AddDetail(details, "quantity", Get("quantity"));
        AddDetail(details, "minimumStock", Get("minimumStock"));
        AddDetail(details, "price", Get("price", "unitPrice"));
        AddDetail(details, "total", Get("total"));
        AddDetail(details, "status", Get("status"));
        AddDetail(details, "type", Get("type"));
        AddDetail(details, "email", Get("email"));
        AddDetail(details, "phone", Get("phone"));
        AddDetail(details, "document", Get("document"));
        AddDetail(details, "address", Get("address"));
        AddDetail(details, "supplier", Get("supplier"));
        AddDetail(details, "customer", Get("customer"));
        AddDetail(details, "product", Get("product"));
        AddDetail(details, "createdAt", Get("createdAt"));

        var idText = Get("id");
        int.TryParse(idText, out var id);
        return new ModuleRow
        {
            Id = id,
            Title = string.IsNullOrWhiteSpace(title) ? "Registro" : title,
            Subtitle = string.Join("  •  ", details),
            Icon = "▦",
            Category = Get("category"),
            Price = Get("price", "unitPrice"),
            Quantity = Get("quantity"),
            MinimumStock = Get("minimumStock"),
            Status = Get("status"),
            Document = Get("document"),
            Email = Get("email"),
            Phone = Get("phone"),
            Address = Get("address")
        };
    }

    private static void AddDetail(List<string> details, string key, string rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue)) return;
        var value = rawValue;
        switch (key)
        {
            case "price":
            case "total":
                if (decimal.TryParse(rawValue, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var money))
                    value = money.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
                break;
            case "createdAt":
                if (DateTimeOffset.TryParse(rawValue, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AssumeUniversal, out var date))
                    value = date.ToLocalTime().ToString("dd/MM/yyyy 'às' HH:mm", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
                break;
            case "type":
                value = rawValue.ToUpperInvariant() switch
                {
                    "ENTRY" => "Entrada",
                    "EXIT" => "Saída",
                    _ => rawValue
                };
                break;
            case "status":
                value = rawValue.Trim().ToLowerInvariant() switch
                {
                    "pending" or "pendente" => "Pendente",
                    "received" or "recebido" => "Recebido",
                    "completed" or "concluída" or "concluida" => "Concluída",
                    "cancelled" or "cancelada" => "Cancelada",
                    _ => rawValue
                };
                break;
        }
        details.Add($"{Friendly(key)}: {value}");
    }

    private static string Friendly(string value) => value switch
    {
        "totalProducts" => "Produtos",
        "productCount" => "Produtos cadastrados",
        "purchaseCount" => "Pedidos de compra",
        "salesCount" => "Pedidos de venda",
        "companyName" => "Nome da empresa",
        "updatedAt" => "Última atualização",
        "lowStock" => "Estoque baixo",
        "stockUnits" => "Unidades em estoque",
        "stockValue" => "Valor do estoque",
        "suppliers" => "Fornecedores",
        "customers" => "Clientes",
        "pendingPurchases" => "Compras pendentes",
        "pendingSales" => "Vendas pendentes",
        "completedSalesValue" => "Vendas concluídas",
        "minimumStock" => "Estoque mínimo",
        "createdAt" => "Criado em",
        "category" => "Categoria",
        "quantity" => "Quantidade",
        "price" => "Preço",
        "unitPrice" => "Preço unitário",
        "total" => "Total",
        "status" => "Situação",
        "type" => "Movimentação",
        "email" => "E-mail",
        "phone" => "Telefone",
        "document" => "CPF/CNPJ",
        "address" => "Endereço",
        "product" => "Produto",
        "supplier" => "Fornecedor",
        "customer" => "Cliente",
        "notes" => "Observações",
        "receivedAt" => "Recebido em",
        "completedAt" => "Concluído em",
        _ => System.Text.RegularExpressions.Regex.Replace(value, "([a-z])([A-Z])", "$1 $2")
    };

    private async void OnHomeNav(object sender, EventArgs e)
    {
        ModulePanel.IsVisible = false; AppPanel.IsVisible = true;
        await LoadDashboardAsync();
    }
    private async void OnProductsNav(object sender, EventArgs e) => await OpenModuleAsync("Produtos", "products", "Catálogo de produtos e níveis de estoque.");
    private async void OnMovementsNav(object sender, EventArgs e) => await OpenModuleAsync("Movimentações", "movements", "Histórico de entradas e saídas do estoque.");
    private async void OnSuppliersNav(object sender, EventArgs e) => await OpenModuleAsync("Fornecedores", "suppliers", "Fornecedores cadastrados no sistema.");
    private async void OnCustomersNav(object sender, EventArgs e) => await OpenModuleAsync("Clientes", "customers", "Clientes cadastrados no sistema.");
    private async void OnPurchasesNav(object sender, EventArgs e) => await OpenModuleAsync("Compras", "purchases", "Pedidos de compra e recebimentos.");
    private async void OnSalesNav(object sender, EventArgs e) => await OpenModuleAsync("Vendas", "sales", "Pedidos de venda e situação dos pedidos.");
    private async void OnReportsNav(object sender, EventArgs e) => await OpenModuleAsync("Relatórios", "reports", "Indicadores consolidados do EstoqueFlow.");
    private async void OnMoreNav(object sender, EventArgs e)
    {
        var choice = await DisplayActionSheet("Mais opções", "Cancelar", null, "Funcionários", "Configurações");
        if (choice == "Funcionários") await OpenModuleAsync("Funcionários", "employees", "Acesso restrito a administradores.");
        else if (choice == "Configurações") await OpenModuleAsync("Configurações", "settings", "Configurações da empresa. Acesso restrito a administradores.");
    }
    private async void OnRefreshModuleClicked(object sender, EventArgs e) => await LoadModuleAsync();

    private async void OnCreateProductClicked(object sender, EventArgs e)
        => await ShowProductFormAsync(null);

    private async void OnCreateContactClicked(object sender, EventArgs e)
        => await ShowContactFormAsync(null);

    private async Task ShowContactFormAsync(ModuleRow? selected)
    {
        var isSupplier = _currentEndpoint == "suppliers";
        var entity = isSupplier ? "fornecedor" : "cliente";
        var name = await DisplayPromptAsync(selected is null ? $"Novo {entity}" : $"Editar {entity}", "Nome ou razão social", initialValue: selected?.Title ?? "", maxLength: 120);
        if (name is null) return;
        if (string.IsNullOrWhiteSpace(name)) { await DisplayAlert("Atenção", "Informe o nome.", "OK"); return; }
        var document = await DisplayPromptAsync("Documento", "CPF ou CNPJ", initialValue: selected?.Document ?? "", maxLength: 20);
        if (document is null) return;
        var email = await DisplayPromptAsync("E-mail", "E-mail de contato", initialValue: selected?.Email ?? "", keyboard: Keyboard.Email, maxLength: 120);
        if (email is null) return;
        var phone = await DisplayPromptAsync("Telefone", "Telefone ou celular", initialValue: selected?.Phone ?? "", keyboard: Keyboard.Telephone, maxLength: 30);
        if (phone is null) return;
        var address = await DisplayPromptAsync("Endereço", "Rua, número, bairro e cidade", initialValue: selected?.Address ?? "", maxLength: 180);
        if (address is null) return;
        try
        {
            SetLoading(true, $"Salvando {entity}...");
            await _api.SaveContactAsync(_currentEndpoint, selected?.Id, name.Trim(), document, email, phone, address);
            await DisplayAlert("Sucesso", selected is null ? $"{entity[..1].ToUpperInvariant() + entity[1..]} cadastrado(a)." : $"{entity[..1].ToUpperInvariant() + entity[1..]} atualizado(a).", "OK");
            await LoadModuleAsync();
            await LoadDashboardAsync();
        }
        catch (Exception ex) { await DisplayAlert("Não foi possível salvar", ex.Message, "OK"); }
        finally { SetLoading(false); }
    }


    private async Task ShowProductFormAsync(ModuleRow? selected)
    {
        var name = await DisplayPromptAsync(selected is null ? "Novo produto" : "Editar produto", "Nome do produto", initialValue: selected?.Title ?? "", maxLength: 100);
        if (name is null) return;
        if (string.IsNullOrWhiteSpace(name)) { await DisplayAlert("Atenção", "Informe o nome do produto.", "OK"); return; }
        var category = await DisplayPromptAsync("Categoria", "Categoria do produto", initialValue: selected?.Category ?? "", maxLength: 60);
        if (category is null) return;
        var priceText = await DisplayPromptAsync("Preço", "Preço unitário (ex.: 29,90)", initialValue: selected?.Price ?? "0", keyboard: Keyboard.Numeric);
        if (priceText is null) return;
        var qtyText = selected is null
            ? await DisplayPromptAsync("Quantidade inicial", "Quantidade inicial em estoque", initialValue: "0", keyboard: Keyboard.Numeric)
            : selected.Quantity;
        if (qtyText is null) return;
        var minText = await DisplayPromptAsync("Estoque mínimo", "Quantidade mínima", initialValue: selected?.MinimumStock ?? "0", keyboard: Keyboard.Numeric);
        if (minText is null) return;
        var culture = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
        if (!decimal.TryParse(priceText, System.Globalization.NumberStyles.Number, culture, out var price) || price < 0 ||
            !int.TryParse(qtyText, out var quantity) || quantity < 0 || !int.TryParse(minText, out var minimum) || minimum < 0)
        {
            await DisplayAlert("Dados inválidos", "Confira preço, quantidade e estoque mínimo.", "OK"); return;
        }
        try
        {
            SetLoading(true, "Salvando produto...");
            await _api.SaveProductAsync(selected?.Id, name.Trim(), category ?? "", price, quantity, minimum);
            await DisplayAlert("Sucesso", selected is null ? "Produto cadastrado." : "Produto atualizado.", "OK");
            await LoadModuleAsync();
            await LoadDashboardAsync();
        }
        catch (Exception ex) { await DisplayAlert("Não foi possível salvar", ex.Message, "OK"); }
        finally { SetLoading(false); }
    }

    private async void OnModuleSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not ModuleRow row) return;
        ModuleList.SelectedItem = null;
        if (_currentEndpoint is "purchases" or "sales")
        {
            if (!string.Equals(row.Status, "Pendente", StringComparison.OrdinalIgnoreCase))
            {
                await DisplayAlert("Pedido", $"Este pedido está com situação: {row.Status}.", "OK");
                return;
            }
            var actionLabel = _currentEndpoint == "purchases" ? "Receber mercadoria" : "Concluir venda";
            var action = await DisplayActionSheet(row.Title, "Cancelar", null, "Ver situação", actionLabel);
            if (action == actionLabel)
            {
                var confirmation = _currentEndpoint == "purchases"
                    ? "Confirmar o recebimento? A quantidade será adicionada ao estoque."
                    : "Confirmar a venda? A quantidade será retirada do estoque.";
                if (!await DisplayAlert(actionLabel, confirmation, "Confirmar", "Cancelar")) return;
                try
                {
                    SetLoading(true, "Atualizando pedido...");
                    await _api.CompleteOrderActionAsync(_currentEndpoint, row.Id);
                    await DisplayAlert("Sucesso", _currentEndpoint == "purchases" ? "Compra recebida e estoque atualizado." : "Venda concluída e estoque atualizado.", "OK");
                    await LoadModuleAsync();
                    await LoadDashboardAsync();
                }
                catch (Exception ex) { await DisplayAlert("Não foi possível concluir", ex.Message, "OK"); }
                finally { SetLoading(false); }
            }
        }
        else if (_currentEndpoint is "suppliers" or "customers")
        {
            var entity = _currentEndpoint == "suppliers" ? "fornecedor" : "cliente";
            var action = await DisplayActionSheet(row.Title, "Cancelar", null, "Ver detalhes", "Editar", "Excluir");
            if (action == "Ver detalhes")
                await DisplayAlert(row.Title, $"CPF/CNPJ: {row.Document}\nE-mail: {row.Email}\nTelefone: {row.Phone}\nEndereço: {row.Address}", "Fechar");
            else if (action == "Editar") await ShowContactFormAsync(row);
            else if (action == "Excluir")
            {
                if (!await DisplayAlert($"Excluir {entity}", $"Deseja excluir {row.Title}? Registros vinculados a pedidos não podem ser excluídos.", "Excluir", "Cancelar")) return;
                try
                {
                    SetLoading(true, $"Excluindo {entity}...");
                    await _api.DeleteContactAsync(_currentEndpoint, row.Id);
                    await DisplayAlert("Sucesso", $"{entity[..1].ToUpperInvariant() + entity[1..]} excluído(a).", "OK");
                    await LoadModuleAsync();
                    await LoadDashboardAsync();
                }
                catch (Exception ex) { await DisplayAlert("Não foi possível excluir", ex.Message, "OK"); }
                finally { SetLoading(false); }
            }
        }
        else if (_currentEndpoint == "products")
        {
            var action = await DisplayActionSheet(row.Title, "Cancelar", null, "Editar", "Excluir");
            if (action == "Editar") await ShowProductFormAsync(row);
            else if (action == "Excluir")
            {
                var confirm = await DisplayAlert("Excluir produto", $"Deseja excluir {row.Title}? Produtos com histórico não podem ser excluídos.", "Excluir", "Cancelar");
                if (!confirm) return;
                try { await _api.DeleteProductAsync(row.Id); await LoadModuleAsync(); await LoadDashboardAsync(); }
                catch (Exception ex) { await DisplayAlert("Não foi possível excluir", ex.Message, "OK"); }
            }
        }
    }

    private async void OnCreateOrderClicked(object sender, EventArgs e)
    {
        var isPurchase = _currentEndpoint == "purchases";
        var partnerEndpoint = isPurchase ? "suppliers" : "customers";
        var partnerLabel = isPurchase ? "fornecedor" : "cliente";
        try
        {
            var partnersJson = await _api.GetDataAsync(partnerEndpoint);
            var partners = partnersJson.EnumerateArray().Select(x => new
            {
                Id = x.GetProperty("id").GetInt32(),
                Name = x.TryGetProperty("name", out var n) ? n.GetString() ?? partnerLabel : partnerLabel
            }).ToList();
            if (partners.Count == 0)
            {
                await DisplayAlert("Cadastro necessário", $"Cadastre um {partnerLabel} antes de criar o pedido.", "OK");
                return;
            }

            var productsJson = await _api.GetDataAsync("products");
            var products = productsJson.EnumerateArray().Select(x => new
            {
                Id = x.GetProperty("id").GetInt32(),
                Name = x.TryGetProperty("name", out var n) ? n.GetString() ?? "Produto" : "Produto",
                Price = x.TryGetProperty("price", out var price) && price.TryGetDecimal(out var parsed) ? parsed : 0m,
                Quantity = x.TryGetProperty("quantity", out var qty) && qty.TryGetInt32(out var q) ? q : 0
            }).ToList();
            if (products.Count == 0) { await DisplayAlert("Cadastro necessário", "Cadastre um produto antes de criar o pedido.", "OK"); return; }

            var partnerChoice = await DisplayActionSheet($"Selecione o {partnerLabel}", "Cancelar", null, partners.Select(x => x.Name).ToArray());
            var partner = partners.FirstOrDefault(x => x.Name == partnerChoice);
            if (partner is null) return;

            var productChoice = await DisplayActionSheet("Selecione o produto", "Cancelar", null, products.Select(x => $"{x.Name} (estoque: {x.Quantity})").ToArray());
            var product = products.FirstOrDefault(x => productChoice == $"{x.Name} (estoque: {x.Quantity})");
            if (product is null) return;
            if (!isPurchase && product.Quantity <= 0)
            {
                await DisplayAlert("Estoque indisponível", "Este produto não possui unidades disponíveis para venda.", "OK");
                return;
            }

            var quantityText = await DisplayPromptAsync("Quantidade", "Informe a quantidade do produto", initialValue: "1", keyboard: Keyboard.Numeric);
            if (quantityText is null) return;
            if (!int.TryParse(quantityText, out var quantity) || quantity <= 0)
            {
                await DisplayAlert("Quantidade inválida", "Informe um número inteiro maior que zero.", "OK");
                return;
            }
            if (!isPurchase && quantity > product.Quantity)
            {
                await DisplayAlert("Estoque insuficiente", $"Disponível: {product.Quantity} unidade(s).", "OK");
                return;
            }

            var culture = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
            var priceText = await DisplayPromptAsync("Preço unitário", "Informe o preço por unidade", initialValue: product.Price.ToString("0.00", culture), keyboard: Keyboard.Numeric);
            if (priceText is null) return;
            if (!decimal.TryParse(priceText, System.Globalization.NumberStyles.Number, culture, out var unitPrice) || unitPrice <= 0)
            {
                await DisplayAlert("Preço inválido", "Informe um preço maior que zero. Exemplo: 29,90.", "OK");
                return;
            }
            var notes = await DisplayPromptAsync("Observações", "Informações adicionais (opcional)", maxLength: 250) ?? "";
            var total = quantity * unitPrice;
            var confirmationText = $"{(isPurchase ? "Compra" : "Venda")}\n{partnerLabel}: {partner.Name}\nProduto: {product.Name}\nQuantidade: {quantity}\nTotal: {total.ToString("C2", culture)}\n\nO pedido será criado como Pendente.";
            if (!await DisplayAlert("Confirmar pedido", confirmationText, "Criar pedido", "Cancelar")) return;

            SetLoading(true, "Salvando pedido...");
            await _api.CreateOrderAsync(_currentEndpoint, partner.Id, product.Id, quantity, unitPrice, notes);
            await DisplayAlert("Pedido criado", isPurchase ? "Compra criada. Receba a mercadoria quando ela chegar para atualizar o estoque." : "Venda criada. Conclua o pedido para baixar o estoque.", "OK");
            await LoadModuleAsync();
            await LoadDashboardAsync();
        }
        catch (Exception ex) { await DisplayAlert("Não foi possível criar o pedido", ex.Message, "OK"); }
        finally { SetLoading(false); }
    }

    private async void OnRegisterMovementClicked(object sender, EventArgs e)
    {
        try
        {
            var productsJson = await _api.GetDataAsync("products");
            var products = productsJson.EnumerateArray().Select(p => new
            {
                Id = p.GetProperty("id").GetInt32(),
                Name = p.TryGetProperty("name", out var n) ? n.GetString() ?? "Produto" : "Produto"
            }).ToList();
            if (products.Count == 0) { await DisplayAlert("Estoque", "Cadastre um produto primeiro.", "OK"); return; }
            var selectedName = await DisplayActionSheet("Selecione o produto", "Cancelar", null, products.Select(p => p.Name).ToArray());
            var selected = products.FirstOrDefault(p => p.Name == selectedName);
            if (selected is null) return;
            var type = await DisplayActionSheet("Tipo de movimentação", "Cancelar", null, "Entrada", "Saída");
            if (type is not ("Entrada" or "Saída")) return;
            var quantityText = await DisplayPromptAsync(type, "Informe a quantidade", keyboard: Keyboard.Numeric);
            if (quantityText is null) return;
            if (!int.TryParse(quantityText, out var quantity) || quantity <= 0) { await DisplayAlert("Quantidade inválida", "Informe um número inteiro maior que zero.", "OK"); return; }
            var notes = await DisplayPromptAsync("Observação", "Motivo (opcional)", maxLength: 200);
            if (notes is null) notes = "";
            SetLoading(true, "Registrando movimentação...");
            await _api.RegisterMovementAsync(selected.Id, quantity, type == "Entrada" ? "ENTRY" : "EXIT", notes);
            await DisplayAlert("Sucesso", "Movimentação registrada e estoque atualizado.", "OK");
            await LoadModuleAsync(); await LoadDashboardAsync();
        }
        catch (Exception ex) { await DisplayAlert("Falha na movimentação", ex.Message, "OK"); }
        finally { SetLoading(false); }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        try { await _api.LogoutAsync(); } catch { }
        LoginPanel.IsVisible = true; AppPanel.IsVisible = false; ModulePanel.IsVisible = false;
        HeaderLogout.IsVisible = false; BottomNav.IsVisible = false; ModuleList.ItemsSource = null;
        _moduleRows.Clear();
        StatusLabel.Text = "Sessão encerrada.";
    }

    private void SetLoading(bool active, string? message = null)
    {
        Loading.IsVisible = active; Loading.IsRunning = active;
        if (message is not null) StatusLabel.Text = message;
    }

    private sealed class ModuleRow
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string Icon { get; set; } = "▦";
        public string Category { get; set; } = "";
        public string Price { get; set; } = "0";
        public string Quantity { get; set; } = "0";
        public string MinimumStock { get; set; } = "0";
        public string Status { get; set; } = "";
        public string Document { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
    }
}
