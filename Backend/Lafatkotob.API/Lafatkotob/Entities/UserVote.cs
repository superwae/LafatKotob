namespace Lafatkotob.Entities
{
    public class UserVote
    {
        public int Id { get; set; }
        public string VoterUserId { get; set; }
        public string TargetUserId { get; set; }
        public bool IsUpvote { get; set; }

        public virtual AppUser VoterUser { get; set; }
        public virtual AppUser TargetUser { get; set; }
    }
}
