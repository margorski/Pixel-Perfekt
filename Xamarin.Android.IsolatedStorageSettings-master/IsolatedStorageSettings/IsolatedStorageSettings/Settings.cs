
namespace CustomIsolatedStorageSettings
{
  /// <summary>
  /// Settings class for storing values in app.
  /// </summary>
  public class Settings
  {
    //Store game highscore.
    public static readonly Setting<int> HighScore = new Setting<int>("HighScore", 0);
  }
}