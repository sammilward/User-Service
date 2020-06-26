namespace UserService.Models
{
    public class GetUsersFilters
    {
        public string LocationScope { get; set; }
        public bool Travellers { get; set; }
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public int GenderOption { get; set; }
    }
}
