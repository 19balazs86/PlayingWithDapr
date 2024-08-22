using JobWorker.Miscellaneous;

namespace JobWorker.Miscellaneous;

public interface IConfigOptions
{
    static abstract string SectionName { get; }
}

// Idea: https://haacked.com/archive/2024/07/18/better-config-sections
public static class OptionsExtensions
{
    public static IHostApplicationBuilder Configure<TOptions>(this IHostApplicationBuilder builder) where TOptions : class, IConfigOptions
    {
        IConfigurationSection section = builder.Configuration.GetSection(TOptions.SectionName);

        builder.Services.Configure<TOptions>(section);

        return builder;
    }

    public static TOptions? ConfigureAndReturn<TOptions>(this IHostApplicationBuilder builder) where TOptions : class, IConfigOptions
    {
        builder.Configure<TOptions>();

        return builder.Configuration.GetConfigurationSection<TOptions>();
    }

    public static TOptions? GetConfigurationSection<TOptions>(this IConfiguration configuration) where TOptions : class, IConfigOptions
    {
        return configuration.GetSection(TOptions.SectionName).Get<TOptions>();
    }
}
