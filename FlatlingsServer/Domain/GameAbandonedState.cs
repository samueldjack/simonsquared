namespace FlatlingsServer.Domain
{
    public class GameAbandonedState : GameState
    {
        public GameAbandonedState(Game game) : base(game)
        {
        }

        public override void Enter()
        {
            base.Enter();

            Game.IsJoinable = false;
        }
    }
}