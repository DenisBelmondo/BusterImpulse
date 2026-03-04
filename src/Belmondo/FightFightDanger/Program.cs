using Belmondo.FightFightDanger;
using Belmondo.FightFightDanger.Raylib_cs;
using static Raylib_cs.BleedingEdge.Raylib;

bool shouldQuit = false;

RaylibAudioService raylibAudioService = new();
RaylibInputService raylibInputService = new();

TimeContext timeContext = new();

GameContext gameContext = new()
{
    AudioService = raylibAudioService,
    InputService = raylibInputService,
    TimeContext = timeContext,
};

Game game = new(gameContext);
UIState uiState = new(gameContext);

game.Battle.PlayerWon += uiState.StartBattleVictoryScreen;
game.Battle.Finished += uiState.WipeAwayBattleVictoryScreen;
game.PlayerDamaged += uiState.ShakeMugshot;
game.Quit += () => shouldQuit = true;

World world = new();

world.TileMap = new int[,]
{
    { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
    { 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1 },
    { 1, 1, 1, 0, 1, 1, 1, 1, 0, 0, 1, 0, 1, 1, 1, 0, 1, 0, 0, 1, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1 },
    { 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 1, 1, 1, 1 },
    { 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1 },
    { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1 },
    { 1, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1 },
    { 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1 },
    { 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
    { 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
    { 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1 },
    { 1, 1, 1, 0, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1 },
    { 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 1, 1, 1, 1 },
    { 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1 },
    { 1, 0, 0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1 },
    { 1, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1 },
    { 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1 },
    { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
};

world.SpawnChest(
    new()
    {
        Position = (6, 5),
    });

world.Chests[^1].Value.Inventory.Snacks[SnackType.ChickenLeg] = 1;

world.SpawnChest(
    new()
    {
        Position = (7, 5),
    });

world.Chests[^1].Value.Inventory.Snacks[SnackType.WholeChicken] = 1;

world.SpawnChest(
    new()
    {
        Position = (5, 7),
    });

world.Chests[^1].Value.Inventory.Snacks[SnackType.ChickenLeg] = 1;

world.ChestOpened += chestID =>
{
    var inventory = world.Chests[chestID].Value.Inventory;

    foreach ((var snackType, var quantity) in inventory.Snacks)
    {
        if (quantity > 0)
        {
            uiState.ShowPopUp($"{Names.Get(snackType)} x{quantity}");
        }
    }
};

world.Player.Transform.Position = (5, 5);
game.SetWorld(world);

world.OldPlayerX = world.Player.Transform.Position.X;
world.OldPlayerY = world.Player.Transform.Position.Y;

RaylibRenderer raylibRenderer = new();

raylibRenderer.Initialize(world);
RaylibResources.CacheAndInitializeAll();

game.StateAutomaton.ChangeState(Game.State.Exploring);

double oldTime = GetTime();

while (!shouldQuit)
{
    shouldQuit |= WindowShouldClose();
    raylibRenderer.CurrentScreenSize = new(GetScreenWidth(), GetScreenHeight());

    double newTime = GetTime();
    double delta = newTime - oldTime;

    oldTime = newTime;

    // semi-fixed timestep
    while (delta > 0)
    {
        double cappedDelta = Math.Min(delta, 1 / 30.0);

        timeContext.Delta = cappedDelta;

        raylibInputService.Update();
        game.Update();

        delta -= cappedDelta;
        timeContext.Time += cappedDelta;
    }

    raylibAudioService.Update();
    uiState.Update();
    raylibRenderer.Render(game, timeContext, uiState);
}

raylibRenderer.Shutdown();
