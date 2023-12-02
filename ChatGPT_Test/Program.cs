using Azure.AI.OpenAI;
using System.Text.RegularExpressions;

namespace OpenAI_Chat
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Create an OpenAIClient object with the non-Azure OpenAI API key
            string nonAzureOpenAIApiKey = "your_open_ai_api_key_here";
            var client = new OpenAIClient(nonAzureOpenAIApiKey, new OpenAIClientOptions());

            // Add console input for the user to play from the console
            bool playing = true; // A flag to indicate if the game is still going on
            string[,] board = new string[3, 3]; // A 2D array to store the board state
            string userSymbol = "O"; // The symbol for the user
            string aiSymbol = "X"; // The symbol for the AI
            string winner = null; // The winner of the game, if any

            // Add console output to ask the player who goes first
            Console.WriteLine("Welcome to the TicTacToe game with the OpenAI chat bot!");
            Console.WriteLine("Do you want to go first or second? Enter 1 for first, 2 for second.");

            // Add console input to get the player's choice
            string firstChoice = Console.ReadLine();

            // Check if the player's choice is valid
            if (firstChoice != "1" && firstChoice != "2")
            {
                // Invalid choice, print an error message and end the program
                Console.WriteLine("Invalid input. Please enter 1 or 2.");
                return;
            }

            // Add console output to ask the player what symbol they want to use
            Console.WriteLine("Do you want to use O or X? Enter O for O, X for X.");

            // Add console input to get the player's symbol
            string symbolChoice = Console.ReadLine();

            // Check if the player's symbol is valid
            if (symbolChoice != "O" && symbolChoice != "X")
            {
                // Invalid symbol, print an error message and end the program
                Console.WriteLine("Invalid input. Please enter O or X.");
                return;
            }

            // Assign the symbols for the user and the AI based on the player's choice
            userSymbol = symbolChoice;
            aiSymbol = symbolChoice == "O" ? "X" : "O"; // The opposite of the user's symbol

            // Initialize the board with empty strings
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    board[i, j] = "";
                }
            }

            // Declare the chatCompletionsOptions variable
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = "gpt-3.5-turbo", // Use DeploymentName for "model" with non-Azure clients
                Messages =
        {
            new ChatMessage(ChatRole.System, "You are a helpful assistant. You will talk minimalistic."),
            new ChatMessage(ChatRole.User, "Can you help me?"),
            new ChatMessage(ChatRole.Assistant, "Yes. What can I do for you?"),
            new ChatMessage(ChatRole.User, "Can you play a game with me?"),
            new ChatMessage(ChatRole.Assistant, "Yes. What game do you want to play?"),
            new ChatMessage(ChatRole.User, "How about TicTacToe?"),
            new ChatMessage(ChatRole.Assistant, "OK. I know how to play TicTacToe."),
            new ChatMessage(ChatRole.User, $"You will play as {aiSymbol} and I will play as {userSymbol}."),
            new ChatMessage(ChatRole.User, "To make a move, you have to tell me the row and column of the cell you want to mark. For example, r1c1 means the top left cell. r3c3 means the bottom right cell. And so on."),
            new ChatMessage(ChatRole.User, "[[,,], [,,], [,,]]"),
        }
            };

            // Declare the aiMove variable
            string aiMove = "";

            // Set the initial move of the AI based on the player's choice
            if (firstChoice == "2")
            {
                // The player chose to go second, so the AI goes first and gets its move using the OpenAI chat bot
                Console.WriteLine("It's the AI's turn. Please wait...");
                aiMove = await GetAIMove(client, chatCompletionsOptions, board);

                // Parse the AI move to get the row and column numbers
                int aiRow = int.Parse(aiMove[1].ToString()) - 1; // Subtract 1 to match the array index
                int aiCol = int.Parse(aiMove[3].ToString()) - 1; // Subtract 1 to match the array index

                // Mark the AI move on the board
                board[aiRow, aiCol] = aiSymbol;
            }

            // Print the board to the console
            Console.WriteLine("This is the current board:");
            PrintBoard(board);

            // Add a message to the chat messages based on the player's choice of turn
            if (firstChoice == "1")
            {
                // The player chose to go first, so the AI waits for the player's move
                chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.Assistant, "You go first. Your turn."));
            }
            else
            {
                // The player chose to go second, so the AI makes the first move and tells the player its move
                chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.Assistant, $"I go first. I mark {aiMove} as {aiSymbol}. Your turn."));
            }

            // Get the chat completion from the OpenAI chat bot
            var response = await client.GetChatCompletionsAsync(chatCompletionsOptions);

            // Get the first choice of the response
            var choice = response.Value.Choices[0];

            // Print the content of the message to the console
            Console.WriteLine(choice.Message.Content);

            // Start the game loop
            while (playing)
            {
                // Get the user input
                Console.WriteLine("Your turn. Enter your move in the format rxcy, where x and y are the row and column numbers. For example, r1c1 means the top left cell. To restart the game, enter restart.");
                string userInput = Console.ReadLine();

                // Check if the user wants to restart the game
                if (userInput == "restart")
                {
                    // Reset the board and the winner
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            board[i, j] = "";
                        }
                    }
                    board[1, 1] = aiSymbol;
                    winner = null;

                    // Print the board and continue the loop
                    Console.WriteLine("The game has been restarted. This is the new board:");
                    PrintBoard(board);
                    continue;
                }

                // Check if the user input is valid
                if (userInput.Length != 4 || userInput[0] != 'r' || userInput[2] != 'c')
                {
                    // Invalid input, print an error message and continue the loop
                    Console.WriteLine("Invalid input. Please enter a valid move.");
                    continue;
                }

                // Parse the user input to get the row and column numbers
                int userRow = int.Parse(userInput[1].ToString()) - 1; // Subtract 1 to match the array index
                int userCol = int.Parse(userInput[3].ToString()) - 1; // Subtract 1 to match the array index

                // Check if the user input is within the board range
                if (userRow < 0 || userRow > 2 || userCol < 0 || userCol > 2)
                {
                    // Out of range, print an error message and continue the loop
                    Console.WriteLine("Invalid input. Please enter a move within the board range.");
                    continue;
                }

                // Check if the user input is an empty cell
                if (board[userRow, userCol] != "")
                {
                    // Occupied cell, print an error message and continue the loop
                    Console.WriteLine("Invalid input. Please enter a move on an empty cell.");
                    continue;
                }

                // Mark the user move on the board
                board[userRow, userCol] = userSymbol;

                // Print the board to the console
                Console.WriteLine("You marked " + userInput + " as " + userSymbol + ". This is the updated board:");
                PrintBoard(board);

                // Check if the user has won the game
                winner = CheckWinner(board, userSymbol);
                if (winner != null)
                {
                    // The user has won, print a congratulatory message and end the game loop
                    Console.WriteLine("Congratulations! You have won the game!");
                    playing = false;
                    continue;
                }

                // Check if the board is full
                if (IsBoardFull(board))
                {
                    // The board is full, print a draw message and end the game loop
                    Console.WriteLine("The game is a draw. No one has won.");
                    playing = false;
                    continue;
                }

                // Get the AI move using the OpenAI chat bot
                Console.WriteLine("It's the AI's turn. Please wait...");
                aiMove = await GetAIMove(client, chatCompletionsOptions, board);

                // Parse the AI move to get the row and column numbers
                int aiRow = int.Parse(aiMove[1].ToString()) - 1; // Subtract 1 to match the array index
                int aiCol = int.Parse(aiMove[3].ToString()) - 1; // Subtract 1 to match the array index

                // Mark the AI move on the board
                board[aiRow, aiCol] = aiSymbol;

                // Print the board to the console
                Console.WriteLine("The AI marked " + aiMove + " as " + aiSymbol + ". This is the updated board:");
                PrintBoard(board);

                // Check if the AI has won the game
                winner = CheckWinner(board, aiSymbol);
                if (winner != null)
                {
                    // The AI has won, print a sad message and end the game loop
                    Console.WriteLine("Sorry, you have lost the game. Better luck next time.");
                    playing = false;
                    continue;
                }

                // Check if the board is full
                if (IsBoardFull(board))
                {
                    // The board is full, print a draw message and end the game loop
                    Console.WriteLine("The game is a draw. No one has won.");
                    playing = false;
                    continue;
                }
            }
        }


        // A method to print the board to the console
        static void PrintBoard(string[,] board)
        {
            // Loop through the rows
            for (int i = 0; i < 3; i++)
            {
                // Print a horizontal line
                Console.WriteLine("-------");

                // Loop through the columns
                for (int j = 0; j < 3; j++)
                {
                    // Print a vertical line
                    Console.Write("|");

                    // Print the cell value
                    Console.Write(board[i, j]);
                }

                // Print a vertical line and a new line
                Console.WriteLine("|");
            }

            // Print a horizontal line
            Console.WriteLine("-------");
        }

        // A method to check if the board is full
        static bool IsBoardFull(string[,] board)
        {
            // Loop through the rows
            for (int i = 0; i < 3; i++)
            {
                // Loop through the columns
                for (int j = 0; j < 3; j++)
                {
                    // Check if the cell is empty

                    // Check if the cell is empty
                    if (board[i, j] == "")
                    {
                        // The board is not full, return false
                        return false;
                    }
                }
            }
            // The board is full, return true
            return true;
        }

        // A method to check if a symbol has won the game
        static string CheckWinner(string[,] board, string symbol)
        {
            // Check the rows for a line of three symbols
            for (int i = 0; i < 3; i++)
            {
                if (board[i, 0] == symbol && board[i, 1] == symbol && board[i, 2] == symbol)
                {
                    // The symbol has won, return the symbol
                    return symbol;
                }
            }

            // Check the columns for a line of three symbols
            for (int j = 0; j < 3; j++)
            {
                if (board[0, j] == symbol && board[1, j] == symbol && board[2, j] == symbol)
                {
                    // The symbol has won, return the symbol
                    return symbol;
                }
            }

            // Check the diagonals for a line of three symbols
            if (board[0, 0] == symbol && board[1, 1] == symbol && board[2, 2] == symbol)
            {
                // The symbol has won, return the symbol
                return symbol;
            }
            if (board[0, 2] == symbol && board[1, 1] == symbol && board[2, 0] == symbol)
            {
                // The symbol has won, return the symbol
                return symbol;
            }

            // No winner, return null
            return null;
        }

        // A method to get the AI move using the OpenAI chat bot
        static async Task<string> GetAIMove(OpenAIClient client, ChatCompletionsOptions options, string[,] board)
        {
            // Convert the board to a string representation
            string boardString = "[";
            for (int i = 0; i < 3; i++)
            {
                boardString += "[";
                for (int j = 0; j < 3; j++)
                {
                    boardString += board[i, j];
                    if (j < 2)
                    {
                        boardString += ",";
                    }
                }
                boardString += "]";
                if (i < 2)
                {
                    boardString += ",";
                }
            }
            boardString += "]";

            // Add the board string to the chat messages
            options.Messages.Add(new ChatMessage(ChatRole.User, boardString));

            // Create a Regex object with the pattern "rxcy"
            Regex regex = new Regex("r[1-3]c[1-3]");

            // Create a variable to store the content of the message
            var content = "";

            // Create a variable to store the number of attempts
            var attempts = 0;

            // Create a loop that repeats until a valid move is found or the number of attempts exceeds a limit
            while (true)
            {
                // Get the chat completion from the OpenAI chat bot
                var response = await client.GetChatCompletionsAsync(options);

                // Get the first choice of the response
                var choice = response.Value.Choices[0];

                // Get the content of the message
                content = choice.Message.Content;

                // Check if the content is in the correct format
                if (regex.IsMatch(content))
                {
                    // Extract the move from the ChatGPT answer string
                    content = regex.Match(content).Value;
                }
                else
                {
                    // The ChatGPT answer string does not contain a valid move, print a warning message and increase the number of attempts
                    Console.WriteLine("Warning: The OpenAI chat bot returned an invalid move: " + content);
                    attempts++;
                }

                // Try to parse the content to get the row and column numbers
                try
                {
                    int aiRow = int.Parse(content[1].ToString()) - 1; // Subtract 1 to match the array index
                    int aiCol = int.Parse(content[3].ToString()) - 1; // Subtract 1 to match the array index

                    // Check if the content is within the board range
                    if (aiRow < 0 || aiRow > 2 || aiCol < 0 || aiCol > 2)
                    {
                        // Out of range, print a warning message and increase the number of attempts
                        Console.WriteLine("Warning: The OpenAI chat bot returned an out of range move: " + content);
                        attempts++;
                    }
                    else
                    {
                        // Check if the content is an empty cell
                        if (board[aiRow, aiCol] != "")
                        {
                            // Occupied cell, print a warning message and increase the number of attempts
                            Console.WriteLine("Warning: The OpenAI chat bot returned an occupied cell move: " + content);
                            attempts++;
                        }
                        else
                        {
                            // Valid move, break the loop
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    // An exception occurred, print an error message and increase the number of attempts
                    Console.WriteLine("Error: An exception occurred when parsing the AI move: " + e.Message);
                    attempts++;
                }

                // Check if the number of attempts exceeds a limit
                if (attempts >= 2)
                {
                    // Too many attempts, print a message and generate a random move
                    Console.WriteLine("The OpenAI chat bot failed to return a valid move after 3 attempts.");
                    Console.WriteLine("Generating a random move for the AI...");
                    content = GetRandomMove(board);
                    break;
                }

                // Remove the last message from the chat messages
                options.Messages.RemoveAt(options.Messages.Count - 1);

                // Add a new message to the chat messages, asking the OpenAI chat bot to pick a different position
                options.Messages.Add(new ChatMessage(ChatRole.User, "That position is already occupied or invalid. Please pick a different position in the format rxcy, where x and y are the row and column numbers. For example, r1c1 means the top left cell."));
            }

            // Remove the last message from the chat messages
            options.Messages.RemoveAt(options.Messages.Count - 1);

            // Return the content as the AI move
            return content;
        }


        // A method to generate a random move for the AI
        static string GetRandomMove(string[,] board)
        {
            // Create a random number generator
            Random random = new Random();

            // Loop until a valid move is found
            while (true)
            {
                // Generate a random row and column number
                int row = random.Next(1, 4); // 1 to 3 inclusive
                int col = random.Next(1, 4); // 1 to 3 inclusive

                // Check if the cell is empty
                if (board[row - 1, col - 1] == "")
                {
                    // Return the move in the format rxcy
                    return "r" + row + "c" + col;
                }
            }
        }

    }
}