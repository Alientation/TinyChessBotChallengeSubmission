# TinyChessBotChallengeSubmission
My submission for Sebastian Lague's Tiny Chess Bot Challenge
Also containing several changes to the engine supplied to allow for easier bot testing
https://github.com/SebLague/Chess-Challenge

Features Multiple Different Bots

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
