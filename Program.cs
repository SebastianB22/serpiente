using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;

namespace SnakeGame
{
    class Program
    {
        static int width = 40;
        static int height = 20;
        static int score = 0;
        static int delay = 100;
        static bool gameOver = false;
        static bool playAgain = true;
        static Random random = new Random();
        static string playerName;
        static List<int[]> snake = new List<int[]>();
        static int[] food = new int[2];
        static List<int[]> obstacles = new List<int[]>(); // Lista de obstáculos
        static int dx = 1; // Dirección X inicial
        static int dy = 0; // Dirección Y inicial
        static List<Tuple<string, int>> highScores = new List<Tuple<string, int>>();
        static string scoresFilePath = "scores.txt";

        static void Main(string[] args)
        {
            Console.Title = "Snake Game";
            Console.CursorVisible = false;
            Console.SetWindowSize(width + 30, height + 2);
            Console.SetBufferSize(width + 30, height + 2);
            Console.BackgroundColor = ConsoleColor.White;
            Console.Clear();

            while (playAgain)
            {
                score = 0;
                snake.Clear();
                obstacles.Clear(); // Limpiar la lista de obstáculos
                DrawBorder();

                // Solicitar nombre del jugador solo al iniciar un nuevo juego
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write("Nombre del jugador: ");
                playerName = Console.ReadLine();
                Console.Clear();
                DrawBorder();
                DrawFood();
                DrawSnake();
                DrawObstacles(); // Dibujar obstáculos
                Thread inputThread = new Thread(ReadInput);
                inputThread.Start();
                gameOver = false;

                while (!gameOver)
                {
                    MoveSnake();
                    if (IsEatingFood())
                    {
                        score++;
                        DrawFood();
                    }
                    Thread.Sleep(delay);
                }

                highScores.Add(Tuple.Create(playerName, score));
                highScores.Sort((x, y) => y.Item2.CompareTo(x.Item2));

                // Guardar y actualizar puntuaciones
                SaveScores();

                // Mostrar puntuaciones al final del juego
                Console.SetCursorPosition(width + 5, 0);
                Console.WriteLine("Record:");
                int i = 1;
                foreach (var entry in highScores)
                {
                    Console.SetCursorPosition(width + 5, i);
                    Console.WriteLine($"{i}. {entry.Item1}: {entry.Item2}");
                    i++;
                }

                // Mostrar jugador ganador
                Console.SetCursorPosition(width + 5, highScores.Count + 1);
                Console.WriteLine($"Mejor record: {highScores[0].Item1}");

                // Preguntar si quiere volver a jugar
                Console.SetCursorPosition(width + 5, 0);
                Console.WriteLine("\n\nDeseas volver a jugar SI(Y) NO(N)");
                char choice = char.ToUpper(Console.ReadKey(true).KeyChar);
                if (choice != 'Y')
                    playAgain = false;

                {
                    Console.SetCursorPosition(width + 5, i);
                }
                Console.Clear();
            }
        }

        static void DrawBorder()
        {
            // Draw horizontal borders
            for (int i = 0; i < width + 2; i++)
            {
                Console.SetCursorPosition(i, 0);
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.Write(" ");
                Console.SetCursorPosition(i, height + 1);
                Console.Write(" ");
            }

            // Draw vertical borders
            for (int i = 1; i < height + 1; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(" ");
                Console.SetCursorPosition(width + 1, i);
                Console.Write(" ");
            }
            Console.BackgroundColor = ConsoleColor.Red;
        }

        static void DrawFood()
        {
            food[0] = random.Next(1, width);
            food[1] = random.Next(1, height);

            Console.SetCursorPosition(food[0], food[1]);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("*");
        }

        static void DrawSnake()
        {
            snake.Clear();
            snake.Add(new int[] { width / 2, height / 2 });
            Console.SetCursorPosition(snake[0][0], snake[0][1]);
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write("#");
        }

        static void DrawObstacles()
        {
            for (int i = 0; i < 10; i++) // Dibuja 10 obstáculos aleatorios
            {
                int[] obstacle = new int[] { random.Next(1, width), random.Next(1, height) };
                if (!snake.Contains(obstacle) && !obstacles.Contains(obstacle) && !obstacle.Equals(food)) // Asegúrate de que no estén en la misma posición que la serpiente, la comida o otros obstáculos
                {
                    obstacles.Add(obstacle);
                    Console.SetCursorPosition(obstacle[0], obstacle[1]);
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("0");
                }
            }
        }

        static void MoveSnake()
        {
            int[] newHead = { snake[0][0] + dx, snake[0][1] + dy };

            // Verificar si la nueva cabeza colisiona con el cuerpo de la serpiente
            for (int i = 1; i < snake.Count; i++)
            {
                if (newHead[0] == snake[i][0] && newHead[1] == snake[i][1])
                {
                    gameOver = true;
                    return;
                }
            }

            // Verificar si la nueva cabeza colisiona con un obstáculo
            foreach (var obstacle in obstacles)
            {
                if (newHead[0] == obstacle[0] && newHead[1] == obstacle[1])
                {
                    gameOver = true;
                    return;
                }
            }

            if (newHead[0] <= 0 || newHead[0] >= width + 1 || newHead[1] <= 0 || newHead[1] >= height + 1)
            {
                gameOver = true;
                return;
            }

            Console.SetCursorPosition(snake[snake.Count - 1][0], snake[snake.Count - 1][1]);
            Console.Write(" ");
            snake.RemoveAt(snake.Count - 1);

            snake.Insert(0, newHead);
            Console.SetCursorPosition(newHead[0], newHead[1]);
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(">");
        }

        static bool IsEatingFood()
        {
            if (snake[0][0] == food[0] && snake[0][1] == food[1])
            {
                snake.Add(new int[] { food[0], food[1] });
                return true;
            }
            return false;
        }

        static void ReadInput()
        {
            while (!gameOver)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        dx = 0;
                        dy = -1;
                        break;
                    case ConsoleKey.DownArrow:
                        dx = 0;
                        dy = 1;
                        break;
                    case ConsoleKey.LeftArrow:
                        dx = -1;
                        dy = 0;
                        break;
                    case ConsoleKey.RightArrow:
                        dx = 1;
                        dy = 0;
                        break;
                    default:
                        gameOver = true;
                        break;
                }
            }
        }

        static void SaveScores()
        {
            using (StreamWriter writer = new StreamWriter(scoresFilePath))
            {
                int highestScore = 0;
                foreach (var entry in highScores)
                {
                    writer.WriteLine($"{entry.Item1},{entry.Item2}");
                    highestScore = Math.Max(highestScore, entry.Item2);
                }

                // Eliminar registros con puntajes más bajos
                highScores.RemoveAll(entry => entry.Item2 < highestScore);
            }
        }
    }
}