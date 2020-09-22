# GBSharp
A Gameboy emulator written in C# using the .NET framework.

## Examples (Recorded at 30fps)
![Kirby](/Resources/Kirby.gif)

## Current Status
### 8-bit CPU (Similar to Z80)
- All [instructions](https://www.pastraiser.com/cpu/gameboy/gameboy_opcodes.html) implemented.
- Almost all interrupts implemented (some like the Link cable are missing)
- Double-speed and Single-speed mode (for CGB)
- All registers (A B C D E F H L AF BC DE HL SP PC) are implemented and fully working.

### PPU (Picture Processing Unit)
- Background and Sprite rendering is implemented.
- Scrolling is implemented.
- DMA procedure to copy data into OAM memory works.
- Renders most gameboy games without any issues.
- Missing some specific quirks such as a sprite draw limit.

### MMU (Memory Management Unit)
- All memory layout of the gameboy is completely implemented.
- Memory can remap based on the specific game cartridge.

### APU (Audio Processing Unit)
- All audio is fully implemented and working.
- Uses a dynamic audio buffer to store sound data before sending it off to be played.
- Generates audio using square waves, noise, and custom waves read in from the game.
- Has some minor issues with timing causing the music to occasionally speed up or slow down.

### Input
- Can use either a keyboard or an xbox controller.

### Cartridges
- Implements the following cartridges
  - MBC0 (Complete)
  - MBC1 (Complete)
  - MBC3 (Partial)
  - MBC5 (Partial)
- Games can also be saved using the rom files specific hash.
  - The cartridges are analyzed so that only games that can save will create save files.
  
## Platforms

### Windows
- Uses Monogame to create dynamic textures and render them using the gpu.
- Can use both keyboard and a controller through Monogame.

### Android
- Uses Monogame again to create dynamic textures and render them using the gpu.
- Uses a custom touchscreen control to interact with the games.
- Xamarin allows for the C# code to be deployed to Android.
