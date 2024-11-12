Highlighted text indicates out-of-scope ideas


Quick References:
1 Stride "Unit" == 1 Meter == 1 Unity "Unit"



 Goals: 
As basic as is physically possible a feature complete "Game"
- Has a start/exit screen on launch
- Has a playing area
	- needs bounds and background
- Has a fail state
	- with a life counter
- Has a win state
	- with a score counter
-  Is 2d
	- sprites/pngs
- Has physics
	- Lunar lander clone
	- Player Character is a quadcopter drone with a single grenade
		- Controls should be quadcopter controls not lunar lander controls
			- Player drone should auto-hover with a PID 
			- Player drone should have two rotors, right and left
		- ==Upgrades?== 
			- ==Heavier grenades make flying harder==
			- ==Mk1 versions of new grenades are janky and difficult to utilize and have poor aerodynamics==
			- ==Mk2+ versions get aero packages that make them more reliable to drop==
		- ==Grenades influenced by simulated aerodynamics==
		- ==Simulated wind buffets player character drone and grenades dropped by drone==
		- ==Background scenery should somehow indicate the direction of the wind at any given altitude== 
			- ==flagpoles, rising smoke, vegetation, particles magically falling from space, anything== 
	- Enemies near ground attempt to throw spears at character
		- Enemies attempt to lead the target and can miss
	- Player must kill all enemies with grenades
		- Enemies attempt to run away if the drone is above them and relatively close
		- Enemies attempt to melee drone if on the same level as drone
		- Enemies should gib and ragdoll 
	- Player must return to base to rearm 
		- Base should be somewhere near the "top" of the play space
		- Players should spawn from the base