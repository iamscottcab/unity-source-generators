using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using Scott.Cab.Tests.Common;
using System.Text;
using System.Threading.Tasks;

namespace Scott.Cab.Initialization.Generation.Tests
{
    [TestFixture]
    public class InitializationTests
    {
        [Test]
        public async Task Generator_Runs()
        {
            await Verify(string.Empty);
        }

        [Test]
        public async Task Generator_Ignores_Non_Class_Syntax()
        {
            await Verify("namespace A {}");
        }

        [Test]
        public async Task Generator_Ignores_NonInitializable_Class()
        {
            await Verify("public class A {}");
        }

        [Test]
        public async Task Generator_Ignores_Invalid_Attribute()
        {
            var code = @"using Scott.Cab.Initialization;
using System;
using System.Threading.Tasks;

namespace Scott.Cab.Initialization
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InitializableAttribute : Attribute { }

    public interface IInitializable
    {
        Task Initialize();
    }
}

namespace X.Y
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InitializableAttribute : Attribute { }
}

[X.Y.Initializable]
public class A
{
}";

            await Verify(code);
        }

        [Test]
        public async Task Generator_Accepts_Fully_Qualified_Name()
        {
            var code = @"using System;
using System.Threading.Tasks;

namespace Scott.Cab.Initialization
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InitializableAttribute : Attribute { }

    public interface IInitializable
    {
        Task Initialize();
    }
}

[Scott.Cab.Initialization.Initializable]
public partial class A
{
    protected virtual partial Task OnInitialize()
    {
        return Task.CompletedTask;
    }
}";

            var generated = @"/// <auto-generated>
/// This file is auto-generated. All changes will be discarded.
/// Source Generator written by Scott.Cab 2023.
/// See: https://github.com/iamscottcab/unity-source-generators for more info.
/// </auto-generated>

using Scott.Cab.Initialization;
using System.Threading;
using System.Threading.Tasks;

public partial class A : IInitializable
{
    private bool _initialized;
    private readonly SemaphoreSlim _semaphore = new(1);

    protected virtual partial Task OnInitialize();

    public async Task Initialize()
    {
        await _semaphore.WaitAsync();

        if (_initialized)
        {
            _semaphore.Release();
            return;
        }

        try
        {
            await InitializeDependencies();
            await OnInitialize();
            _initialized = true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected virtual async Task InitializeDependencies()
    {
        Task[] tasks = new Task[]
        {
        };

        await Task.WhenAll(tasks);
    }
}
";

            await Verify(code, (generated, "A_Initializable.g.cs"));
        }

        [Test]
        public async Task Generator_Identifies_InitializableClass_In_Global_NameSpace()
        {
            var code = @"using Scott.Cab.Initialization;
using System;
using System.Threading.Tasks;

namespace Scott.Cab.Initialization
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InitializableAttribute : Attribute { }

    public interface IInitializable
    {
        Task Initialize();
    }
}

[Initializable]
public partial class A
{
    protected virtual partial Task OnInitialize()
    {
        return Task.CompletedTask;
    }
}";

            var generated = @"/// <auto-generated>
/// This file is auto-generated. All changes will be discarded.
/// Source Generator written by Scott.Cab 2023.
/// See: https://github.com/iamscottcab/unity-source-generators for more info.
/// </auto-generated>

using Scott.Cab.Initialization;
using System.Threading;
using System.Threading.Tasks;

public partial class A : IInitializable
{
    private bool _initialized;
    private readonly SemaphoreSlim _semaphore = new(1);

    protected virtual partial Task OnInitialize();

    public async Task Initialize()
    {
        await _semaphore.WaitAsync();

        if (_initialized)
        {
            _semaphore.Release();
            return;
        }

        try
        {
            await InitializeDependencies();
            await OnInitialize();
            _initialized = true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected virtual async Task InitializeDependencies()
    {
        Task[] tasks = new Task[]
        {
        };

        await Task.WhenAll(tasks);
    }
}
";

            await Verify(code, (generated, "A_Initializable.g.cs"));
        }

        [Test]
        public async Task Generator_Identifies_InitializableClass_In_Custom_NameSpace()
        {
            var code = @"using Scott.Cab.Initialization;
using System;
using System.Threading.Tasks;

namespace Scott.Cab.Initialization
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InitializableAttribute : Attribute { }

    public interface IInitializable
    {
        Task Initialize();
    }
}

namespace X.Y
{
    [Initializable]
    public partial class A
    {
        protected virtual partial Task OnInitialize()
        {
            return Task.CompletedTask;
        }
    }
}";

            var generated = @"/// <auto-generated>
/// This file is auto-generated. All changes will be discarded.
/// Source Generator written by Scott.Cab 2023.
/// See: https://github.com/iamscottcab/unity-source-generators for more info.
/// </auto-generated>

using Scott.Cab.Initialization;
using System.Threading;
using System.Threading.Tasks;

namespace X.Y
{
    public partial class A : IInitializable
    {
        private bool _initialized;
        private readonly SemaphoreSlim _semaphore = new(1);

        protected virtual partial Task OnInitialize();

        public async Task Initialize()
        {
            await _semaphore.WaitAsync();

            if (_initialized)
            {
                _semaphore.Release();
                return;
            }

            try
            {
                await InitializeDependencies();
                await OnInitialize();
                _initialized = true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        protected virtual async Task InitializeDependencies()
        {
            Task[] tasks = new Task[]
            {
            };

            await Task.WhenAll(tasks);
        }
    }
}
";

            await Verify(code, (generated, "A_Initializable.g.cs"));
        }

        [TestCase("public")]
        [TestCase("internal")]
        public async Task Generator_Uses_Correct_Accessability_Modifier(string modifier)
        {
            var code = $@"using Scott.Cab.Initialization;
using System;
using System.Threading.Tasks;

namespace Scott.Cab.Initialization
{{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InitializableAttribute : Attribute {{ }}

    public interface IInitializable
    {{
        Task Initialize();
    }}
}}

[Initializable]
{modifier} partial class A
{{
    protected virtual partial Task OnInitialize()
    {{
        return Task.CompletedTask;
    }}
}}";

            var generated = $@"/// <auto-generated>
/// This file is auto-generated. All changes will be discarded.
/// Source Generator written by Scott.Cab 2023.
/// See: https://github.com/iamscottcab/unity-source-generators for more info.
/// </auto-generated>

using Scott.Cab.Initialization;
using System.Threading;
using System.Threading.Tasks;

{modifier} partial class A : IInitializable
{{
    private bool _initialized;
    private readonly SemaphoreSlim _semaphore = new(1);

    protected virtual partial Task OnInitialize();

    public async Task Initialize()
    {{
        await _semaphore.WaitAsync();

        if (_initialized)
        {{
            _semaphore.Release();
            return;
        }}

        try
        {{
            await InitializeDependencies();
            await OnInitialize();
            _initialized = true;
        }}
        finally
        {{
            _semaphore.Release();
        }}
    }}

    protected virtual async Task InitializeDependencies()
    {{
        Task[] tasks = new Task[]
        {{
        }};

        await Task.WhenAll(tasks);
    }}
}}
";

            await Verify(code, (generated, "A_Initializable.g.cs"));
        }

        [Test]
        public async Task Generator_Adds_Generic_Type_Argument()
        {
            var code = @"using Scott.Cab.Initialization;
using System;
using System.Threading.Tasks;

namespace Scott.Cab.Initialization
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InitializableAttribute : Attribute { }

    public interface IInitializable
    {
        Task Initialize();
    }
}

[Initializable]
public partial class A<T>
{
    protected virtual partial Task OnInitialize()
    {
        return Task.CompletedTask;
    }
}";

            var generated = @"/// <auto-generated>
/// This file is auto-generated. All changes will be discarded.
/// Source Generator written by Scott.Cab 2023.
/// See: https://github.com/iamscottcab/unity-source-generators for more info.
/// </auto-generated>

using Scott.Cab.Initialization;
using System.Threading;
using System.Threading.Tasks;

public partial class A<T> : IInitializable
{
    private bool _initialized;
    private readonly SemaphoreSlim _semaphore = new(1);

    protected virtual partial Task OnInitialize();

    public async Task Initialize()
    {
        await _semaphore.WaitAsync();

        if (_initialized)
        {
            _semaphore.Release();
            return;
        }

        try
        {
            await InitializeDependencies();
            await OnInitialize();
            _initialized = true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected virtual async Task InitializeDependencies()
    {
        Task[] tasks = new Task[]
        {
        };

        await Task.WhenAll(tasks);
    }
}
";

            await Verify(code, (generated, "A_Initializable.g.cs"));
        }

        [Test]
        public async Task Generator_Adds_Generic_Type_Arguments()
        {
            var code = @"using Scott.Cab.Initialization;
using System;
using System.Threading.Tasks;

namespace Scott.Cab.Initialization
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InitializableAttribute : Attribute { }

    public interface IInitializable
    {
        Task Initialize();
    }
}

[Initializable]
public partial class A<T, U, V>
{
    protected virtual partial Task OnInitialize()
    {
        return Task.CompletedTask;
    }
}";

            var generated = @"/// <auto-generated>
/// This file is auto-generated. All changes will be discarded.
/// Source Generator written by Scott.Cab 2023.
/// See: https://github.com/iamscottcab/unity-source-generators for more info.
/// </auto-generated>

using Scott.Cab.Initialization;
using System.Threading;
using System.Threading.Tasks;

public partial class A<T,U,V> : IInitializable
{
    private bool _initialized;
    private readonly SemaphoreSlim _semaphore = new(1);

    protected virtual partial Task OnInitialize();

    public async Task Initialize()
    {
        await _semaphore.WaitAsync();

        if (_initialized)
        {
            _semaphore.Release();
            return;
        }

        try
        {
            await InitializeDependencies();
            await OnInitialize();
            _initialized = true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected virtual async Task InitializeDependencies()
    {
        Task[] tasks = new Task[]
        {
        };

        await Task.WhenAll(tasks);
    }
}
";

            await Verify(code, (generated, "A_Initializable.g.cs"));
        }

        [Test]
        public async Task Generator_Ignores_NonInitializable_Dependency()
        {
            var code = @"using Scott.Cab.Initialization;
using System;
using System.Threading.Tasks;

namespace Scott.Cab.Initialization
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InitializableAttribute : Attribute { }

    public interface IInitializable
    {
        Task Initialize();
    }
}

[Initializable]
public partial class A
{
    B b;

    protected virtual partial Task OnInitialize()
    {
        return Task.CompletedTask;
    }
}

public class B { }";

            var generated = @"/// <auto-generated>
/// This file is auto-generated. All changes will be discarded.
/// Source Generator written by Scott.Cab 2023.
/// See: https://github.com/iamscottcab/unity-source-generators for more info.
/// </auto-generated>

using Scott.Cab.Initialization;
using System.Threading;
using System.Threading.Tasks;

public partial class A : IInitializable
{
    private bool _initialized;
    private readonly SemaphoreSlim _semaphore = new(1);

    protected virtual partial Task OnInitialize();

    public async Task Initialize()
    {
        await _semaphore.WaitAsync();

        if (_initialized)
        {
            _semaphore.Release();
            return;
        }

        try
        {
            await InitializeDependencies();
            await OnInitialize();
            _initialized = true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected virtual async Task InitializeDependencies()
    {
        Task[] tasks = new Task[]
        {
        };

        await Task.WhenAll(tasks);
    }
}
";

            await Verify(code, (generated, "A_Initializable.g.cs"));
        }

        [Test]
        public async Task Generator_Adds_Initializable_Dependency()
        {
            var code = @"using Scott.Cab.Initialization;
using System;
using System.Threading.Tasks;

namespace Scott.Cab.Initialization
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InitializableAttribute : Attribute { }

    public interface IInitializable
    {
        Task Initialize();
    }
}

[Initializable]
public partial class A
{
    B b;

    protected virtual partial Task OnInitialize()
    {
        return Task.CompletedTask;
    }
}

[Initializable]
public partial class B
{
    protected virtual partial Task OnInitialize()
    {
        return Task.CompletedTask;
    }
}";

            var generatedA = @"/// <auto-generated>
/// This file is auto-generated. All changes will be discarded.
/// Source Generator written by Scott.Cab 2023.
/// See: https://github.com/iamscottcab/unity-source-generators for more info.
/// </auto-generated>

using Scott.Cab.Initialization;
using System.Threading;
using System.Threading.Tasks;

public partial class A : IInitializable
{
    private bool _initialized;
    private readonly SemaphoreSlim _semaphore = new(1);

    protected virtual partial Task OnInitialize();

    public async Task Initialize()
    {
        await _semaphore.WaitAsync();

        if (_initialized)
        {
            _semaphore.Release();
            return;
        }

        try
        {
            await InitializeDependencies();
            await OnInitialize();
            _initialized = true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected virtual async Task InitializeDependencies()
    {
        Task[] tasks = new Task[]
        {
            b.Initialize(),
        };

        await Task.WhenAll(tasks);
    }
}
";
            var generatedB = @"/// <auto-generated>
/// This file is auto-generated. All changes will be discarded.
/// Source Generator written by Scott.Cab 2023.
/// See: https://github.com/iamscottcab/unity-source-generators for more info.
/// </auto-generated>

using Scott.Cab.Initialization;
using System.Threading;
using System.Threading.Tasks;

public partial class B : IInitializable
{
    private bool _initialized;
    private readonly SemaphoreSlim _semaphore = new(1);

    protected virtual partial Task OnInitialize();

    public async Task Initialize()
    {
        await _semaphore.WaitAsync();

        if (_initialized)
        {
            _semaphore.Release();
            return;
        }

        try
        {
            await InitializeDependencies();
            await OnInitialize();
            _initialized = true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected virtual async Task InitializeDependencies()
    {
        Task[] tasks = new Task[]
        {
        };

        await Task.WhenAll(tasks);
    }
}
";

            await Verify(code, 
                (generatedA, "A_Initializable.g.cs"),
                (generatedB, "B_Initializable.g.cs"));
        }

        [Test]
        public async Task Generator_Does_Not_Implement_Interface_For_Inheritors()
        {
            var code = @"using Scott.Cab.Initialization;
using System;
using System.Threading.Tasks;

namespace Scott.Cab.Initialization
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InitializableAttribute : Attribute { }

    public interface IInitializable
    {
        Task Initialize();
    }
}

[Initializable]
public partial class A
{
    B b;

    protected virtual partial Task OnInitialize()
    {
        return Task.CompletedTask;
    }
}


public partial class B : A
{

}";

            var generated = @"/// <auto-generated>
/// This file is auto-generated. All changes will be discarded.
/// Source Generator written by Scott.Cab 2023.
/// See: https://github.com/iamscottcab/unity-source-generators for more info.
/// </auto-generated>

using Scott.Cab.Initialization;
using System.Threading;
using System.Threading.Tasks;

public partial class A : IInitializable
{
    private bool _initialized;
    private readonly SemaphoreSlim _semaphore = new(1);

    protected virtual partial Task OnInitialize();

    public async Task Initialize()
    {
        await _semaphore.WaitAsync();

        if (_initialized)
        {
            _semaphore.Release();
            return;
        }

        try
        {
            await InitializeDependencies();
            await OnInitialize();
            _initialized = true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected virtual async Task InitializeDependencies()
    {
        Task[] tasks = new Task[]
        {
            b.Initialize(),
        };

        await Task.WhenAll(tasks);
    }
}
";
            
            await Verify(code,(generated, "A_Initializable.g.cs"));
        }

        [Test]
        public async Task Generator_Implements_Interface_For_Inheritors_When_Extra_Deps()
        {
            var code = @"using Scott.Cab.Initialization;
using System;
using System.Threading.Tasks;

namespace Scott.Cab.Initialization
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InitializableAttribute : Attribute { }

    public interface IInitializable
    {
        Task Initialize();
    }
}

[Initializable]
public partial class A
{
    protected virtual partial Task OnInitialize()
    {
        return Task.CompletedTask;
    }
}

public partial class B : A
{
    C c;
}

[Initializable]
public partial class C
{
    protected virtual partial Task OnInitialize()
    {
        return Task.CompletedTask;
    }
}";

            var generatedA = @"/// <auto-generated>
/// This file is auto-generated. All changes will be discarded.
/// Source Generator written by Scott.Cab 2023.
/// See: https://github.com/iamscottcab/unity-source-generators for more info.
/// </auto-generated>

using Scott.Cab.Initialization;
using System.Threading;
using System.Threading.Tasks;

public partial class A : IInitializable
{
    private bool _initialized;
    private readonly SemaphoreSlim _semaphore = new(1);

    protected virtual partial Task OnInitialize();

    public async Task Initialize()
    {
        await _semaphore.WaitAsync();

        if (_initialized)
        {
            _semaphore.Release();
            return;
        }

        try
        {
            await InitializeDependencies();
            await OnInitialize();
            _initialized = true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected virtual async Task InitializeDependencies()
    {
        Task[] tasks = new Task[]
        {
        };

        await Task.WhenAll(tasks);
    }
}
";
            var generatedB = @"/// <auto-generated>
/// This file is auto-generated. All changes will be discarded.
/// Source Generator written by Scott.Cab 2023.
/// See: https://github.com/iamscottcab/unity-source-generators for more info.
/// </auto-generated>

using Scott.Cab.Initialization;
using System.Threading;
using System.Threading.Tasks;

public partial class B
{
    protected override async Task InitializeDependencies()
    {
        Task[] tasks = new Task[]
        {
            c.Initialize(),
            base.InitializeDependencies()
        };

        await Task.WhenAll(tasks);
    }
}
";
            var generatedC = @"/// <auto-generated>
/// This file is auto-generated. All changes will be discarded.
/// Source Generator written by Scott.Cab 2023.
/// See: https://github.com/iamscottcab/unity-source-generators for more info.
/// </auto-generated>

using Scott.Cab.Initialization;
using System.Threading;
using System.Threading.Tasks;

public partial class C : IInitializable
{
    private bool _initialized;
    private readonly SemaphoreSlim _semaphore = new(1);

    protected virtual partial Task OnInitialize();

    public async Task Initialize()
    {
        await _semaphore.WaitAsync();

        if (_initialized)
        {
            _semaphore.Release();
            return;
        }

        try
        {
            await InitializeDependencies();
            await OnInitialize();
            _initialized = true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected virtual async Task InitializeDependencies()
    {
        Task[] tasks = new Task[]
        {
        };

        await Task.WhenAll(tasks);
    }
}
";

            await Verify(code,
                (generatedA, "A_Initializable.g.cs"),
                (generatedB, "B_Initializable.g.cs"),
                (generatedC, "C_Initializable.g.cs"));
        }

        private static async Task Verify(string code, params(string generated, string fileName)[] generatedSources)
        {
            var verifier = new SourceGeneratorVerifier<Generator>.Test();

            verifier.TestState.Sources.Add(code);

            foreach (var (generated, fileName) in generatedSources)
            {
                if (string.IsNullOrEmpty(generated) || string.IsNullOrEmpty(fileName)) continue;

                verifier.TestState.GeneratedSources.Add((typeof(Generator), fileName, SourceText.From(generated, Encoding.UTF8, SourceHashAlgorithm.Sha1)));
            }

            await verifier.RunAsync();
        }
    }
}