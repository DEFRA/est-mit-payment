namespace EST.MIT.Payment.Interfaces
{
    public interface IEventQueueService
    {
        Task CreateMessage(string id, string status, string action, string message, string data);
    }
}
