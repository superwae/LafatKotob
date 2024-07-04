using Lafatkotob.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.ViewModels
{
    public class ConversationsUserModel
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public int ConversationId { get; set; }


    }
}
