# Animation

## Overview

Complex lighting effects can be constructed using the animation engine. Definitions for animations can be supplied via the command line or animation files.

## Animation Environment

An **animation** is defined for a single lighting area and consists of one or more **animation steps**. Each animation step runs for a specified number of **clock ticks** during which it plays one or more **effects**. Each effect is defined by either a basic effect definition or by composing together one or more previously defined effects. Each animation runs until all steps are completed and then continues looping back through the animation steps until all animation is halted.

Effects defined by name are stored in the **Effect Library** until application is restarted and can be referenced by name later when composing complex effects or defining animation steps. Re-defining an effect overwrites the previous definition in the library but does not change that effect for any currently loaded animations.

Each clock tick for an animation represents approximately **10 milliseconds**, which is the shortest resolution available for any effect. This is not driven by a high-resolution timer and so timing is approximate.

All animations that are created persist for each area until they are cleared. Adding an additional animation or animation step to an area that already has an animation defined appends the contents of the new animation definition to the existing animation. To clear an animation from an area, issue the a `--setarea` command for that area. Issuing a `setarea` command with the area set to `-1` will clear all loaded animations from all areas.

## Command Line

Animations via the command line are possible, but only basic commands can be constructed. Each command issued represents a single animation step that contains a single basic effect. Multiple commands create additional steps that are appended to the existing animation.

### Command Structure

#### Animation Definition

```--animation:<AREA_ID>:<LENGTH>:<MODE>:<R_VALUE>:<G_VALUE>:<B_VALUE>:<SPEED_VALUE>:<BRIGHT_VALUE>```

The structure for the `--animation` command is similar to `--setarea` with the addition of a `<LENGTH>` value, which represents the length of time for the animation step to run in clock ticks. In addition to fixed values, each of the values for `MODE`, `R_VALUE`, `G_VALUE`, `B_VALUE`, `SPEED_VALUE`, and `BRIGHT_VALUE` can be defined using any of the built in [Animation Functions](#animation-functions).

Command line animations cannot create named effects, and they cannot reference named effect.

#### Load Animation File
`--animation-file <FILE_PATH>`

Loads an animation from the specified path. For more information on animation files, see the [Animation File](#animation-file) section.

#### Animation Control

Animations can be controlled via the control commands.

- `--animation:play` Starts any existing animation definitions. Animations that are added while the engine is playing will start playing automatically.
- `--animation:pause` Pauses any existing animations
- `--animation:stop` Stops any existing animations and resets the clock state and any animations states. This can be used to synchronize animations back to a common starting point.

## Animation File

Animations can be defined using an animation definition file, which is a JSON definition of effect, animations, or both. Multiple animation files can be loaded to create composite animations or load multiple effect sets into the effect library.

### File Structure

```json
{
	"simulateBrightness": [],
	"effects": [],
	"animations": []
}
```

Each animation file consists of a JSON document with three objects: 

- `simulateBrightness` 
- `effects`
  - Defines effects
- `animations`
  - Defines animations

Each object is optional, though at least one needs to be defined in order for the file to do anything when loaded.

### Simulated Brightness Definition

Some lighting areas do not support brightness control. In order to control brightness for those areas it must be simulated by modification of the RGB lighting values. To add brightness simulation for an area, add its `AREA_ID` value to the `simulateBrightness` array.

To determine which areas on your system need simulated brightness you will need to test each one and see if it supports modification to brightness.


### Effects Definition

Each entry in the `effects` array represents an effect definition that can be used in an animation. Defining an effect in this array does not apply it to an animation but instead makes it available in the Effect Library for use in animations.

The definition of an effect is structured as follows:
```json
{
	"name": "EffectName",
	"steps": []	
}
```

The `name` element is the name of the effect. Effect names may only contain alphanumeric characters and underscores (a-z, A-Z, 0-9, and _). Any invalid characters will be stripped from effect names when loaded. Multiple effects with the same name may not be defined within the same file.

The `steps` array defines what the effect does and how it plays out. It could be a single color change, a series of colors with associated durations, a composition of existing effects to blend them together, or a combination of all of the above.

The full structure of a step is:

```json
{
	"name":[],
	"length": 0,
	"mode": "mode definition",	
	"red": "red definition",
	"green": "green definition",
	"blue": "blue definition",
	"speed": "speed definition",
	"brightness": "brightness definition"
}
```

- `name` An array of effect names that were previously defined. If more than one effect name is provided then the step becomes a [composite effect](#composite-effect)
- `length` The relative length for the step to run. This value is only used if multiple entries are added to the `step` array, which turns the effect into a [pipeline effect](#pipeline-effect).

The remaining fields, `mode`, `red`, `green`, `blue`, `speed`, and `brightness` are fields for defining a [basic effect](#basic-effect). They can be left empty, a constant value, or an [Animation Function](#animation-functions). Any values that are empty become [pass through value](#pass-through-value).

When defining an effect step, you must choose either define a basic effect or reference existing effects. You cannot do both. If you reference any existing effects those references will be used and any basic effect settings will be ignored for that step.

When defining an effect with multiple steps, each step _must_ have a `length` value defined.

#### Effect Definition Terms

##### Pass Through Value
A pass through value is a value that is not modified when an effect is run. The previous value that was set is returned from the effect. This allows effects to be layered. You could have one effect that only modifies the RGB values, while another only modifies the brightness, creating a [composite effect](#composite-effect).

##### Basic Effect
A basic effect is an effect that only modifies core attributes of a lighting area, e.g. mode, RGB value, speed, and brightness. It does not reference existing effect definitions.

##### Composite Effect
A composite effect is an effect that blends two or more existing effects together. A composite effect runs all effects at the same time, and in the order the effects are defined for the composite effect. Using a composite effect with effects that don't contain [pass through values](#pass-through-value) may have unintended results.

##### Pipeline Effect
A pipeline effect is an effect that is made by running two or more effects in a sequence. Each effect within a pipeline effect has a **length** value that defines the _relative_ length of that effect in the pipeline. 

**Example**

Pipeline Steps | Step Lengths | Percentage of Display Time
-------------- | ------------ | --------------------
4 | 1, 1, 1, 1 | 25%, 25%, 25%, 25%
3 | 1, 2, 2 | 20%, 40%, 40%

### Animations

The entries in the `animations` array represent animations. Each animation is comprised the id of the area it is applied to and a series of steps in the animation.

```json
{
	"areaId": 5,
	"steps": []
}
```

The `steps` array contains an entry for each step:
```json
{
	"effects": [],
	"stepLength": 300,					
	"endingType": ""
}
```

The `effects` array is an array of the effects to play in the step, how long to play each effect, and in what order the effects should run. If we defined `effects` as `"effect": ["EffectA(10)", "EffectB(20)"]`, this would denote that we wanted to first run `EffectA` for 10 clock ticks and then `EffectB` for 20 clock ticks.

`stepLength` defines how many clock ticks the step should run for. An animation step can run longer or shorter than the time it takes for all effects in the step to complete. If it runs shorter, some effects may not run or may be cut short. If it runs longer the resulting behavior depends on the `endingType`. If `stepLength` is omitted then the step length defaults to the sum of the length of the effects in the step.

`endingType` can be one of three values:

- `continuous` If the effects have finished loop back to the beginning of the effects and start again as long as there is time
- `hold` If the effects have finished hold the current settings that the last effect set until the step is over
- `off` If the effects have finished turn this area off

If no ending type is specified the step defaults to `continuous`.

## Animation Functions

Each of the attributes in an effect can be set using any of the available **animation functions**. These allow the creation of more complex patterns and effects by leveraging gradients and easing functions.

### Gradient Functions

Gradient functions create procedural changes to an attribute based on one of the available gradient functions:

Function Name | Description
------------- | ------------
sin | A sin wave
cos | A cosine wave
saw | A saw wave, oscillating between -1 and 1
bsaw | A binary saw wave, oscillating between 0 and 1

Each function takes for parameters and returns a value based on the type of function and the provided parameters. It is possible to provide parameters that result in generated values that are outside the range of values that are valid for a particular attribute in an effect. It is up to you to properly choose the parameters for these functions.

The for parameters for gradient functions are:
- `center` The center point of the wave
- `width` The amplitude for the wave from the center. The maximum and minimum values generated will be `center + width` and `center - width`.
- `frequency` The frequency of the wave. Generally values of .1 or less work best.
- `phase` The phase of the wave.
 
When defining a gradient function, the following syntax is used:

`<function name>(<center>, <width>, <frequency>, <phase>)`

For example, if we wanted create an oscillating pattern that walked through the available values for a color using a sin wave, we would use the following:

`sin(128, 127, .1, 0)`

This would result in values ranging from 1 to 255 that repeat approximately every 31 clock ticks.

All gradient functions are synced to the animation clock, so using the same values in multiple functions will produce the same output. This is useful for creating synchronized patterns.

### Easing Functions

Easing functions transition a value from a starting point to a target value based on the type of easing function that is chosen. The starting value is always the value of the chosen attribute at the start of the effect using the easing function. If you are not familiar with how easing function works, try referencing [easings.net](https://easings.net/en).

Function Name | Description
------------- | -----------
linear | A linear easing
ease_in_quad | A quadratic ease in
ease_out_quad | A quadratic ease out
ease_in_out_quad | A quadratic ease in and out
ease_in_cubic | A cubic ease in
ease_out_cubic | A cubic ease out
ease_in_out_cubic | A cubic ease in and out
ease_in_quart | A quart ease in
ease_out_quart | A quart ease out
ease_in_out_quart | A quart ease in and out
ease_in_quint | A qunit ease in
ease_out_quint | A quint ease out
ease_in_out_quint | A quint east in and oud
ease_in_sine | A sin ease in
ease_out_sine | A sin ease out
ease_in_out_sine | A sin ease in and out
ease_in_expo | An exponential ease in
ease_out_expo | An exponential ease out
ease_in_out_expo | An exponential ease in and out
ease_in_circ | ?
ease_out_circ | ?
ease_in_out_circ | ?
ease_in_back | ?
ease_out_back | ?
ease_in_out_back | ?
ease_in_elastic | An elastic ease in
ease_out_elastic | An elastic ease out
ease_in_out_elastic | An elastic ease in and out
ease_in_bounce | A bouncing ease in
east_out_bounce | A bouncing ease out
ease_in_out_bounce | A bouncing ease in an dout

Each function takes a single value, the target value to ease to. When defining an easing function, use the following syntax:

`<function>(<target value>)`

For example, if we wanted to use a linear easing from the current red value to a full red (255) we would use:

`linear(255)`

This would result in a consistent transition from the previous red value to a value of 255 spread evenly over the duration of the effect.

## Example Animation File

```json
{
	"effects":[
		{
			"name": "SmoothRainbow",
			"steps": [
				{
					"mode": 0,
					"red": "sin(128, 127, .1, 0)",
					"green": "sin(128, 127, .1, 2)",
					"blue": "sin(128, 127, .1, 4)",
				}
			]
		},
		{
			"name": "OscillatingBrightness_DarkToFull",
			"steps": [
				{
				"brightness": "bsaw(-1, 9, .05, 0)"
				}
			]
		},
		{
			"name": "OscillatingRainbow",
			"steps": [
				{
					"name": ["SmoothRainbow", "OscillatingBrightness_DarkToFull"]
				}
			]
		},
		{
			"name": "BounceToPurple",
			"steps": [
				{
					"mode": 0,
					"red": "ease_in_bounce(255)",
					"green": "ease_in_bounce(0)",
					"blue": "ease_in_bounce(255)"
				}
			]
		},
		{
			"name": "RGB_Hard",
			"steps": [
				{
					"mode": 0,
					"length": 1,
					"red": 255,
					"green": 0,
					"blue": 0
				},
				{
					"mode": 0,
					"length": 1,
					"red": 0,
					"green": 255,
					"blue": 0
				},
				{
					"mode": 0,
					"length": 1,
					"red": 0,
					"green": 0,
					"blue": 255
				}
			]
		}
	],
	"animations": [
		{
			"areaId": 2,
			"steps": [
				{
					"effects": ["RGB_Hard(100)"],
					"stepLength": 150,
					"endingType": "hold"
				},
				{
					"effects": ["SmoothRainbow(100)"]					
				}
			]
		},
		{
			"areaId": 3,
			"steps": [
				{
					"effects": ["OscillatingRainbow(100)", "RGB_Hard(100)", "BounceToPurple(100)"],
					"endingType": "hold"
				},
				{
					"effects": ["OscillatingRainbow(100)"],
					"stepLength": 200,
					"endingType": "off"
				}
			]
		}
	]
}
```

## Animation FAQ

#### How come my brightness settings don't affect some areas?

Some devices don't seem to support the brightness control (some ram for instance). You can simulate brightness control by using the `simulateBrightness` option in animations or via the command line.

#### When I'm animating brightness it doesn't behave like I expect it to. What's going on?

The brightness setting doesn't instantly jump to the value you specify. Instead the system gradually fades to the brightness setting. This means you don't have control over how long it takes to get to a certain brightness level. 

To deal with this you'll want to use something like the "binary saw wave" gradient to cause the brightness to oscillate: `bsaw(-1, 9, .05, 0)` This setting is pretty close to the speed of the fade from full brightness (9) to off (-1). Using something like a sin wave will generate all of the intermediate values for brightness which will not work as expected.

An alternative approach would be to turn on simulated brightness for all areas in your animation. This puts you in direct control of the "brightness" though it won't behave the same way as the native brightness. 

Different lighting devices have different tolerances for "low" lights, which means some may be off at low RGB values while others may still be on but dim. This can create cases where your lights may not appear to be synchronized for some effects where you would expect them to be.

#### I can't use the "brightness" setting to turn off the lights!

The brightness settings for "off" is -1.