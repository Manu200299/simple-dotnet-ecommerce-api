namespace RESTful_API.Models
{
    public class UserAddressModel
    {
        public int UsersAddressID { get; set; }
        public int UserID { get; set; }
        public int AddressID { get; set; }
        public UserModel User { get; set; }
        public AddressModel Address { get; set; }
    }
}
