using Migration;

namespace Microsoft.Extensions.Configuration;

public class ProgramCommandLineConfigurationSource(string[] args) : IConfigurationSource
{
    IConfigurationProvider IConfigurationSource.Build(IConfigurationBuilder builder)
    {
        return new ProgramCommandLineProvider(args);
    }
}
