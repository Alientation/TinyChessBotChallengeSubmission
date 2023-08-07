# TinyChessBotChallengeSubmission
My submission for Sebastian Lague's Tiny Chess Bot Challenge (https://github.com/SebLague/Chess-Challenge) <br>
Contains several changes to the supplied project to allow for easier bot testing/ui look and feel <br>
Feel free to make use of this, but just know that there's been so many changes to so many places so it might be hard to incorporate a select subset of features <br>
Also don't ask why I didn't fork from the original repository (im a github noob) <br>

# Credits
- Fast Forward (https://github.com/GheorgheMorari/Chess-Challenge)
- UCI Cutechess (https://github.com/GediminasMasaitis/Chess-Challenge-Uci/tree/uci)

<br>

# QOL Feature list
- Dropdown lists for choosing bot vs bot matches (to reduce button spam and allow more customizability)
  - *Might implement some kind of paging system to the dropdown list*  
- Time control text inputs (*this was painful*)
- Set how many games to play in a match up
- Brain Capacity is appropriately shown for both bots playing each other
- added pausing and ending games prematurely (pausing might cause any bots currently in play to get a slight boost)
- switching sides of the players, fast forwarding, setting the time pause between games, and more

<br>

# TODO
- UCI Cutechess Command Generator
- Tournament Mode
- Save results of a specific bot to a file unique to it
  - Possibly hashing the code in the bot's file and saving stats to its respective hash to accurately show changes between versions 
- Allow the user to use multiple fen files from the fens folder without having to rebuild
- Customizable fen input to play a game in

<br>

# My Progress

## V1.X
### V1
- Rudimentary minimax algorithm with AB pruning with some simple board/move eval
- Brain Bot Power - ~625

### V1_1, V1_2, V1_3
- Learning how Minimax ACTUALLY works by trying (and failing) a Negamax implementation

### V1_4
- Working Negamax (flawed board evaluations - sometimes stupid trades are made when they should not have happened, ie queen for a knight) - FUTURE ME: realized this was because of a search ending in a nonquiet state (quiescence fixed this)
- Piece Square Table implementation (will need to compress into ulongs and unpack at runtime later)

<br>

## V2.X

### V2, V2_1, 
- implemented Transposition Tables + corrected time control in bot while mid move
  
### V2_2
- Fixed board evaluation problems, implemented Quiescence
- Issues with illegal moves

<br>

## V3.X

### V3
- Implemented Move Ordering
- Apparently implementing move ordering fixed illegal moves?? -NVM illegal moves happen when my bot is losing so it tries to cheat nice

### V3_1, V3_2
- Tried to update eval function and quiescence search but failed.. its worse than V3

### V3_3
- Fixed illegal moves??? for real this time???
- Shortened/Improved some eval functions
- Fixed Transposition Tables to properly determine when to overwrite and when to use cached calculations

### V3_4
- 
