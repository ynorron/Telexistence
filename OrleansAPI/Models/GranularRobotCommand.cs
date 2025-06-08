
using System;
using Orleans;
using Orleans.Serialization;


    namespace Telexistence.Models
    {
        /// <summary>
        /// Fine-grained streaming command for VR/continuous control.
        /// </summary>
        [GenerateSerializer]
        public class GranularRobotCommand
        {
            [Id(0)]
            public double X { get; set; }
            [Id(1)]
            public double Y { get; set; }
            [Id(2)]
            public double Z { get; set; }
            [Id(3)]
            public double Rotation { get; set; }
            /// <summary>When the command was created.</summary>
            [Id(4)]
            public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        // Extend with velocity, etc, as needed.
    }
    }




