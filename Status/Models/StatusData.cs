namespace Flyga.AdditionalAchievements.Status.Models
{
    public class StatusData
    {
        public Status Status { get; set; }

        public string StatusMessage { get; set; }

        public StatusData(Status status, string statusMessage)
        {
            Status = status;
            StatusMessage = statusMessage;
        }
    }
}
