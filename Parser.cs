using System;

namespace StarterGame
{
    
    // Parses user input into executable commands.
    // Implements the Chain of Responsibility pattern for command parsing.
    
    public class Parser
    {
        #region Fields
        private readonly CommandWords _commands;
        private readonly CommandParser _firstParser;
        #endregion

        #region Constructors
        
        // Initializes a new instance of the Parser class with default command words.
        
        public Parser() : this(new CommandWords())
        {
        }

        
        // Initializes a new instance of the Parser class.
        
        // <param name="commandWords">The command words to use for validation.</param>
        public Parser(CommandWords commandWords)
        {
            _commands = commandWords ?? throw new ArgumentNullException(nameof(commandWords));
            _firstParser = SetupParserChain();
        }
        #endregion

        #region Public Methods
        
        // Parses a command string into a Command object.
        
        // <param name="commandString">The command string to parse.</param>
        // <returns>A Command object if the input is valid; otherwise, null.</returns>
        public Command ParseCommand(string commandString)
        {
            if (string.IsNullOrWhiteSpace(commandString))
            {
                return null;
            }

            var normalizedInput = NormalizeInput(commandString);
            return _firstParser.ParseCommand(normalizedInput);
        }

        
        // Gets a description of all available commands.
        
        // <returns>A string describing available commands.</returns>
        public string Description()
        {
            return _commands.Description();
        }
        #endregion

        #region Private Methods
        
        // Sets up the chain of responsibility for command parsing.
        
        // <returns>The first parser in the chain.</returns>
        private CommandParser SetupParserChain()
        {
            var simpleParser = new MultiCommandParser();
            var multiWordParser = new MultiWordCommandParser();

            // Set up the chain: simple commands first, then multi-word commands
            simpleParser.SetNextParser(multiWordParser);

            return simpleParser;
        }

        
        // Normalizes user input by trimming whitespace.
        
        // <param name="input">The input to normalize.</param>
        // <returns>The normalized input string.</returns>
        private string NormalizeInput(string input)
        {
            return input?.Trim() ?? string.Empty;
        }
        #endregion
    }

    
    // Base class for command parsers in the Chain of Responsibility pattern.
    
    public abstract class CommandParser
    {
        #region Fields
        protected CommandParser _nextParser;
        #endregion

        #region Public Methods
        
        // Sets the next parser in the chain.
        
        // <param name="nextParser">The next parser to use if this one cannot handle the command.</param>
        public void SetNextParser(CommandParser nextParser)
        {
            _nextParser = nextParser;
        }

        
        // Parses a command string into a Command object.
        
        // <param name="commandString">The command string to parse.</param>
        // <returns>A Command object if recognized; otherwise, null or passes to next parser.</returns>
        public abstract Command ParseCommand(string commandString);
        #endregion
    }
}
