# WordleSolver – Final Exam

## Overview

The **WordleSolver** solution (already written) hosts an API that can play up to **2,000 turns** per game and automatically runs **100 games** in succession.  Your task is to create **one C# class** that serves as the “brain” for choosing guesses as efficiently as possible.

> **You may not modify any files except your own strategy class** (and the registration line in *Program.cs* if you rename the class). 

---

## Project structure

```
WordleSolver/
│   Program.cs                  → builds the host and runs 100 games
│
├── data/
│   └── wordle.txt              → ≈ 2 300 five‑letter dictionary words (the only valid guesses)
│
├── Models/
│   ├── LetterStatus.cs         → enum  { Correct, Misplaced, Unused }
│   └── GuessResult.cs          → full feedback for a guess + game‑state snapshot
│
├── Services/
│   ├── WordleService.cs        → game engine (do **NOT** change)
│   └── StudentGuesserService.cs→ driver that runs 100 games and prints stats
│
└── Strategies/
    ├── IWordleSolverStrategy.cs → contract you must implement
    └── SlowStudentSolver.cs     → starter strategy (very slow / naïve)
```

---

## Calling <code>WordleService.Guess</code> – what to expect

When you call <code>WordleService.Guess(string guess)</code> the engine returns a **<code>GuessResult</code>** object that contains everything your solver needs to make the next decision.

1. **Validation happens first.**  If the supplied word isn’t found in <code>data/wordle.txt</code>,<br>  • <code>IsValid == false</code><br>  • <code>GuessNumber</code> and <code>GuessesRemaining</code> stay the same (the guess does **not** count).<br>  • <code>LetterStatuses</code> is all <code>Unused</code> because no comparison took place.
2. **For a valid word** the service:

   * Increments <code>GuessNumber</code> (first valid guess is 1).
   * Decrements <code>GuessesRemaining</code> (starts at 2,000, stops at 0).
   * Compares each letter to the hidden answer and fills <code>LetterStatuses</code> accordingly.
3. **End‑of‑game conditions**

   * <code>IsCorrect == true</code> when the guess matches the answer – the game ends immediately.
   * When <code>GuessesRemaining</code> reaches 0 the game also ends; subsequent calls to <code>Guess</code> just return the last <code>GuessResult</code> unchanged.

### Understanding <code>GuessResult</code>

| Property                        | Meaning                                                                                        |
| ------------------------------- | ---------------------------------------------------------------------------------------------- |
| <code>IsCorrect</code>          | <code>true</code> if the word exactly matches the secret answer.                               |
| <code>IsValid</code>            | <code>true</code> if the word exists in the dictionary; invalid words don’t use up turns.      |
| <code>GuessNumber</code>        | 1‑based index of this valid guess. 0 means no valid guesses yet.                               |
| <code>GuessesRemaining</code>   | How many valid guesses you still have (starts at 2,000).                                       |
| <code>LetterStatuses\[5]</code> | Per‑letter feedback: <code>Correct</code>, <code>Misplaced</code>, or <code>Unused</code>.     |
| <code>Guesses</code>            | A list of every **valid** <code>GuessResult</code> returned so far, including the current one. |

Use these fields to prune your candidate list and choose the next word.

---

## Assignment steps

1. **Make a copy** of *Strategies/SlowStudentSolver.cs* and rename it (e.g., *MySolver.cs*).  Do not delete the original.
2. Implement both **`void Reset()`** and **`string PickNextGuess(GuessResult previousResult)`**:

   * **`Reset()`** is called automatically at the start of every new game.  Use it to clear or re‑initialise any state your solver keeps between guesses.  The starter implementation in *SlowStudentSolver.cs* simply resets an index:

     ```csharp
     public void Reset()
     {
		// If using SLOW student strategy, we just reset the current index
		// to the first word to start the next guessing sequence
        _remainingWords = [..WordList];  // Set _remainingWords to a copy of the full word list
     }
     ```
   * **`PickNextGuess(GuessResult previousResult)`** must:
   * Chooses only words from *data/wordle.txt*.  Invalid guesses are rejected and do **not** consume turns.
   * Uses feedback (`LetterStatuses`, `IsCorrect`, etc.) to eliminate impossible words.
   * Converges on the secret word in as few guesses as possible.
3. Update *Program.cs* so the DI container registers your solver:

   ```csharp
   services.AddSingleton<IWordleSolverStrategy, MySolver>();
   ```
4. Run with `dotnet run`.  The console shows statistics such as:

   ```text
   Completed 100 game(s).
   Wins: 97/100
   Average guesses per win: 4.83
   ```

   Aim for an average below **5.5** guesses.

---

## Grading rubric (100 pts)

| Requirement                                                         | Points  |
| ------------------------------------------------------------------- | ------- |
| Average guesses **≤ 25**                                            | 30      |
| Average guesses **≤ 10**                                            | 20      |
| Average guesses **≤ 5.5**                                           | 10      |
| Average guesses **≤ 5.0**                                           | 10      |
| Code compiles **without** warnings and follows C# conventions       | 10      |
| Clear, well‑commented algorithm description at the top of your file | 10      |
| **Total**                                                           | **100** |

*Extra credit:* earn **+5 pts** for an average ≤ 4.0.

---

## Getting Started tips

* Maintain a `List<string> _remainingWords` that begins as the full dictionary and shrinks after every valid guess.
* `LetterStatus.Correct` → fix letters in position.  `Misplaced` → letter exists elsewhere.  `Unused` → letter not used AT THAT SPOT.
* Picking the candidate containing the most frequent distinct letters is a simple but effective heuristic.
* Handle duplicate letters carefully—track counts, not just presence.
* First focus on **winning consistently** (even if your first version averages well over 10 guesses); then refine for speed.

---

## Deliverables

* **One new solver class** inside *Strategies/*.  Keep *SlowStudentSolver.cs* unchanged for reference.
* **1–2 paragraphs of comments** at the top of your class explaining the algorithm.
* Commit and push to the GitHub repository **before the deadline**.

---

## Suggested timeline

| Day | In class / Homework target                                        |
| --- | ----------------------------------------------------------------- |
| 1   | Build & run scaffold, confirm statistics print, skim starter code |
| 2   | Implement candidate filtering so every guess is valid             |
| 3   | Add a scoring heuristic to choose the best candidate              |
| 4   | Refine, comment, clean code, run full tests, push final commit    |


---

## Rules & academic integrity

* **First 30 minutes:** You **may not** use any online AI tools (ChatGPT, Copilot, etc.).  Plan your approach and begin coding locally. 
* **Last 40 minutes:** You **may** consult AI tools, but you **must** attend class and be prepared to explain the code in person to receive credit.
* First 30 minutes AI generated code will result in a zero. Unexplained AI generated code or plagiarism in second half results in a zero.

Good luck – and have fun cracking Wordle efficiently!
