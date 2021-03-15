using System;

namespace REF.Caching
{
    public record Creation
    {
        public DateTime CreatedAt { get; init; }

        public Creation() => CreatedAt = DateTime.UtcNow;
    }
}
