namespace UniCortex.Editor.Domains.Models
{
    // Position of a pointer event: either Game View coordinates or the center of a UI object.
    // The constructor is private, so Coordinates and Target are the only kinds.
    public abstract class PointerPosition
    {
        private PointerPosition()
        {
        }

        // Game View coordinates (origin at the bottom-left).
        public sealed class Coordinates : PointerPosition
        {
            public float X { get; }
            public float Y { get; }

            public Coordinates(float x, float y)
            {
                X = x;
                Y = y;
            }
        }

        // The center of the UI object with the given instanceId.
        public sealed class Target : PointerPosition
        {
            public int InstanceId { get; }

            public Target(int instanceId)
            {
                InstanceId = instanceId;
            }
        }
    }
}
