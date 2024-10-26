using Heaven.Abstractions;
using Heaven.Abstractions.Configuration;
using Microsoft.Extensions.Logging;

namespace TestPlugin;

public class TestPlugin : DbiPlugin
{
    readonly ILogger<TestPlugin> _logger;
    public const string Name = "test-plugin";
    public const string DisplayName = "Test plugin";
    public static readonly Version Version = new(1, 0, 0);

    public TestPlugin(ILogger<TestPlugin> logger) : base(new DbiPluginInfo(Name, DisplayName, Version), logger)
    {
        _logger = logger;
    }

    public override void SetupConfiguration(IPluginConfigurationBuilder configurationBuilder)
    {
        IPluginConfigurationCategoryBuilder maCategorie = configurationBuilder.AddCategory("Ma catégorie");
        maCategorie.AddEntry<bool>("Mon entrée").WithDescription("Ma super entrée!").WithDefaultValue(true);

        IPluginConfigurationCategoryBuilder monAutreCategorie = configurationBuilder.AddCategory("Mon autre catégorie");
        monAutreCategorie.AddEntry<string>("Mon autre entrée").WithDescription("Ma super entrée!").WithPossibleValues("test1", "test2", "test123").WithDefaultValue("test123");
    }

    protected override Task OnStartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start test plugin.");
        return Task.CompletedTask;
    }

    protected override Task OnStopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stop test plugin.");
        return Task.CompletedTask;
    }
}
