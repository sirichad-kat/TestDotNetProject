using FeedCommonLib.Application.Abstractions.Messaging;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DotnetProject.Application.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, Assembly? assembly = null)
        {
            assembly ??= Assembly.GetExecutingAssembly();
            //assembly ??= Assembly.GetCallingAssembly();
            //assembly ??= typeof(ApplicationServiceExtensions).Assembly;
 
            Console.WriteLine($"🔍 Scanning Assembly: {assembly.GetName().Name}");

            // Register Command Handlers without result (ICommandRequestHandler<>)
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes
                    .AssignableTo(typeof(ICommandRequestHandler<>))
                    .Where(type => type.Name.EndsWith("Handler")))
                .AsSelfWithInterfaces()
                .WithScopedLifetime());

            // Register Command Handlers with result (ICommandRequestHandler<,>)
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes
                    .AssignableTo(typeof(ICommandRequestHandler<,>))
                    .Where(type => type.Name.EndsWith("Handler")))
                .AsSelfWithInterfaces()
                .WithScopedLifetime());

            // Register Query Handlers (IQueryRequestHandler<,>)
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes
                    .AssignableTo(typeof(IQueryRequestHandler<,>))
                    .Where(type => type.Name.EndsWith("Handler")))
                .AsSelfWithInterfaces()
                .WithScopedLifetime());

            var commandHandlers = services.Where(s => s.ServiceType.IsGenericType &&
               (s.ServiceType.GetGenericTypeDefinition() == typeof(ICommandRequestHandler<>) ||
                s.ServiceType.GetGenericTypeDefinition() == typeof(ICommandRequestHandler<,>)))
               .ToList();

            var queryHandlers = services.Where(s => s.ServiceType.IsGenericType &&
                s.ServiceType.GetGenericTypeDefinition() == typeof(IQueryRequestHandler<,>))
                .ToList();

            Console.WriteLine($"✅ Registered {commandHandlers.Count} Command Handlers");
            foreach (var handler in commandHandlers)
            {
                Console.WriteLine($"   - {handler.ImplementationType?.Name} -> {handler.ServiceType.Name}");
            }

            Console.WriteLine($"✅ Registered {queryHandlers.Count} Query Handlers");
            foreach (var handler in queryHandlers)
            {
                Console.WriteLine($"   - {handler.ImplementationType?.Name} -> {handler.ServiceType.Name}");
            }

            // ⭐ Register all Validators from the assembly 
            services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Scoped, includeInternalTypes: true);
            var validators = services.Where(s => s.ServiceType.IsGenericType &&
               s.ServiceType.GetGenericTypeDefinition() == typeof(IValidator<>))
               .ToList();

            Console.WriteLine($"✅ Registered {validators.Count} Validators");
            foreach (var validator in validators)
            {
                Console.WriteLine($"   - {validator.ImplementationType?.Name} -> {validator.ServiceType.GetGenericArguments()[0].Name}");
            }
            


            return services;
        }
       
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, Assembly assembly)
        {
            Console.WriteLine($"🔍 Scanning Infrastructure Assembly: {assembly.GetName().Name}");

            // Register all classes that implement interfaces in the 'DotnetProject' namespace
            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes.Where(t => !t.IsAbstract)) // Filter concrete classes
                .As(type => type.GetInterfaces()
                    .Where(i => i.Namespace?.StartsWith("DotnetProject") == true)) // Match interfaces like DotnetProject.Core.Interfaces.*
                .WithScopedLifetime());

            return services;
        }
    }

}
