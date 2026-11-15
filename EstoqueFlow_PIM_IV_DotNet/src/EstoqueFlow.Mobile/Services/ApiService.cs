using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EstoqueFlow.Mobile.Models;

namespace EstoqueFlow.Mobile.Services;

public class ApiService(HttpClient http)
{
    public bool IsAuthenticated { get; private set; }

    public async Task<string> CheckConnectionAsync()
    {
        using var response = await http.GetAsync("api/mobile/health");
        response.EnsureSuccessStatusCode();
        return "Servidor conectado.";
    }

    public async Task<(bool Success, string Message)> LoginAsync(string email, string password)
    {
        using var response = await http.PostAsJsonAsync("api/mobile/login", new { Email = email, Password = password });
        var payload = await response.Content.ReadFromJsonAsync<ApiMessage>();
        IsAuthenticated = response.IsSuccessStatusCode;
        return (response.IsSuccessStatusCode, payload?.Message ?? (response.StatusCode == HttpStatusCode.Unauthorized
            ? "E-mail ou senha inválidos." : $"Falha no login ({(int)response.StatusCode})."));
    }

    public async Task LogoutAsync()
    {
        using var response = await http.PostAsync("api/mobile/logout", null);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
            response.EnsureSuccessStatusCode();
        IsAuthenticated = false;
    }

    public async Task<List<ProductDto>> GetProductsAsync()
    {
        using var response = await http.GetAsync("api/products");
        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            IsAuthenticated = false;
            throw new UnauthorizedAccessException("Sua sessão expirou. Entre novamente.");
        }
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ProductDto>>() ?? new();
    }


    public async Task<JsonElement> GetDataAsync(string endpoint)
    {
        using var response = await http.GetAsync($"api/mobile/data/{endpoint}");
        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            IsAuthenticated = false;
            throw new UnauthorizedAccessException("Seu usuário não tem permissão para acessar este módulo ou a sessão expirou.");
        }
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }


    public async Task SaveProductAsync(int? id, string name, string category, decimal price, int quantity, int minimumStock)
    {
        var payload = new { Name = name, Category = category, Price = price, Quantity = quantity, MinimumStock = minimumStock };
        using var response = id.HasValue
            ? await http.PutAsJsonAsync($"api/mobile/data/products/{id.Value}", payload)
            : await http.PostAsJsonAsync("api/mobile/data/products", payload);
        await EnsureSuccessWithMessage(response);
    }

    public async Task DeleteProductAsync(int id)
    {
        using var response = await http.DeleteAsync($"api/mobile/data/products/{id}");
        await EnsureSuccessWithMessage(response);
    }

    public async Task SaveContactAsync(string endpoint, int? id, string name, string document, string email, string phone, string address)
    {
        if (endpoint is not ("suppliers" or "customers")) throw new ArgumentException("Cadastro inválido.", nameof(endpoint));
        var payload = new { Name = name, Document = document, Email = email, Phone = phone, Address = address };
        using var response = id.HasValue
            ? await http.PutAsJsonAsync($"api/mobile/data/{endpoint}/{id.Value}", payload)
            : await http.PostAsJsonAsync($"api/mobile/data/{endpoint}", payload);
        await EnsureSuccessWithMessage(response);
    }

    public async Task DeleteContactAsync(string endpoint, int id)
    {
        if (endpoint is not ("suppliers" or "customers")) throw new ArgumentException("Cadastro inválido.", nameof(endpoint));
        using var response = await http.DeleteAsync($"api/mobile/data/{endpoint}/{id}");
        await EnsureSuccessWithMessage(response);
    }

    public async Task RegisterMovementAsync(int productId, int quantity, string type, string notes)
    {
        using var response = await http.PostAsJsonAsync("api/mobile/data/movements", new
        {
            ProductId = productId, Quantity = quantity, Type = type, Notes = notes
        });
        await EnsureSuccessWithMessage(response);
    }

    public async Task CreateOrderAsync(string endpoint, int partnerId, int productId, int quantity, decimal unitPrice, string notes)
    {
        if (endpoint is not ("purchases" or "sales")) throw new ArgumentException("Tipo de pedido inválido.", nameof(endpoint));
        using var response = await http.PostAsJsonAsync($"api/mobile/data/{endpoint}", new
        {
            PartnerId = partnerId, ProductId = productId, Quantity = quantity, UnitPrice = unitPrice, Notes = notes
        });
        await EnsureSuccessWithMessage(response);
    }

    public async Task CompleteOrderActionAsync(string endpoint, int id)
    {
        var action = endpoint == "purchases" ? "receive" : endpoint == "sales" ? "complete" : throw new ArgumentException("Tipo de pedido inválido.", nameof(endpoint));
        using var response = await http.PostAsync($"api/mobile/data/{endpoint}/{id}/{action}", null);
        await EnsureSuccessWithMessage(response);
    }

    private static async Task EnsureSuccessWithMessage(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync();
        string message = $"Falha na operação ({(int)response.StatusCode}).";
        try
        {
            using var json = JsonDocument.Parse(body);
            if (json.RootElement.TryGetProperty("message", out var value)) message = value.GetString() ?? message;
            else if (json.RootElement.TryGetProperty("title", out value)) message = value.GetString() ?? message;
        }
        catch { if (!string.IsNullOrWhiteSpace(body)) message = body; }
        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            throw new UnauthorizedAccessException("Sua sessão expirou ou seu perfil não permite esta operação.");
        throw new InvalidOperationException(message);
    }

    private sealed class ApiMessage { public string? Message { get; set; } }
}
