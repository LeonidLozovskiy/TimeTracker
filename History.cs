using LiteDB;

#nullable disable
namespace TimeTracker
{
    public class History
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public DateTime Date { get; set; }

        public string DateString
        {
            get
            {
                DateTime date = Date;
                date = date.Date;
                return date.ToString("dddd d-M-yyyy");
            }
        }

        public List<HistoryRow> Rows { get; set; }

        [BsonIgnore]
        public int TimeSum => Rows.Sum(x => x.TrackedTime) / 60;
    }
}
