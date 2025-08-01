namespace Microsoft.Extensions.Configuration;

public static class ProgramCommandLineConfigurationExtensions
{
    public static IConfigurationBuilder AddProgramCommandLine(this IConfigurationBuilder configurationBuilder, string[] args)
    {
        configurationBuilder.Add(new ProgramCommandLineConfigurationSource(args));

        return configurationBuilder;
    }
}
