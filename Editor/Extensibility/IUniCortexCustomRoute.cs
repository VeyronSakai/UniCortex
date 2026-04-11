namespace UniCortex.Editor.Extensibility
{
    public interface IUniCortexCustomRoute
    {
        UniCortexCustomRouteDefinition Definition { get; }
        UniCortexCustomRouteResponse Handle(UniCortexCustomRouteRequest request);
    }
}
