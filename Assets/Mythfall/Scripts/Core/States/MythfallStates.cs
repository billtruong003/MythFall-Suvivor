using BillGameCore;

namespace Mythfall.States
{
    // Sprint 1 Day 1 — empty stubs so PlayerBase + scene flow can reference these types.
    // Day 3 fills in Enter/Exit to open/close UI panels:
    //   MainMenuState.Enter → Bill.UI.Open<MainMenuPanel>()
    //   CharacterSelectState.Enter → Bill.UI.Open<CharacterSelectPanel>()
    //   InRunState.Enter → Bill.UI.Open<HudPanel>()
    //   DefeatState.Enter → Time.timeScale = 0.3f; Bill.UI.Open<GameOverPanel>()

    public class MainMenuState : GameState { }
    public class CharacterSelectState : GameState { }
    public class InRunState : GameState { }
    public class DefeatState : GameState { }
}
