namespace DotAuth.UI;

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

public static class ServiceCollectionExtensions
{
    public static IMvcBuilder AddDotAuthUi(
        this IMvcBuilder mvcBuilder,
        params Type[] discoveryTypes)
    {
        return AddDotAuthUi(mvcBuilder, discoveryTypes.Select(t=>(t.Namespace, t.Assembly)).ToArray());
    }

    public static IMvcBuilder AddDotAuthUi(
        this IMvcBuilder mvcBuilder,
        params (string? defaultNamespace, Assembly assembly)[] assemblies)
    {
        return assemblies.Distinct()
            .Aggregate(
                mvcBuilder,
                (b, a) =>
                {
                    return b.AddRazorRuntimeCompilation(
                            o => o.FileProviders.Add(new EmbeddedFileProvider(a.assembly, a.defaultNamespace)))
                        .AddApplicationPart(a.assembly);
                });
    }
}
