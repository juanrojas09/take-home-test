namespace Fundo.Applications.Apllication.Interfaces;

public interface IContextService
{
    public int? GetUserIdByToken();
    public string? GetUserNameByToken();
    public string? GetCustomerRoleByToken();
}