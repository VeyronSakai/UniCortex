using System;
using UnityEngine;

namespace UniCortex.Editor.Extensibility
{
    [Serializable]
    public sealed class UniCortexEmptyArguments
    {
    }

    public abstract class UniCortexCustomToolBase<TArguments> : IUniCortexCustomTool
        where TArguments : class, new()
    {
        public abstract UniCortexCustomToolDefinition Definition { get; }

        public Type ArgumentsType => typeof(TArguments);

        public string Invoke(string argumentsJson)
        {
            var arguments = new TArguments();
            if (!string.IsNullOrWhiteSpace(argumentsJson))
            {
                JsonUtility.FromJsonOverwrite(argumentsJson, arguments);
            }

            return Execute(arguments);
        }

        protected abstract string Execute(TArguments arguments);

        protected static string ToJson<T>(T value, bool prettyPrint = false)
        {
            return JsonUtility.ToJson(value, prettyPrint);
        }
    }

    public abstract class UniCortexCustomToolBase : UniCortexCustomToolBase<UniCortexEmptyArguments>
    {
        protected sealed override string Execute(UniCortexEmptyArguments arguments)
        {
            return Execute();
        }

        protected abstract string Execute();
    }
}
