# Changelog

## [0.3.0] - 2026-03-12

### Added
- Collectible bananas scattered through the level (8 total); collecting all triggers the win condition
- Banana counter HUD label ("X / 8 bananas") displayed on-screen during play
- Win screen overlay with image and "You ate all the bananas! Time for a nap." text
- Banana collect animation: shake then shrink to zero using a tween
- Main menu scene with title and Play button
- Sound effects: grab sound on successful hand grab, eat sound on banana collection, monkey sound on win

### Changed
- Player fully disabled (Freeze + ProcessMode Disabled) on win so movement stops
- Arm edges changed from antialiased to hard-pixel (DrawPolyline antialiased=false)
- Project main scene changed from Level to MainMenu

## [0.2.0] - 2026-03-11

### Added
- Jungle background image via a CanvasLayer
- Looping jungle music (jungle.mp3) that starts on level load
- Greatly expanded level with ~28 solid platforms and 10+ ghost grab-only platforms
- Level boundaries extended to 12 000 px wide; ceiling raised to Y -2200

### Changed
- GrabHand: replaced raycast-only grab with point overlap check + reverse raycast for accurate surface contact
- GrabHand: pin joint now anchors to an invisible StaticBody2D instead of directly to the platform, preventing collision suppression between player and surface
- GrabHand: added `Press()` / `WantsGrab` state so grab intent is tracked independently from the joint
- GrabHand: cleanup now frees both the joint and the anchor node
- Camera zoom reset to 1× (was 1.5×)

## [0.1.0] - 2026-03-10

### Added
- Heave Ho-inspired swinging/grabbing mechanic with left and right hands (left click / right click)
- WASD aim + swing force, stronger when hanging from a grab
- Level with platforms, walls, and a goal platform
- Basic control instructions label above spawn point
- Monkey face character sprite with brown arms (`#a6713e`) and tan hands (`#e8ba91`)
- Hand labels ("Left Click" / "Right Click") that stay upright regardless of player rotation
