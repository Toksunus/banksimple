namespace ClientService.Domain.Entities;

public class Authentification
{
    public Guid AuthentificationId { get; set; }
    public Guid ClientId { get; set; }
    public string Login { get; set; } = string.Empty;
    public string MotDePasse { get; set; } = string.Empty;
    public bool MfaActive { get; set; } = true;

    public Client? Client { get; set; }
}
