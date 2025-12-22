using Microsoft.Extensions.Configuration;

namespace Migration;

public static class ProgramCommandLineConfigurationExtensions
{
    extension(IConfigurationBuilder configurationBuilder)
    {
        public IConfigurationBuilder AddProgramCommandLine(string[] args)
        {
            configurationBuilder.Add(new ProgramCommandLineConfigurationSource(args));

            return configurationBuilder;
        }
    }
}
