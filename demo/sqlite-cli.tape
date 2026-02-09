# Stroke SQLite CLI — Interactive SQL REPL
# Port of Python Prompt Toolkit's tutorial/sqlite-cli.py
# Demonstrates: PromptSession, WordCompleter, PygmentsLexer, Style

Output demo/sqlite-cli.gif
Output demo/sqlite-cli.mp4

Require dotnet

Set Shell "bash"
Set FontSize 16
Set Width 1100
Set Height 700
Set Theme "Dracula"
Set TypingSpeed 75ms
Set Padding 20
Set WindowBar Colorful
Set BorderRadius 8
Set CursorBlink false

# Launch the SQLite CLI example
Hide
Type "cd /Users/brandon/src/stroke && clear"
Enter
Sleep 500ms
Show

Sleep 1s
Type@30ms "dotnet run --project examples/Stroke.Examples.Tutorial -- sqlite-cli"
Sleep 500ms
Enter
Sleep 5s

# CREATE TABLE
Sleep 1s
Type "CREATE TABLE books (id INTEGER, title TEXT, author TEXT, year INTEGER, rating REAL)"
Sleep 500ms
Enter
Sleep 2s

# INSERT rows
Sleep 500ms
Type "INSERT INTO books VALUES (1, 'The Pragmatic Programmer', 'David Thomas', 1999, 4.7)"
Sleep 500ms
Enter
Sleep 1s

Sleep 500ms
Type "INSERT INTO books VALUES (2, 'Design Patterns', 'Gang of Four', 1994, 4.2)"
Sleep 500ms
Enter
Sleep 1s

Sleep 500ms
Type "INSERT INTO books VALUES (3, 'Structure and Interpretation of Computer Programs', 'Abelson & Sussman', 1985, 4.9)"
Sleep 500ms
Enter
Sleep 1s

Sleep 500ms
Type "INSERT INTO books VALUES (4, 'The C Programming Language', 'Kernighan & Ritchie', 1978, 4.8)"
Sleep 500ms
Enter
Sleep 2s

# SELECT all — show Python tuple output format
Sleep 1s
Type "SELECT * FROM books"
Sleep 500ms
Enter
Sleep 5s

# Filtered query
Sleep 1s
Type "SELECT title, author FROM books WHERE rating > 4.5"
Sleep 500ms
Enter
Sleep 5s

# Aggregate query
Sleep 1s
Type "SELECT author, COUNT(*) FROM books GROUP BY author"
Sleep 500ms
Enter
Sleep 5s

# UPDATE
Sleep 1s
Type "UPDATE books SET rating = 5.0 WHERE title = 'The Pragmatic Programmer'"
Sleep 500ms
Enter
Sleep 1.5s

Sleep 500ms
Type "SELECT title, rating FROM books WHERE rating = 5.0"
Sleep 500ms
Enter
Sleep 4s

# DELETE
Sleep 1s
Type "DELETE FROM books WHERE year < 1990"
Sleep 500ms
Enter
Sleep 1.5s

Sleep 500ms
Type "SELECT * FROM books"
Sleep 500ms
Enter
Sleep 4s

# Tab completion demo — type "sel" then Tab
Sleep 1s
Type@150ms "sel"
Sleep 1s
Tab
Sleep 4s
Ctrl+C
Sleep 1s

# Multi-match completion — type "in" then Tab
Sleep 500ms
Type@150ms "in"
Sleep 1s
Tab
Sleep 4s
Ctrl+C
Sleep 1s

# Error handling
Sleep 1s
Type "INVALID SQL"
Sleep 500ms
Enter
Sleep 4s

# Graceful exit
Sleep 1s
Ctrl+D
Sleep 3s
