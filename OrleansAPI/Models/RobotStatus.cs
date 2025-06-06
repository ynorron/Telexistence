[GenerateSerializer]
public class RobotStatus
{
    [Id(0)] public string RobotId { get; set; } = string.Empty;
    [Id(1)] public int X { get; set; } = 0;
    [Id(2)] public int Y { get; set; } = 0;
    [Id(3)] public int Z { get; set; } = 0;
    [Id(4)] public int Rotation { get; set; } = 0; // degrees
    [Id(5)] public string Task { get; set; } = "Idle";
    [Id(6)] public string Status { get; set; } = "Ready";
}
