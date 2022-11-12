# Back (and Forth)

Back is a Forth-like toy language. This means just like [Forth](https://www.forth.com/forth/), Back is a concatenative, stackbased programming language that uses reverse polish notation.

Back is untyped, hihgly unsafe and not intended to solve real world problems but it implements enough functionality to be turing complete. Turing completeness is proven by impelementing the rule 110 cellular automaton, which itself is known to be turing complete, in Back.

Back compiles to x86-64 assembly, which again is compiled to a statically-linked linux executable. Support for other platforms is not planned.

## Credits

Thanks to [its zxc](https://github.com/singiamtel) for the awesome name.

Another big thank you goes to [tsoding](https://github.com/tsoding) the creator of [Porth](https://gitlab.com/tsoding/porth), another Forth-like programming language. This whole project is heavily inspired by Porth and without it, Back wouldn't exist.
