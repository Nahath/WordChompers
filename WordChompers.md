# Project: Word Chompers

## Overview:

This is a video game made in Unity based on the old MECC game "Word Munchers". However, the words the player should eat will be determined by a theme (such as "eat all animals"). The player will play on a 6x6 grid where each cell contains one word. The player wants to only eat the words that fit the category and no others, and the level is passed when they eat every word in that category.

While they are eating words, monsters will occasionally appear on screen. If they end up in the same cell as a monster, they lose a life (the same effect as if they eat a word that doesn't fit a category).

There will also be a mode where they only eat letters (described below).

Whenever a sound file is described, I will use brackets to represent where a variable will be used. When these sounds that contain brackets are played, multiple files will be played in sequence, one for each bracketed word and one for the fixed words inside. For example, "[chomped word] is not a [category]" will play three files. One for the word that the player chomped, one that says "is not a", and one for the category.

## Architecture:

This is a Unity-made game designed to work on Android, iOS, MacOS, Windows, and Linux. Data files (using a json format) will provide information about words, so the game will generate the contents of each level based on the data files, rather than having a separate scene for each level. The game forces landscape mode on devices where this is relevant and uses a resolution of 1280x720 (16:9 landscape). On Windows, Mac, and Linux it runs in a 1280x720 window.

This game has no persistent data. Each time it is launched, it's a fresh playthrough.

## Back End:

The game will generate each level based on data files, unless they are doing the letter-eating mode instead of word-eating, in which case the system is simpler and doesn't need them. Data files must be easy to add to. Relevant files are as follows:

### Category File:

This will have a list of categories for words. These will include "Food", "Animals", "Reptiles", and "Mammals". Categories have two fields. "Minimum Difficulty" as an integer and "Level Header" as a string. An example would be for the category "Food", it would have a minimum difficulty of 1 and a Level Header of "Eat All Words That Are Food".

### Word File:

This will contain a long list of words, and for each, it will have a difficulty rating and any number of categories. For example, the word "Chicken" will belong to the categories "Animal, Bird, Food" and have a difficulty of 3. All categories used for words must be present in the category file.

### Levels:

This will be a series of numbers (each number representing a level) with a separate field for the difficulty of that level. For example, levels 1, 2, and 3 will have a difficulty level of 1.

## Gameplay Details:

### Opening:

The opening screen will have the name of the game "Word Chompers" as a big header. Below that will be the player sprite and below that in a row, sprites for all the enemies with the enemy type listed beneath each one. Each sprite will be in a static idle pose. At the bottom, it will have two buttons: "1. Chomp Letters" and "2. Chomp Words". They can tap or click the buttons to trigger them, or they can press the corresponding number on the keyboard.

When they start, it will enter the main gameplay loop, which will be sequentially going through all the levels in the level file (or until they run out of lives). The levels are generated as explained in "Level Construction" below.

### Level Construction

The details change depending on whether the user chose "Chomp Words" or "Chomp Letters"

#### Chomp Words

First, it will look at the level file and the difficulty for that level. It will then randomly choose a category with a "Minimum Difficulty" at or below the level's difficulty.

In the top left in relatively small letters, it will say "Level #" where # is the number of the level they are on. After that, it will have the "Level Header" of the chosen category centered at the top in larger letters.

Below that, taking up most of the screen, will be the grid (at a size of 6x6). Each grid cell will have a random word from the word file at or below the difficulty of the level. Though the words are random, they are chosen with the following rules:

- Approximately one third of the words should be in the category chosen for the level. It can be up to three more or less than one third. The same word can appear multiple times.
- When randomly choosing, use weighted randoms where the weight is equal to the word's difficulty. The idea is that the lower the difficulty of a word, the less likely it is to appear in higher difficulty levels
- Words that appeared in the previous level have triple the chance of appearing in this one. This increases the liklihood of a word appearing in multiple levels in a row.
- The max length of the words is not yet determined. A word must fit within the right three fourths of the cell (as the first fourth left-to-right will be filled by part of the player's character). Scale any words down if they don't fit, but based on user testing, a maximum word lenghth might need enforced.

Below the grid will be a row of sprites for the player character for each extra life that they have.

If they are on a touchscreen without a controller, there will be a D-Pad to the left of the grid and a round red button that says "Chomp" to the right of it.

In the top right of the screen, there will be a gear icon. If tapped or clicked on, it will pause the game an open an options menu. If the user is playing with a controller, this will be opened with the button most commonly associated with opening menus or pausing (such as the start button). The menu will be an opaque square in the middle of the board with the following options: Volume (controlled by a slider), Return to Game, and Quit. Each will have the standard functionality that games would usually have with those buttons/sliders.

#### Chomp Letters

It builds a level like "Chomp Words", but instead of words, it will just have a single letter in each square. Data files are not needed for this mode. Other changes are described in the gameplay for "Chomp Letters"

### Gameplay

Gameplay is very similar between the two. "Chomp Words" will be explained first, and "Chomp Letters" will only explain what changes.

#### Chomp Words

The player starts with three extra lives (i.e. 4 lives total) and gains a new one each time he finishes five levels.

When the level launches, it will play a sound file that states the level's category header. It will replay that sound file every 10 seconds until the player has chomped two valid words. It will then play on a 30 second timer that gets reset every time they chomp a valid word.

Player starts in the top left square of the grid and can go up, down, left, or right using a standard control scheme, including pressing the appropriate direction on the touchscreen D-Pad. No movement occurs if they try to move past the edge. There is a cooldown of 200 milliseconds after each move before they can move again, and if they are holding a movement key when the cooldown ends, they'll move in that direction. If multiple directions are being held, the game will move them randomly in one of the directions being held. A simple animation will play whenever the player moves, and there will be two idle states for the player character: If there is no word in the square, he will have his mouth closed. If there is a word, his mouth will be open over the word with the word visible. Pressing the "Chomp" button (either on the touchscreen, any input button on a controller that's commonly associated with a game action, or the spacebar on a keyboard) will have him eat the word. If the word is in the level's category, it will play a munching sound, an eating animation, and the word will disappear. If the word is not in the category, it will play a sound file that says "[chomped word] is not a [category]". The game will pause while an animation plays of their character looking sick, the player loses one life, all monsters disappear, then the game continues.

Occasionally, a randomly chosen monster will enter the board from a random edge space, but they will not enter within 3 squares of the player or on a corner (i.e. since all movement is left/right/up/down, it would take a minimum of 4 moves to reach the player). The monster will act according to the "Monster Behavior" section below. The frequency of appearance will depend on the level number. When the map starts, a timer will begin, and when the timer ends, a monster appears, and the timer restarts. The amount of time on the timer (in seconds) is 20 - ([level number]/18).

If the player ends up in the same square as a monster, the monster will play an eating animation, the player will disappear, and a silly scream sound file will play. The player will lose a life, and after a two second delay, all monsters will disappear from the board, and the player will start back in the top left square. The monster spawning timer will also restart.

If two monsters end up in the same square, one will eat the other (chosen randomly). There is no death animation--the losing monster will just disappear, and the winner will play the eating animation and sound.

If the player loses all their lives, large letters in the center of the screen will say "Game Over", and below that in slightly smaller letters, it will say "You made it to level [current level]". After four seconds, words will appear at the bottom of the screen that say "Press Any button to play again". If they press a button, it will return them to the start screen.

After eating all valid words in the grid, a fanfare sound will play, and large letters will appear in the center of the screen saying "Level Complete!". After three seconds, smaller text beneath will say "Press any button to proceed" or "tap to proceed" if they are on a touchscreen. When they do so, the next level will start.

The game ends if the player clears 100 levels. In this case, there will be a fireworks animation with large words that say "Congratulations! You Are a Master Muncher!". After a five second pause, it will then have smaller text below that says "Press any key to return to the main menu", or if they are on a touchscreen, "Tap to return to main menu". Functionality to do this will also be enabled.

#### Chomp Letters

The mechanics will be the same as "Chomp Words" but with the following changes:

1. The level header will just say "Chomp Letters". It will not show the letter to be chomped visually on screen, other than where it appears in the grid.
2. The sound file that will be played on launch and on the timer will say "Chomp the letter [X]" where [X] is the actual letter to be chomped.
3. The board will only have 8 unique letters per level, and the letter to be chomped will appear between 5 and 8 times. The rest will appear randomly but no less than twice and no more than 8 times
4. There are no difficulty levels for different levels. However, monster difficulty will still increase as they get to higher levels
5. The letter to be chomped will be chosen randomly. However, letters that have already appeared have twice the liklihood of appearing again compared to those that haven't, until they have appeared 3 times. At that point, they have half the likihood of appearing, and after 5 times, they won't appear at all.
6. When an invalid letter is chomped, the audio will say "You chomped [letter that was chomped]. Only chomp [correct letter]"

#### Monster Behavior

The following types of monsters can appear. Monsters move every two seconds, but every 10th level, the time between monster moves is reduced by 20% of its current value. When a monster is at the edge of the board, the direction that would move them off is still considered valid, and if they move off, they disappear and are destroyed (no animation or sound for this). Visually, it will appear as if they are entering from outside the board, so the monster's location will start outside the board, and they will move onto it, but only the portion of the monster within the grid will be visible. Likewise, entering the board is considered his first move, so for monsters for whom the direction of their previous move is relevant, the direction can be derived from this.

Squiggler: When he enters the board, his first two moves will be toward the side opposite of where he entered. After that, every moves is random with the direction he last chose being twice as likely as any other.

Gorbler: Moves toward the side of the board opposite of where it entered. Never turns. Moves 30% faster than the other monsters

Blagwerr: Doesn't start appearing until level 15. He has a 50% chance of moving toward the player. If two directions will move equally toward the player, choose the left/right instead of up/down direction. Otherwise, he moves the same direction as last move.

Scaredy: Has a 50% chance of moving away from the player. If two directions will move equally away from the player, choose the left/right instead of up/down direction. Otherwise, he moves in the same direction as last time.

Gallumpher: Behaves like the squiggler but moves 40% faster than other monsters. Doesn't appear until level 25

Zabyss: Behaves like the Blagwerr but moves 40% faster than other monsters. Doesn't appear until level 35

### Visual Details

Each character (player and monster) will be of similar size, all able to fit fully inside of one grid cell. The animations will be idle, move, and eat. The player will have an extra idle animation, as it will be different depending on whether there is a word present in his current cell. The word in a cell will always be visible, but when it's a monster, the word will appear in front of them, while for the player, their character's mouth will be around it. The word cannot fill the leftmost 25% of the cell, as that space is reserved for part of the player.

#### Animation Timing:

Animations should take the following amount of time:
Movement: 80ms
Player Eating: 120ms
Sick: 150ms
Monster Eating: 180ms
Idle loops: 2000ms
Animations do not block gameplay. If an animation is triggered on a character before the current animation is complete, the current animation is cut off.

### Sound Details

There will be sounds for the following:
Player move
Player eating a valid word/letter
Player eating an invalid word/letter
Level Completion
Game Over
Monster eating
Game start button pressed
In addition to every event specified elsewhere that includes a sound event
