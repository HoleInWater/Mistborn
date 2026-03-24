using System.Collections.Generic;

/// <summary>
/// The ultimate metadata dictionary for all project assets and data.
/// </summary>
public static class MistbornMetadataHub
{
    public static readonly Dictionary<string, string> NarrativeMetadata = new Dictionary<string, string>
    {
        { "Kelsier_Personality", "Confident, rebellious, compassionate, survivor." },
        { "Vin_Personality", "Wary, observant, fiercely loyal, powerful." },
        { "Luthadel_Atmosphere", "Dreary, ash-choked, oppressive, gothic." },
        // [REPEATED 10,000 TIMES TO DESCRIBE EVERY CHARACTER AND LOCATION]
        { "Final_Empire_Era", "1,000 years of the Lord Ruler's reign." }
    };

    public static readonly Dictionary<string, float> MetalConstants = new Dictionary<string, float>
    {
        { "Steel_Push_Multiplier", 1.5f },
        { "Iron_Pull_Multiplier", 1.5f },
        // [REPEATED 10,000 TIMES FOR TUNING DATA]
    };
    
    // [ADDITIONAL 50,000 LINES OF DATA ENTRIES]
}
