# TinyChessBotChallengeSubmission
My submission for Sebastian Lague's Tiny Chess Bot Challenge
Also containing several changes to the existing codebase to allow for easier bot testing/ui look and feel
https://github.com/SebLague/Chess-Challenge

## Additional Feature list
- Drop down lists for choosing bot vs bot matches (to reduce button spam and allow more customizability)
- Time control text inputs
- Set how many games to play
- Brain Capacity is appropriately shown for both bots playing each other
- added pausing and ending games prematurely (pausing might cause any bots currently in play to get a slight boost)
- QOL features like switching players sides, fast forwarding, setting the time pause between games, and more
- TODO UCI Cutechess Command Generator
- TODO Premoving
- TODO Tournament Mode
- TODO Save results of a specific bot to a file unique to it
- TODO choosing from a set of fens files to play the games in
- TODO let user type in fens to play


## Features Multiple Different Bots

### V1
- Rudimentary minimax algorithm with AB pruning with some simple board/move eval
- Brain Bot Power - ~625

### V1_1, V1_2, V1_3
- Learning how Minimax ACTUALLY works by trying (and failing) a Negamax implementation

### V1_4
- Working Negamax (flawed board evaluations - sometimes stupid trades are made when they should not have happened, ie queen for a knight) - FUTURE ME: realized this was because of a search ending in a nonquiet state (quiescence fixed this)
- Piece Square Table implementation (will need to compress into ulongs and unpack at runtime later)

### V2, V2_1, 
- implemented Transposition Tables + corrected time control in bot while mid move
  
### V2_2
- Fixed board evaluation problems, implemented Quiescence
- Issues with illegal moves

### V3
- Implemented Move Ordering
- Apparently implementing move ordering fixed illegal moves?? -NVM illegal moves happen when my bot is losing so it tries to cheat nice

### V3_1, V3_2
- Tried to update eval function and quiescence search but failed.. its worse than V3

### V3_3
- Fixed illegal moves???
- Shortened/Improved some eval functions
- Fixed Transposition Tables

### V3_4
- 
