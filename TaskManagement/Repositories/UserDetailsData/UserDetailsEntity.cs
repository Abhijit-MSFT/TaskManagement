using Microsoft.Azure.Cosmos.Table;


namespace TaskManagement.Repositories.UserDetailsData
{
    public class UserDetailsEntity : TableEntity
    {
        public string UserID { get; set; }
        public string UserUniqueID { get; set; }
        public string EmailId { get; set; }
        public string MessageId { get; set; }
        public string Name { get; set; }
        public string ProfilePictureURL { get; set; }                     
        public string UserConversationReference { get; set; }
    }
}
