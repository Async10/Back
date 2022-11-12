BOARD_CAP = 50
GENERATIONS = BOARD_CAP - 1

# bin(110) = '0b01101110'
RULE = 110

CHARS = " *"
ENDS = ["\n", ""]

def main():
    board = [0] * BOARD_CAP

    # seed board
    board[-1] = 1

    for _ in range(GENERATIONS):
        for idx, c in enumerate(board):
            print(CHARS[c], end=ENDS[1 % (BOARD_CAP - idx)])

        # get first two bits of pattern window
        pattern = board[0] << 1 | board[1]

        for idx in range(1, BOARD_CAP - 1):
            # advance pattern window
            #
            # NOTE: this works because we mutate the board outside of
            # the pattern window
            pattern = ((pattern << 1) | board[idx + 1]) & 7
            

            # calculate next state of cell by applying rule to current pattern
            board[idx] = RULE >> pattern & 1

if __name__ == "__main__":
    import sys
    sys.exit(main())
