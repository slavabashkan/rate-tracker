using Microsoft.Extensions.Options;

namespace PriceUpdateHandlerService.Configuration;

public class AppSettingsValidation : IValidateOptions<AppSettings>
{
    public ValidateOptionsResult Validate(string? name, AppSettings settings)
    {
        if (string.IsNullOrEmpty(settings.TickersStorageFilePath))
        {
            return ValidateOptionsResult.Fail(
                $"\'{nameof(settings.TickersStorageFilePath)}\' must be provided in configuration");
        }

        if (string.IsNullOrEmpty(settings.PublicSourceWsEndpoint))
        {
            return ValidateOptionsResult.Fail(
                $"\'{nameof(settings.PublicSourceWsEndpoint)}\' must be provided in configuration");
        }

        if (string.IsNullOrEmpty(settings.PublicSourceAPIKey))
        {
            return ValidateOptionsResult.Fail(
                $"\'{nameof(settings.PublicSourceAPIKey)}\' must be provided in configuration");
        }

        return ValidateOptionsResult.Success;
    }
}