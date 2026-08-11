using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Infrastructures.CodeGraph;
using UniCortex.Core.UseCases;

namespace UniCortex.Core.Test.Fixtures;

/// <summary>
/// Creates a throwaway fake Unity project directory for code graph tests. UnityEngine stub
/// classes are provided as source files, so semantic analysis works deterministically
/// without a Unity installation or a running Editor.
/// </summary>
public sealed class CodeGraphProjectFixture : IDisposable
{
    public string ProjectPath { get; }
    public CodeGraphStore Store { get; }
    public CodeGraphUseCase UseCase { get; }

    private sealed class FixedProjectPathProvider(string projectPath) : IUnityProjectPathProvider
    {
        public string GetProjectPath() => projectPath;
    }

    public CodeGraphProjectFixture()
    {
        ProjectPath = Path.Combine(
            Path.GetTempPath(), "unicortex-codegraph-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(ProjectPath, "Assets"));
        Store = new CodeGraphStore(new FixedProjectPathProvider(ProjectPath));
        UseCase = new CodeGraphUseCase(Store);
    }

    public void WriteScript(string relativePath, string content)
    {
        var fullPath = Path.Combine(ProjectPath, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, content);
    }

    public void WriteUnityStubs()
    {
        WriteScript("Assets/Stubs/UnityStubs.cs",
            """
            namespace UnityEngine
            {
                public class Object { }
                public class Component : Object { }
                public class Behaviour : Component { }
                public class MonoBehaviour : Behaviour { }
                public class ScriptableObject : Object { }
                public class SerializeField : System.Attribute { }
                public static class Debug { public static void Log(object message) { } }
            }
            """);
    }

    public void WriteSampleGame()
    {
        WriteUnityStubs();
        WriteScript("Assets/Scripts/IWeapon.cs",
            """
            namespace Game
            {
                public interface IWeapon
                {
                    void Attack();
                }
            }
            """);
        WriteScript("Assets/Scripts/Sword.cs",
            """
            namespace Game
            {
                public class Sword : IWeapon
                {
                    public void Attack()
                    {
                        OnAttack();
                    }

                    private void OnAttack() { }
                }
            }
            """);
        WriteScript("Assets/Scripts/Player.cs",
            """
            using UnityEngine;

            namespace Game
            {
                public class Player : MonoBehaviour
                {
                    [SerializeField] private int maxHealth = 100;
                    public float speed = 1.5f;
                    private IWeapon weapon;

                    private void Awake()
                    {
                        weapon = new Sword();
                    }

                    private void Update()
                    {
                        Move();
                    }

                    private void Move()
                    {
                        weapon.Attack();
                        Debug.Log(maxHealth);
                    }
                }
            }
            """);
        WriteScript("Assets/Editor/GameBuildTools.cs",
            """
            namespace Game.EditorTools
            {
                public static class GameBuildTools
                {
                    public static void Build() { }
                }
            }
            """);
        WriteScript("Assets/Scripts/Sub/Game.Sub.asmdef", """{ "name": "Game.Sub" }""");
        WriteScript("Assets/Scripts/Sub/MathHelper.cs",
            """
            namespace Game.Sub
            {
                public class MathHelper
                {
                    public static int Add(int a, int b) => a + b;
                }
            }
            """);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(ProjectPath, recursive: true);
        }
        catch (Exception)
        {
            // Best-effort cleanup of the temp directory.
        }
    }
}
