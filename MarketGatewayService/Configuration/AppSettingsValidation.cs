using Microsoft.Extensions.Options;

namespace MarketGatewayService.Configuration;

public class AppSettingsValidation : IValidateOptions<AppSettings>
{
    public ValidateOptionsResult Validate(string? name, AppSettings settings)
    {
        if (string.IsNullOrEmpty(settings.TickersStorageFilePath))
        {
            return ValidateOptionsResult.Fail("\'TickersStorageFilePath\' must be provided in configuration.");
        }

        return ValidateOptionsResult.Success;
    }
}