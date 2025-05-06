namespace Proiect___Implementare_Software.Repositories
{
    public interface IEmailRepository
    {
        Task SaveEmailLogAsync(string email, string subject, string message);
    }
}

