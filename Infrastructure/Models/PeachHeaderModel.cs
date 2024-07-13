namespace Infrastructure.Models
{
    public class PeachHeaderModel
    {
        public string? ApiVersion {get; set;}
        public string Client {get; set;}
        public string Service {get; set;}
        public string ServiceType {get; set;}
        public string DueDate {get; set;}
        public string Reference {get; set;}
        public string CallBackUrl {get; set;}
        public string BankAccount {get; set;}
    }
}