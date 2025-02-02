using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Shared.Validations;

public static class OptionsFluentValidationExtensions
{
    public static IServiceCollection AddOptionsWithFluentValidation<TOptions>(
        this IServiceCollection services,
        string section) where TOptions : class
    {
        services.AddOptions<TOptions>()
            .BindConfiguration(section)
            .ValidateFluentValidation()
            .ValidateOnStart();

        return services;
    }

    private static OptionsBuilder<TOptions> ValidateFluentValidation<TOptions>(
        this OptionsBuilder<TOptions> builder) where TOptions : class
    {
        builder.Services.AddSingleton<IValidateOptions<TOptions>>(
            sp => new FluentValidateOptions<TOptions>(sp, builder.Name));
        
        return builder;
    }
}