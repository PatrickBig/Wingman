using System.Text.Json.Serialization;

namespace WarThunder.Shared.Models
{
    public class HudMessage
    {
        public int Id { get; set; }

        [JsonPropertyName("msg")]
        public string? Message { get; set; }

        public string? Sender { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the event was triggered by an enemy or not.
        /// </summary>
        public bool Enemy { get; set; }

        public string? Mode { get; set; }

        /// <summary>
        /// Gets or sets the time of the event. This is the number of seconds since the match started.
        /// </summary>
        public int Time { get; set; }

        [JsonIgnore]
        public int Minutes
        {
            get
            {
                return Convert.ToInt32(Math.Floor((decimal)Time / 60));
            }
        }

        [JsonIgnore]
        public int Seconds
        {
            get
            {
                return Time % 60;
            }
        }
    }
}
