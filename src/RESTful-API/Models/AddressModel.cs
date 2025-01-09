namespace RESTful_API.Models
{
    public class AddressModel
    {
        public int AddressID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string StreetName { get; set; }
        public string? StreetAdditional { get; set; }
        public string PostalCode { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string? AdditionalNote { get; set; }
    }
}
