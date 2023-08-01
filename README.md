# TinyChessBotChallengeSubmission
My submission for Sebastian Lague's Tiny Chess Bot Challenge
https://github.com/SebLague/Chess-Challenge

Features Multiple Different Bots

### V1
- Rudimentary minimax algorithm with AB pruning with some simple board/move eval
- Brain Bot Power - ~625

### V1_1, V1_2, V1_3
- Learning how Minimax ACTUALLY works by trying (and failing) a Negamax implementation

### V1_4
- Working Negamax (flawed board evaluations - sometimes stupid trades are made when they should not have happened, ie queen for a pawn)
- Piece Square Table implementation (will need to compress into ulongs and unpack at runtime later)

### V2
- Ideally, fix board evaluation problems, and implement Quisence + move ordering
