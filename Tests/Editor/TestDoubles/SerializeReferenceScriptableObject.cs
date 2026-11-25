using System;
using UnityEngine;

namespace UniCortex.Editor.Tests.TestDoubles
{
    // Holds [SerializeReference] values, which can refer to each other and to themselves.
    internal sealed class SerializeReferenceScriptableObject : ScriptableObject
    {
        [Serializable]
        public class Node
        {
            public int value;
            [SerializeReference] public Node next;
        }

        [SerializeReference] public Node head;
    }
}
