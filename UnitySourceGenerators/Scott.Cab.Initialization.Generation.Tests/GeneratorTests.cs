using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System.Reflection;

namespace Scott.Cab.Initialization.Generation.Tests
{
    [TestFixture]
    public class GeneratorTests
    {
        /// <summary>
        /// Used as a convenient entrypoint for debugging the generator, this doesn't actually test anything...
        /// </summary>
        [Test]
        public void Run()
        {
            var compilation = CreateCompilation(@"
using Scott.Cab.Initialization;
using System;
using UnityEngine;
using My.Game;

namespace Scott.Cab.Initialization
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InitializableAttribute : Attribute { }
}

namespace Some.Other
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class InitializableAttribute : Attribute { }
}

namespace My.Game
{
    [Initializable]
    internal class A : MonoBehaviour
    {
        C c;
        F f;
    }

    public class B : A
    {
        D d;
    }

    public class S : B { }

    [Initializable]
    public class C : MonoBehaviour { }

    [Initializable]
    public class D : MonoBehaviour { }

    [Initializable]
    public class Foo<T> : MonoBehaviour { }

    [Initializable]
    public class Bar<T, U> : MonoBehaviour { }
}

[Scott.Cab.Initialization.Initializable]
public class E : MonoBehaviour
{
    A a;
}

public class F : MonoBehaviour { }
public class G { }

[Some.Other.Initializable]
public class Y { }
[Some.Other.Initializable]
public class Z : MonoBehaviour { }
");
            GeneratorDriver driver = CSharpGeneratorDriver.Create(new Generator());
            driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        }

        private static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }
}