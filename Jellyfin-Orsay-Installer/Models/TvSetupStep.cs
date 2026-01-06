namespace Jellyfin.Orsay.Installer.Models;

public record TvSetupStep(int Number, string Title, string Description);

public enum TvSeries
{
    E,  // 2012
    F,  // 2013
    H   // 2014-2015
}
