namespace BibleBot.Models
{
    /// <summary>
    /// An interface describing the implementation of Preference models.
    /// </summary>
    public interface IPreference
    {
        /// <summary>
        /// The internal database ID.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// The default version of the preference.
        /// </summary>
        string Version { get; set; }

        /// <summary>
        /// The default language of the preference.
        /// </summary>
        string Language { get; set; }
    }
}