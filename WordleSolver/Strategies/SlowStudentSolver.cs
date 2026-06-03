
using System.Collections.Generic;

namespace WordleSolver.Strategies;

/// <summary>
/// Example solver that simply iterates through a fixed list of words.
/// Students will replace this with a smarter algorithm.
/// </summary>
public sealed class SlowStudentSolver : IWordleSolverStrategy
{
	/// <summary>Absolute or relative path of the word-list file.</summary>
	private static readonly string WordListPath = Path.Combine("data", "wordle.txt");

	/// <summary>In-memory dictionary of valid five-letter words.</summary>
	private static readonly List<string> WordList = LoadWordList();

    /// <summary>
    /// Remaining words that can be chosen
    /// </summary>
    private List<string> _remainingWords = new();
    
    // TODO: ADD your own private variables that you might need
    private readonly HashSet<string> _guessedWords = new();
    /// <summary>
    /// Loads the dictionary from disk, filtering to distinct five-letter lowercase words.
    /// </summary>
    private static List<string> LoadWordList()
    {
	    if (!File.Exists(WordListPath))
		    throw new FileNotFoundException($"Word list not found at path: {WordListPath}");

	    return File.ReadAllLines(WordListPath)
		    .Select(w => w.Trim().ToLowerInvariant())
		    .Where(w => w.Length == 5)
		    .Distinct()
		    .ToList();
    }

    /// <inheritdoc/>
    public void Reset()
    {
		// TODO: What should happen when a new game starts?
        _guessedWords.Clear();
		// If using SLOW student strategy, we just reset the current index
		// to the first word to start the next guessing sequence
        _remainingWords = [..WordList];  // Set _remainingWords to a copy of the full word list
    }

    /// <summary>
    /// Determines the next word to guess given feedback from the previous guess.
    /// </summary>
    /// <param name="previousResult">
    /// The <see cref="GuessResult"/> returned by the game engine for the last guess
    /// (or <see cref="GuessResult.Default"/> if this is the first turn).
    /// </param>
    /// <returns>A five-letter lowercase word.</returns>
    public string PickNextGuess(GuessResult previousResult)
    {
        // Analyze previousResult and remove any words from
        // _remainingWords that aren't possible

        if (!previousResult.IsValid)
            throw new InvalidOperationException("PickNextGuess shouldn't be called if previous result isn't valid");

        // Check if first guess
        if (previousResult.Guesses.Count == 0)
        {
            // TODO: Pick the best starting word from wordle.txt 
            string firstWord = WordList.Contains("slate") ? "slate" : WordList.First();

            // Save that we guessed this word.
            _guessedWords.Add(firstWord);

            // Remove it from remaining words so we do not guess it again.
            _remainingWords.Remove(firstWord);

            return firstWord;
            // BE CAREFUL that the first word you pick is in that wordle.txt list or your
            // program won't work. Regular Wordle allows users to guess any five-letter
            // word from a much larger dictionary, but we restrict it to the words that
            // can actually be chosen by WordleService to make it easier on
        }
        else
        {
            // TODO: Analyze the previousResult and reduce/filter _remainingWords based on the feedback
            // Get the most recent guess.
            var lastGuess = previousResult.Guesses.Last();

            // Get the word that was guessed.
            string guessedWord = lastGuess.Word;

            // Get the feedback for that word.
            var feedback = lastGuess.LetterStatuses;

            // Remove words that cannot be the answer.
            _remainingWords = _remainingWords
       .Where(word => !_guessedWords.Contains(word))
       .Where(word => MatchesFeedback(word, guessedWord, feedback))
       .ToList();
        }


        // Utilize the remaining words to choose the next guess
        string choice = ChooseBestRemainingWord(previousResult);
        _remainingWords.Remove(choice);

        return choice;
    }
    private static bool MatchesFeedback(string possibleAnswer, string guessedWord, IReadOnlyList<LetterStatus> realFeedback)
    {
        // Create the feedback we WOULD get if possibleAnswer was the real answer.
        List<LetterStatus> testFeedback = GetFeedback(possibleAnswer, guessedWord);

        // Compare the fake feedback to the real feedback.
        for (int i = 0; i < 5; i++)
        {
            if (testFeedback[i] != realFeedback[i])
                return false;
        }

        return true;
    }

    private static List<LetterStatus> GetFeedback(string answer, string guess)
    {
        // Start with everything gray.
        List<LetterStatus> result =
        [
            LetterStatus.Unused,
        LetterStatus.Unused,
        LetterStatus.Unused,
        LetterStatus.Unused,
        LetterStatus.Unused
        ];

        // This stores answer letters that were not already used as green.
        Dictionary<char, int> remainingLetters = new();

        // First check for green letters.
        for (int i = 0; i < 5; i++)
        {
            if (guess[i] == answer[i])
            {
                result[i] = LetterStatus.Correct;
            }
            else
            {
                char letter = answer[i];

                if (!remainingLetters.ContainsKey(letter))
                    remainingLetters[letter] = 0;

                remainingLetters[letter]++;
            }
        }

        // Then check for yellow letters.
        for (int i = 0; i < 5; i++)
        {
            // Skip green letters.
            if (result[i] == LetterStatus.Correct)
                continue;

            char letter = guess[i];

            if (remainingLetters.ContainsKey(letter) && remainingLetters[letter] > 0)
            {
                result[i] = LetterStatus.Misplaced;
                remainingLetters[letter]--;
            }
        }

        return result;
    }
    /// <summary>
    /// Pick the best of the remaining words according to some heuristic.
    /// For example, you might want to choose the word that has the most
    /// common letters found in the remaining words list
    /// </summary>
    /// <param name="previousResult"></param>
    /// <returns></returns>
    public string ChooseBestRemainingWord(GuessResult previousResult)
    {
        if (_remainingWords.Count == 0)
            throw new InvalidOperationException("No remaining words to choose from");

        // Count how often each letter appears in the remaining possible words.
        Dictionary<char, int> letterCounts = new();

        foreach (string word in _remainingWords)
        {
            // Distinct means repeated letters only count once.
            foreach (char letter in word.Distinct())
            {
                if (!letterCounts.ContainsKey(letter))
                    letterCounts[letter] = 0;

                letterCounts[letter]++;
            }
        }

        // Pick the word with the most common useful letters.
        return _remainingWords
            .OrderByDescending(word => word.Distinct().Sum(letter => letterCounts[letter]))
            .First();
    }
}