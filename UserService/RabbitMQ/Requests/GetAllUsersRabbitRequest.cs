namespace UserService.RabbitMQ.Requests
{
    public class GetAllUsersRabbitRequest
    {
        public string Id { get; set; }
        public string LocationScope { get; set; }
        public bool Travellers { get; set; }
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public int GenderOption { get; set; }
    }
}
