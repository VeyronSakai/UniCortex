namespace UniCortex.Editor.UseCases
{
    // Position of a mouse event: Game View coordinates, or the center of a UI object.
    internal readonly struct MousePosition
    {
        public readonly float X;
        public readonly float Y;
        public readonly int? InstanceId;

        private MousePosition(float x, float y, int? instanceId)
        {
            X = x;
            Y = y;
            InstanceId = instanceId;
        }

        public static MousePosition At(float x, float y) => new(x, y, null);

        public static MousePosition CenterOf(int instanceId) => new(0f, 0f, instanceId);
    }
}
