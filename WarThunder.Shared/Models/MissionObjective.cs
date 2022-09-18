namespace WarThunder.Shared.Models
{
    public class MissionObjective : IEquatable<MissionObjective?>
    {
        public bool Primary { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;

        public override bool Equals(object? obj)
        {
            return Equals(obj as MissionObjective);
        }

        public bool Equals(MissionObjective? other)
        {
            return other is not null &&
                   Primary == other.Primary &&
                   Status == other.Status &&
                   Text == other.Text;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Primary, Status, Text);
        }

        public static bool operator ==(MissionObjective? left, MissionObjective? right)
        {
            return EqualityComparer<MissionObjective>.Default.Equals(left, right);
        }

        public static bool operator !=(MissionObjective? left, MissionObjective? right)
        {
            return !(left == right);
        }
    }
}
