using System.ComponentModel.DataAnnotations.Schema;

namespace Lafatkotob.ViewModels
{
    public class HistoryModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime Date { get; set; }
    }
}
