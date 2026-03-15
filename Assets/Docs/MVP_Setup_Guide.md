# Mini Convenience Store Simulator MVP

## Recommended Scene Setup

1. Create an empty `Bootstrap` object.
2. Add:
   - `GameManager`
   - `EconomyManager`
   - `DayCycleManager`
   - `OrderManager`
   - `CustomerManager`
3. Create a `Player` object with:
   - `CharacterController`
   - `PlayerController`
   - `PlayerInteractor`
   - Child camera assigned to `cameraRoot` and `playerCamera`
4. Place:
   - 2 to 4 `Shelf` objects
   - 1 `CheckoutCounter`
   - 1 `OrderTerminal`
   - 1 storage spawn point for `OrderManager`
   - customer spawn points and one exit point for `CustomerManager`
5. Create a `Canvas` HUD with:
   - top-left `MoneyText`
   - center `InteractionPromptText`
   - bottom-left or bottom-center `StatusText`
   - `DebugOverlayUI`

## First Playable Goal

- Press `E` on `OrderTerminal` to place a simple order
- Pick items from spawned `StorageBox`
- Fill `Shelf`
- Customer walks to shelf, takes one item, then waits at checkout
- Press `E` on checkout to finish sale
- End of day shows sales, costs, and profit

## HUD Layout

- `MoneyText`
  - role: always-visible current cash display
  - anchor: top-left
  - sample position: `X 20, Y -20`
  - suggested text: `Money: 30,000 won`
- `InteractionPromptText`
  - role: shows what the object in the center ray can do
  - anchor: center-bottom or screen center
  - sample text: `[E] Order Water x5`
- `StatusText`
  - role: short-lived debug/result feedback for actions
  - anchor: bottom-left
  - sample text: `Ordered Water x5.`

## HUD Wiring

1. Add `MoneyUI` to any HUD object and assign `MoneyText`.
2. Add `DebugOverlayUI` to any HUD object and assign:
   - `Interaction Prompt Text`
   - `Status Text`
3. Keep `MoneyText` separate from the centered prompt text.

## Suggested Next Implementation Steps

1. Replace `OrderTerminal` default ordering with a real UI.
2. Add a shelf product display and stock text.
3. Connect `CustomerManager` to available shelves automatically.
4. Add a basic interaction prompt UI.
5. Split inventory into storage UI and shelf UI.
