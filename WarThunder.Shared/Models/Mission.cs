namespace WarThunder.Shared.Models
{
    public class Mission : IEquatable<Mission?>
    {
        public MissionObjective[] Objectives { get; set; } = Array.Empty<MissionObjective>();

        public string Status { get; set; } = string.Empty;

        public override bool Equals(object? obj)
        {
            return Equals(obj as Mission);
        }

        public bool Equals(Mission? other)
        {
            return other is not null &&
                   EqualityComparer<MissionObjective[]>.Default.Equals(Objectives, other.Objectives) &&
                   Status == other.Status;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Objectives, Status);
        }

        public static bool operator ==(Mission? left, Mission? right)
        {
            return EqualityComparer<Mission>.Default.Equals(left, right);
        }

        public static bool operator !=(Mission? left, Mission? right)
        {
            return !(left == right);
        }
    }
}
