using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SecureSightSystems.Core.Models
{
    public class ChannelMetadata
    {
        public readonly static ChannelMetadata Empty = new ChannelMetadata
        {
            ChannelId = "",
            Name = ""
        };

        public string ChannelId { get; set; }

        public Rectangle Resolution { get; set; } = new Rectangle { X = 640, Y = 480 };

        public string Name { get; set; }

        public string ServerId { get; set; }

        public bool IsDisabled { get; set; } = false;

        public bool IsSoundOn { get; set; }

        public bool IsOnAir => !IsDisabled;

        public override string ToString()
        {
            return $"Name: {Name}, Id: {ChannelId}, IsDisabled: {IsOnAir}";
        }
    }
}
