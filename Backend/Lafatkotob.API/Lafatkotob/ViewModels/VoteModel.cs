using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.ViewModels
{
    public class VoteModel
    {
        public string VoterUserId { get; set; }  // for clarity in development or testing
        public string TargetUserId { get; set; }
        public bool IsUpvote { get; set; }
    }
}