# RGBFusion390SetColor

## Description

RGBFusion390SetColor is a very simple tool used to change the color, mode, speed and brightness of devices compatible with [Gigabyte's RGB Fusion] To operate, RGBFusion390SetColorload and initialize the internal components of [Gigabyte's RGB Fusion] to use them as a HAL due to that [Gigabyte's RGB Fusion] is capable of handling hardware of different types and brands. Initialization tasks may take several seconds. To prevent this delay from occurring every time a change in lighting is applied, RGBFusion390SetColorstart its internal components and then wait for commands sent by command line or by a NamedPipe.

## Modes of use

This utility can be used in two different ways:

1. Command line
2. Sending messages to a NamedPipe. This mode was developed to be used with applications such as Aurora Projectothers that require sending commands quickly (up to 15 commands per second).

## Characteristics

* Low CPU usage
* Up to 15 commands per second using NamedPipe.

## Dependencies

This application depends on LedLib2.dlland SelLEDControl.dll, both part of the main [Gigabyte's RGB Fusion] application. Both are necessary for compilation.

## Installation

This application does not need to be installed. You should only copy RGBFusion390SetColor.exeto the [Gigabyte's RGB Fusion Application] folder. Typically C:\Program Files (x86)\GIGABYTE\RGBFusion.

## Commands

Obtain the list of area identifiers.

```
--areas
```
Set areas

```
--setarea:<AREA_ID>:<MODE_ID>:<R_VALUE>:<B_VALUE>:<G_VALUE>:<SPEED_VALUE>:<BRIGHT_VALUE>
```

Where:

- <AREA_ID>: Area number. Area identifiers can be obtained with the command --areasor used -1to set all areas. If area -1 is used, the parameter `<MODE_ID>` will be ignored.
- <MODE_ID>: Numerical value that sets the mode of operation of the area. See Mode Identifiers
- <R_VALUE>: Value between 0 and 255 representing the color red.
- <G_VALUE>: Value between 0 and 255 representing the green color.
- <B_VALUE>: Value between 0 and 255 representing the color blue.
- <SPEED_VALUE>: OPTIONAL (Default is 5) Value between 0 and 9 representing the speed of the animation if an animated mode is used.
- <BRIGHT_VALUE>: OPTIONAL (Default is 9) Value between 0 and 9 representing the brightness level of the area.

```
--loadprofile: <PROFILE_ID>
```

Where:

- <PROFILE_ID>: Profile number of [Gigabyte's RGB Fusion] to be loaded.

** Mode identifiers: **

- Still = 0;
- Breath = 1;
- Beat = 2;
- MixColor = 3;
- Flash = 4;
- Random = 5;
- Wave = 6;
- Scenes = 7;
- off = 8;
- auto = 9;
- other = 10;
- DFlash = 11;

It is important to note that not all areas are compatible with all modes. You will have to try and see which modes work for each area.
