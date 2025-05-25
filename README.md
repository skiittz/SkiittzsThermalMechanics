Try this mod out on my server!
174.175.47.104:27016
(unseenskiittz.duckdns.org:27016)
_______________________________

HIGHLY CONFIGURABLE!
A settings.xml file will be created in your save location/Storage/SkiittzThermalMechanics folder. There are all sorts of data points in there that you can tweak to your heart's desire.
_______________________________

Reactors, Batteries, H2 Engines, and H2 Thrusters will produce heat proportional to their output. Once enough heat has built up they will begin to take damage.

Damage can be delayed by building a heat sink on the grid. All excess heat will be stored in the heat sink until it is full (be careful! if a hot heat sink is destroyed, all that stored heat has to go somewhere!)

To cool your heat sink, build one or more radiators. These will ramp up over time, and at full efficiency can dissipate up 50MW of heat (5MW for small grid) per cycle. But this dissipated heat will manifest as a signal that can be seen by other players. The more heat dissipated, the further the signal can be detected.

Heavily Inspired by Kinesi's Thermal Mechanics Mod, but written from scratch with my own spin https://steamcommunity.com/sharedfiles/filedetails/?id=2496040401&searchtext=thermal+mechanics

Weather now impacts radiator efficiency as well.

Heat Ratios are available as script hooks. See the source code on github for an example script.

https://github.com/skiittz/SkiittzsThermalMechanics
https://github.com/sponsors/skiittz


a lot of people are requesting the dlc heat vents to work - they do! in the menu there are two variants of each radiator (well 4 if you count the makeshift ones, those are inefficient radiators that are cheap to make)

one uses the spotlight model. this is for people who do not own the DLC. If you own the DLC, there absolutely is a variant of the radiator that uses the Keen heat vent model.
_______________________________
NOTE:
By default, being in space makes it 10x harder to vent heat, while also producing 10x signal. This can be changed in the Settings.xml file
