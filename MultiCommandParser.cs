using System;

namespace StarterGame
{
    
    // Parses single-word commands using the Chain of Responsibility pattern.
    
    public class MultiCommandParser : CommandParser
    {
        #region Fields
        private readonly GameSaveService _saveService;
        #endregion

        #region Constructor
        
        // Initializes a new instance of the MultiCommandParser class.
        
        public MultiCommandParser()
        {
            _saveService = new GameSaveService();
        }
        #endregion

        #region Command Parsing
        
        // Parses a single-word command string into a Command object.
        
        // <param name="commandString">The command string to parse.</param>
        // <returns>A Command object if recognized; otherwise, passes to next parser.</returns>
        public override Command ParseCommand(string commandString)
        {
            if (string.IsNullOrWhiteSpace(commandString))
            {
                return null;
            }

            var command = ParseSingleWordCommand(commandString.ToLower());
            
            return command ?? _nextParser?.ParseCommand(commandString);
        }
        #endregion

        #region Private Methods
        
        // Attempts to parse a single-word command.
        
        private Command ParseSingleWordCommand(string commandString)
        {
            return commandString switch
            {
                "go" => new GoCommand(),
                "look" => new LookCommand(),
                "back" => new BackCommand(),
                "take" => new TakeCommand(),
                "add" => new AddCommand(),
                "remove" => new RemoveCommand(),
                "shoot" => new ShootCommand(),
                "load" => new LoadCommand(_saveService),
                "save" => new SaveCommand(_saveService),
                "inspect" => new InspectCommand(),
                "quit" => new QuitCommand(),
                "say" => new SayCommand(),
                "help" => new HelpCommand(),
                _ => null
            };
        }
        #endregion
    }

    
    // Parses multi-word commands using the Chain of Responsibility pattern.
    // Handles commands with parameters and arguments.
    
    public class MultiWordCommandParser : CommandParser
    {
        #region Command Parsing
        
        // Parses a multi-word command string into a Command object.
        
        // <param name="commandString">The command string to parse.</param>
        // <returns>A Command object if recognized; otherwise, passes to next parser.</returns>
        public override Command ParseCommand(string commandString)
        {
            if (string.IsNullOrWhiteSpace(commandString))
            {
                return null;
            }

            var command = ParseMultiWordCommand(commandString);
            
            return command ?? _nextParser?.ParseCommand(commandString);
        }
        #endregion

        #region Private Methods
        
        // Attempts to parse a multi-word command with parameters.
        
        private Command ParseMultiWordCommand(string commandString)
        {
            // Go command
            if (TryParseCommand(commandString, "go", 3, out string goParam))
            {
                return new GoCommand { SecondWord = goParam };
            }

            // Say command
            if (TryParseCommand(commandString, "say", 4, out string sayParam))
            {
                return new SayCommand { SecondWord = sayParam };
            }

            // Back command (with optional parameter)
            if (TryParseCommand(commandString, "back", 5, out string backParam))
            {
                return new BackCommand { SecondWord = backParam };
            }

            // Take the [item]
            if (TryParseCommand(commandString, "take the", 9, out string takeParam))
            {
                return new TakeCommand { SecondWord = takeParam };
            }

            // Add the [item]
            if (TryParseCommand(commandString, "add the", 8, out string addParam))
            {
                return new AddCommand { SecondWord = addParam };
            }

            // Remove the [item]
            if (TryParseCommand(commandString, "remove the", 11, out string removeParam))
            {
                return new RemoveCommand { SecondWord = removeParam };
            }

            // Inspect [target]
            if (TryParseCommand(commandString, "inspect", 8, out string inspectParam))
            {
                return new InspectCommand { SecondWord = inspectParam };
            }

            // Shoot at the [target]
            if (TryParseCommand(commandString, "shoot at the", 13, out string shootParam))
            {
                return new ShootCommand { SecondWord = shootParam };
            }

            // Look at [target]
            if (TryParseCommand(commandString, "look at", 8, out string lookAtParam))
            {
                return new LookCommand { SecondWord = lookAtParam };
            }

            // Look for [target]
            if (TryParseCommand(commandString, "look for", 9, out string lookForParam))
            {
                return new LookCommand { SecondWord = lookForParam };
            }

            return null;
        }

        
        // Attempts to parse a command with a specific prefix.
        
        // <param name="input">The input string to parse.</param>
        // <param name="prefix">The command prefix to match.</param>
        // <param name="prefixLength">The length to substring after the prefix.</param>
        // <param name="parameter">The extracted parameter if successful.</param>
        // <returns>True if the command was successfully parsed; otherwise, false.</returns>
        private bool TryParseCommand(string input, string prefix, int prefixLength, out string parameter)
        {
            parameter = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            if (!input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (input.Length <= prefixLength)
            {
                // Command exists but has no parameter
                parameter = string.Empty;
                return true;
            }

            parameter = input.Substring(prefixLength).Trim();
            return true;
        }
        #endregion
    }
}
