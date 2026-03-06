namespace ClientService.Domain.Entities;

public class Session
{
    public Guid SessionId { get; set; }
    public Guid ClientId { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime DateExpiration { get; set; }

    public Client? Client { get; set; }
}
