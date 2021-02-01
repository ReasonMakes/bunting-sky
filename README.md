# bunting-sky
A portfolio project where you play as the commander of a small starship in a verse artistically coloured as the title suggests

DEFAULT KEYBINDS:
(keybinds are saved in %appdata%\..\LocalLow\Reason Studios\Bunting Sky\user)

- Main menu/pause: ESC

- Move: WASD, LEFT CONTROL, and SPACE (forward thrusting is much faster than translating/strafing in any other direction)
- Orient view: mouse movement
- Free look: hold right mouse button (when moving your ship will automatically torque in the direction you are looking unless you hold right mouse button)
- Zoom in/out (and toggle first/third-person): mouse scroll wheel

- Map: M
- Set object as target: middle mouse button (will automatically match velocity)
- Select weapon 1: Z
- Select weapon 2: X
- Fire selected weapon: mouse 1
- Reload selected weapon: R

- Toggle spotlight on/off: F
- Toggle refinery on/off: T

- Take screenshot: F2
- Toggle entire HUD: F3
- Toggle FPS display: F4

- Cheat 1: I
- Cheat 2: O

CREDITS:
Rocket sound by Zovex
Modified by Reason: EQ'd and looped
https://freesound.org/people/Zovex/sounds/237974/
http://creativecommons.org/publicdomain/zero/1.0/

Cannon ball (laser) sound by OGsoundFX
Modified by Reason to start at the transient and fade out more quickly
https://freesound.org/people/OGsoundFX/sounds/423105/
https://creativecommons.org/licenses/by/3.0/

Elevator (reloading) sound by calivintage
Modified by Reason to fit the reloading time
https://freesound.org/people/calivintage/sounds/95701/
https://creativecommons.org/licenses/sampling+/1.0/

Ore collection sound is original - made by Reason using Serum in Reaper

Coins sound by DWOBoyle
https://freesound.org/people/DWOBoyle/sounds/140382/
https://creativecommons.org/licenses/by/3.0/

Rock slide (asteroid explosion) sound by Opsaaaaa
Modified by Reason: clipped to start at the transient and end as the volume dies out, deepened the pitch of the low-end, hushed the high-end
https://freesound.org/people/Opsaaaaa/sounds/335337/
https://creativecommons.org/licenses/by/3.0/

Metal storm door (ship collision) sound by volivieri
Modified by Reason: clipped to start at the second hit and end more quickly, deepened the pitch of the low-end, boosted bass, added a low-pass filter
https://freesound.org/people/volivieri/sounds/161190/
http://creativecommons.org/licenses/by/3.0/

Everything else made by Reason

TODO:
COMPLETED

EASY TO IMPLEMENT
Make map-view ship rotate
Add menu scrolling
Add setting to toggle music
Add setting to adjust in-game volume
Add keybinds menu

Add planetoid variations
Add asteroid moons to planetoids
Randomly vary overall scale of asteroids

Map screen should be less janky
- scroll wheel to zoom in and out
- centres around player without skipping forward tons
- displays background stars
- doesn't render destroyed planetoids

DIFFICULT TO IMPLEMENT
Add sound system which can handle multiple sounds being played at once(probably a dedicated object under control with an array of sound components)?
Fix ship auto-torquing?
Copy acceleration AS WELL AS velocity? (So the ship doesn't seem to slip around celestial bodies)
ADD SPACE PIRATES?? With combat music similar to Subnautica
Add hologram of target?
Line renderer contrail?
Sun disappears sometimes
Add shadows
Make it so that not all space stations have dry docks, so you can't always repair but you CAN always refuel
Add toggle button which keeps movement inputted on. Useful for long trips so you don't have to hold down W
Change drag physics into auto-thrust physics instead (instead of spooky drag, the ship automatically thrusts to match velocities as best it can. This may be problematic for planetoids since the ship currently feels no gravity)

Add warp drive:
 - for traveling in between solar systems
 - travels through to bulk, or maybe uses alcubierre drive
 - no drag, limited range (can't go past several solar systems, have to make pit stop)
 - ship deploys some antimatter thing or something in front of it which becomes a mini singularity thing from interstellar ("spherical hole"). Just completely rip off those graphics
 - Also has a long cooldown time (more than a minute) and is expensive resource-wise (need lots of water).
 - https://images.gr-assets.com/hostedimages/1437669203ra/15612130.gif
 - When not using warp drive, the default movement mode is called sublight

THIRD-PART MUSIC TO ADD?:
Caleb Etheridge - Skyboy
Jordan Critz - A Ripple in Time
Nick Box - Where Ocean Meets Sky
Chelsea McGough - Distant Water

!Add music from Sebastian Lague's videos!
Shimmer - Frontier
Shimmer - A Beautiful Dream

Antti Luode - Brief Respite
Tide Electric - When Rain Comes
Aeroplanes - Reflections of Space and Time
Bad Snacks - In the Atmosphere
MK2 - Reflections
Jesse Gallagher - Nidra in the Sky with Ayla
Antti Luode - Far Away

ADVERTISING STRAGETY:
To advertise the release, make several tutorial videos explaining how the various features work and how to implement them in your own game
Have a brief intro with sick Subnuatica-esque beats showcasing the game and the feature the tutorial will teach
At the end of the video, mention where to get the game if curious about it