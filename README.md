# Unity Source Generators
A collection of source generators for Unity3D, that I build as I find them useful.

## Initialization Source Generator
This source generator standardizes asychronous loading of classes and their dependencies such that you don't have to worry about managing your dependencies initialization status or handling multiple calls to initialization of your classes. This allows your components to stay self contained and not require a heirarchical structure to ensure initialization order.

### ⚙️ Installation
Simply install via the Unity Package Manager with the following url **https://github.com/iamscottcab/unity-source-generators.git?path=/UnitySourceGenerators-UnityProj/Assets/Scott.Cab.Initialization#main**

Or, you can install via compiling the source as described below:

1. Clone and compile `Scott.Cab.Initialization.Generation` from source, in Release Mode.
2. Import `Scott.Cab.Initialization.dll`, `Scott.Cab.Initialization.Generation.dll` and `Scott.Cab.Generation.Common.dll` into your Unity Assets folder.
3. Set `Scott.Cab.Initialization.Generation.dll` and `Scott.Can.Generation.Common.dll` to be Roslyn Analyzers [as per the Unity documentation](https://docs.unity3d.com/Manual/roslyn-analyzers.html).

## ❓ How Does It Work

**Supports**:
- `internal` and `public` access modifiers.
- Generic Type parameters in the form of `A<T, T1 ...Tn>`
- Supports inheritance where `B : A` is considered `Initializable` and dependencies added in `B` are initialized appropriately.

**Tested On**:
- Unity 2021.3.x

**Generates**:

Given some class `B` where both `A` and `C` are also `Initializable`:
```
[Initializable]
public partial class B : MonoBehaviour
{
    public A a;
    public C c;
}
```
The following partial is created:
```
/// <auto-generated>
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
            a.Initialize(),
            c.Initialize(),
        };

        await Task.WhenAll(tasks);
    }
}
```

This implementation allows for your classes to be initialized exactly once, no matter the number of callers to `Initialize` as well as managing your `Initializable` dependencies.

For cases where you have inheritance you get the following, where classes `Z` and `X` are `Initializable` and `Y : X`.

```
/// <auto-generated>
/// This file is auto-generated. All changes will be discarded.
/// Source Generator written by Scott Cabot 2023.
/// </auto-generated>

using Scott.Cab.Initialization;
using System.Threading;
using System.Threading.Tasks;

public partial class Y
{
    protected override async Task InitializeDependencies()
    {
        Task[] tasks = new Task[]
        {
            z.Initialize(),
            base.InitializeDependencies()
        };

        await Task.WhenAll(tasks);
    }
}
```
Classes that inherit from their base but introduce no extra fields do not receive a partial as it is not required.

## ⚡️ Quick Start
1. Follow the steps in installation to get the dlls in your Unity Project.
2. Given some class _(for now we will assume a MonoBehaviour)_:
```
public class A : MonoBehaviour { }
```
3. Mark the class as `Initializable`. You may need to reference `Scott.Cab.Initialization.dll` depending on your Assembly Definition Setup.
```
[Initializable]
public class A : MonoBehaviour { }
```
4. Mark the class as `partial` and implement `protected virtual partial Task OnInitialize()`. Note that you only need to worry about `A`'s initialization and not any of it's dependencies! _(nice_)
```
[Initializable]
public partial class A : MonoBehaviour
{
    protected virtual partial async Task OnInitialize()
    {
        Debug.Log("Start Init A");
        await Task.Delay(Random.Range(0, 1000));
        Debug.Log("End Init A");
    }
}
```
5. Rinse and repeat for other classes that require initialization.
6. Find all your `Initializable` classes, via DI, direct references or some other method, and then await them all accordingly. Something like below will get you started...
```
private async void Start()
{
    var objects = GameObject.FindObjectsOfType<MonoBehaviour>(true);

    List<Task> tasks = new();

    foreach(var o in objects)
    {
        if (o is not IInitializable initializable) continue;

        tasks.Add(initializable.Initialize());
    }

    await Task.WhenAll(tasks);
}
```

## 📝Notes
- The source generator assumes your dependencies will be valid before `Initialize` is called. It purposely does not `null` check them because doing so might lead to cases where you think initialization has occured but instead it silently fails.
- The source generator only supports fields and not properties for dependencies. Primarily because the former is most commonly used for either direct reference serialization or dependency injection frameworks.

## 💖 Thanks
If you've gotten this far, or you've enjoyed this repo and want to say thanks you can do that in the following ways:
- Add a [GitHub Star](https://github.com/iamscottcab/unity-source-generators) to the project.
- Say hi on [Twitter](https://twitter.com/iamscottcab).