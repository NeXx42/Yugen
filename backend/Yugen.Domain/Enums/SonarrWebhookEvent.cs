namespace Yugen.Domain.Enums;

public enum SonarrWebhookEventType
{
    Test,
    Grab,
    Download,
    Rename,
    SeriesAdd,
    SeriesDelete,
    EpisodeFileDelete,
    Health,
    ApplicationUpdate,
    HealthRestored,
    ManualInteractionRequired
}

