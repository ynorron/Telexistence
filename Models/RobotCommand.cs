
using System;
using Orleans;
using Orleans.Serialization;

namespace Telexistence.Models
{
    /// <summary>
    /// Command sent to a robot (e.g., move/rotate/stop).
    /// </summary>
    [GenerateSerializer]
    public class RobotCommand
    {
        /// <summary>Unique command identifier (set by server).</summary>
        [Id(0)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>The robot's unique ID.</summary>
        [Id(1)]
        required
        public string RobotId { get; set; } = string.Empty;

        /// <summary>Command type: Move, Rotate, or Stop.</summary>
        [Id(2)]
        required
        public CommandType CommandType { get; set; }

        /// <summary>Axis to move on (X, Y, or Z). Only required for Move.</summary>
        [Id(3)]
        public string Axis { get; set; } = "X";

        /// <summary>Distance to move along the axis. Positive/negative for direction.</summary>
        [Id(4)]
        public int Distance { get; set; } = 0;

        /// <summary>Angle to rotate, in degrees (e.g., 90 or -90). Only for Rotate.</summary>
        [Id(5)]
        public int RotateAngle { get; set; } = 0;

        /// <summary>When the command was created.</summary>
        [Id(6)]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>When the command was created.</summary>
        [Id(7)]
        required
        public string User { get; set; } = string.Empty;
    }


}
