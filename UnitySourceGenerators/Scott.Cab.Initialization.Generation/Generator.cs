using Microsoft.CodeAnalysis;
using Scott.Cab.Generation.Common;
using System.Collections.Generic;
using System.Linq;

namespace Scott.Cab.Initialization.Generation
{
    [Generator]
    public class Generator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver) return;

            var initializableAttributeSymbol = context.Compilation.GetTypeByMetadataName("Scott.Cab.Initialization.InitializableAttribute");

            foreach (var node in receiver.Nodes)
            {
                var symbol = node.GetSymbol<INamedTypeSymbol>(context.Compilation);

                // Skip any non-initializable classes
                if (!symbol.HasAttributeRecursive(initializableAttributeSymbol, out var matchedSymbol)) continue;

                var fields = symbol.GetFields().Where(f => (f.Type as INamedTypeSymbol).HasAttributeRecursive(initializableAttributeSymbol, out _)).ToList();

                // If the matched class was the same as our starting class we know we must be the base (or we added the attribute twice, which I think is a problem for the implementer to solve for now...)
                var isBaseImplementer = SymbolEqualityComparer.Default.Equals(symbol, matchedSymbol);

                // If we are inheriting and have no newly defined fields then there is no reason for an override...
                if (!isBaseImplementer && fields.Count == 0) continue;

                var source = CreateTemplate(symbol, isBaseImplementer, fields);
                context.AddSource($"{symbol.Name}_Initializable.g.cs", source);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        /// <summary>
        /// Creates the source generated partial for the given symbol and its fields.
        /// </summary>
        private static string CreateTemplate(INamedTypeSymbol symbol, bool isBaseImplementer, IReadOnlyCollection<IFieldSymbol> fields)
        {
            TemplateBuilder builder = new();

            var hasNamespace = !symbol.ContainingNamespace.IsGlobalNamespace;

            builder.AppendLine($@"{Templates.Header}

using Scott.Cab.Initialization;
using System.Threading;
using System.Threading.Tasks;
");

            if (hasNamespace)
            {
                builder.AppendLine($@"namespace {symbol.ContainingNamespace.ToDisplayString()}");
                builder.StartBlock();
            }

            builder.Append(symbol.GetTemplateClassDefinition());
            builder.AppendLine(isBaseImplementer ? " : IInitializable" : "");

            builder.StartBlock();

            if (isBaseImplementer) CreateBaseImplementation(builder);
            if (isBaseImplementer || fields.Count > 0) CreateDependenciesImplementation(builder, isBaseImplementer, fields);

            builder.EndBlock();

            if (hasNamespace) builder.EndBlock();

            return builder.ToString();
        }

        /// <summary>
        /// Constructs the relevant fields and function for implementing the interface directly.
        /// </summary>
        private static void CreateBaseImplementation(TemplateBuilder builder)
        {
            builder.AppendLines(@"private bool _initialized;
private readonly SemaphoreSlim _semaphore = new(1);

protected virtual partial Task OnInitialize();

public async Task Initialize()");
            builder.StartBlock();
            builder.AppendLines(@"await _semaphore.WaitAsync();

if (_initialized)");

            builder.WriteBlock(@"_semaphore.Release();
return;");
            builder.AppendLines(@"
try");
            builder.WriteBlock(@"await InitializeDependencies();
await OnInitialize();
_initialized = true;");
            builder.AppendLine("finally");
            builder.WriteBlock("_semaphore.Release();");
            builder.EndBlock();
        }

        /// <summary>
        /// Creates the function responsible for initializing the classes dependencies.
        /// </summary>
        private static void CreateDependenciesImplementation(TemplateBuilder builder, bool isBaseImplementer, IEnumerable<IFieldSymbol> fields)
        {
            if (isBaseImplementer) builder.AppendLine();
            builder.AppendLine($"protected {(isBaseImplementer ? "virtual" : "override")} async Task InitializeDependencies()");
            builder.StartBlock();
            builder.AppendLine("Task[] tasks = new Task[]");
            builder.StartBlock();
            
            foreach (var field in fields)
            {
                builder.AppendLine($"{field.Name}.Initialize(),");
            }

            if (!isBaseImplementer) builder.AppendLine("base.InitializeDependencies()");

            builder.DecrementIndent();
            builder.AppendLines(@"};

await Task.WhenAll(tasks);");
            builder.EndBlock();
        }
    }
}
