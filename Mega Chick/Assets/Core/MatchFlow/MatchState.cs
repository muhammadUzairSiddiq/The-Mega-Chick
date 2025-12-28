/// <summary>
/// Match state enum - defines all possible match states.
/// Why enum? Clear states, impossible to be in invalid state, type-safe.
/// </summary>
public enum MatchState
{
    MainMenu,      // Not in match, at main menu
    Lobby,         // In lobby, selecting characters
    Countdown,     // Pre-match countdown (3..2..1..GO)
    Playing,       // Match is active
    Results        // Match ended, showing results
}

