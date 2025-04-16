

public class StateMachine
{
    public enum GameState
    {
        DISABLED,
        IDLE,
        MENU_SELECTION,
        GAME_ACTIVE
    }

    public enum GameModeState
    {
        _8BALL,
        _9BALL,
        _4BALL,
        _4BALL_JP,
        _4BALL_KR,
        _6REDS
    }

    private GameState currentState;

    public StateMachine()
    {
        currentState = GameState.IDLE;
    }

    public void setGameState(GameState newState)
    {
        currentState = newState;
    }

    public void setGameModeState(GameState newState)
    {
        currentState = newState;
    }

    // Future improvement how to use state machine
    // * should logic be handled here?
    // * define _OnStateChange in every Manager?
    // * Use this as virtual class or as sparate object?
    // * Confirm where it will be used (probably only GameManager)
    //    - if so, maybe we can use logic here with only GameManager and NetworkManager
    // * This may be too much for this project, since we don't neccessary handle fail cases
    // Maybe just have a simple state, just to verify what we have
    // Or just use enums where they are needed, there is not enough interactions
}