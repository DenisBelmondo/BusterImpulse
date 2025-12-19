# TODO

- Viewmodel.cs isn't even needed anymore. Consolidate with either Game/Battle
  or the renderer directly.
- You need multiple TimeContexts if you want fixed timesteps (gamestate) AND
  variable timesteps (renderer)
