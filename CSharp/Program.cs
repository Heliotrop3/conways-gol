using System.Text;

namespace Conway
{
	internal class Program
	{
		enum CellStatus { Dead = 0, Alive = 1 };

		struct WorldInfo
		{
			public int rows;
			public int cols;
		}

		static void Main(string[] args)
		{
			
			if (args.Length != 2)
			{
				throw new ArgumentException("Exactly two arguments required");
			}

			#region Setup
			WorldInfo wi = ParseArgs(args);
			Console.CursorVisible = false;

			#region Matrix
			// Because Matrix that's why
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.Green;
			#endregion

			CellStatus[,] world = CreateWorld(wi.rows, wi.cols);
			#endregion

			PrintWorld(world);

			while (true)
			{
				UpdateWorld(ref world);
				PrintWorld(world);
				Thread.Sleep(150); // Basically the tick rate. Wouldn't reccomend going lower than 100
			}

		}
		static WorldInfo ParseArgs(string[] args)
		{
			try
			{
				WorldInfo retval = new WorldInfo();

				retval.rows = int.Parse(args[0]);
				retval.cols = int.Parse(args[1]);

				return retval;
			}
			catch
			{
				throw new ArgumentException("Error parsing arguments - expected two integers");
			}
		}

		static CellStatus[,] CreateWorld(in int rows, in int columns)
		{
			CellStatus[,] world = new CellStatus[rows,columns];
			Random rnd = new Random();

			for (int row = 0; row < rows; row++)
			{
				for (int col = 0; col < columns; col++)
				{
					world[row, col] = (CellStatus) rnd.Next(Enum.GetNames(typeof(CellStatus)).Length);
				}
			}
			return world;
		}

		static void PrintWorld(in CellStatus[,] world)
		{
			// In compiler inlining I trust
			int rows = world.GetLength(0);
			int cols = world.GetLength(1);

			// Reset cursor for seamless overwriting
			Console.SetCursorPosition(0, 0);
			Console.CursorVisible = false; // Resizing window resets to true
			StringBuilder output = new StringBuilder();

			for (int row = 0; row < rows; row++)
			{
				for (int col = 0; col < cols; col++)
				{
					// Use the integer value as the names aren't the same length
					// Leading to character artifacts when the cell's status changes
					int numberToPrint = (int)world[row, col];

					// Trailing spaces otherwise there'd be another append after these clauses
					if (numberToPrint == 0) { output.Append("  "); }
					else { output.Append("1 "); }

				}
				output.Append("\n");
			}
			Console.Write(output);
		}

		/// <summary>
		/// Determine the status of the cell based on it's current status and number of living neighbors
		/// </summary>
		/// <param name="currentStatus"></param>
		/// <param name="aliveNeighbors"></param>
		/// <returns><see cref="CellStatus"/></returns>
		static CellStatus DetermineNewStatus(CellStatus currentStatus, int aliveNeighbors)
		{
			if (
				(currentStatus == CellStatus.Dead && aliveNeighbors == 3) ||
				(currentStatus == CellStatus.Alive && (aliveNeighbors > 1 && aliveNeighbors < 4))
			   )
			{
				return CellStatus.Alive;
			}
			return CellStatus.Dead;

		}

		/// <summary>
		/// Examine the state of the world and update the cells
		/// </summary>
		/// <param name="world"></param>
		static void UpdateWorld(ref CellStatus[,] world)
		{
			// In compiler inlining I trust
			int rows = world.GetLength(0);
			int cols = world.GetLength(1);

			// Iterate over the world, apply updates to copy, overwrite orig with copy
			CellStatus[,] updatedWorld = new CellStatus[rows,cols];

			// rows - 1 to offset index
			for (int row = 0; row <= rows - 1; row++)
			{
				// cols - 1 to offset index
				for (int col = 0; col <= cols-1; col++)
				{
					CellStatus currStatus = world[row, col];
					int livelyNeighbors = 0;

					// There might be some logic optimization below. Opting not to section the below logic
					// into it's own function. I'd either need to pass the whole world or slice off
					// a 3 x 3 section surrounding the cell being examined. Passing in the 3 x 3 slice
					// requires checking to ensure indices aren't out of bounds. At that point why not check
					// the contents instead? Passing the world would work - function would have a signature
					// like 'CheckCell(in CellStatus[,] world, int row, int col)'

					// Check above
					if (row > 0)
					{
						// Check directly above
						if (world[row-1, col] == CellStatus.Alive) { livelyNeighbors++; }

						// Check above left
						if (col > 0)
						{
							if (world[row-1, col-1] == CellStatus.Alive) { livelyNeighbors++; }
						}

						// Check above right
						if (col < cols - 1)
						{
							if (world[row-1, col+1] == CellStatus.Alive) { livelyNeighbors++; }
						}
					}

					// Check below
					if (row < rows - 1)
					{
						// Check directly below
						if (world[row+1, col] == CellStatus.Alive) { livelyNeighbors++; }

						// Check below left
						if (col > 0)
						{
							if (world[row+1, col-1] == CellStatus.Alive) { livelyNeighbors++; }
						}

						// Check below right
						if (col < cols - 1)
						{
							if (world[row+1, col+1] == CellStatus.Alive) { livelyNeighbors++; }
						}
					}

					// Check left
					if (col > 0)
					{
						if (world[row, col-1] == CellStatus.Alive) { livelyNeighbors++; }
					}

					// Check right
					if (col < cols - 1)
					{
						if (world[row, col+1] == CellStatus.Alive) { livelyNeighbors++; }
					}

					updatedWorld[row, col] = DetermineNewStatus(currStatus, livelyNeighbors);
				}
			}
			world = updatedWorld;
		}
	}
}