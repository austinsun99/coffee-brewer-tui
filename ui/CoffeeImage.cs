using Spectre.Console;

namespace UI;

public static class CoffeeImage
{

	private static readonly Color CUP_COLOUR = Color.Grey100;
	private static readonly Color BACKGROUND_COLOUR = Color.Orange4_1;

	public static Canvas CoffeeCanvas(int width, int height)
	{

		const int heightPad = 9;
		const int leftPad = 3;
		const int cupBaseWidth = 10;

		var canvas = new Canvas(width, height);

		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				canvas.SetPixel(i, j, BACKGROUND_COLOUR);
			}
		}

		for (int i = 0; i < cupBaseWidth; i++)
		{
			canvas.SetPixel(i + leftPad, heightPad, CUP_COLOUR);
		}

		for (int i = 0; i < cupBaseWidth + 2; i++)
		{
			canvas.SetPixel(i + leftPad, heightPad + 1, CUP_COLOUR);
		}

		for (int i = 0; i < cupBaseWidth + 2; i++)
		{
			canvas.SetPixel(i + leftPad, heightPad + 2, CUP_COLOUR);
		}
		canvas.SetPixel(cupBaseWidth + leftPad, heightPad + 2, BACKGROUND_COLOUR);

		for (int i = 0; i < cupBaseWidth + 2; i++)
		{
			canvas.SetPixel(i + leftPad, heightPad + 3, CUP_COLOUR);
		}

		for (int i = 0; i < cupBaseWidth - 2; i++)
		{
			canvas.SetPixel(i + leftPad + 1, heightPad + 4, CUP_COLOUR);
		}

		for (int i = 0; i < cupBaseWidth - 4; i++)
		{
			canvas.SetPixel(i + leftPad + 2, heightPad + 5, CUP_COLOUR);
		}

		return canvas;
	}

	public static Panel CoffeeASCII()
	{

		const string s1 = "█";
		const string s2 = "▓";
		const string s3 = "▒";
		const string s4 = "░";

		Random rnd = new Random();

		string lineOne = " ";
		string lineTwo = " ";
		string lineThree = "  ";
		string lineFour = "  ";

		for (int i = 0; i < 20; i++) {
			string charToAppend = rnd.Next(3) == 0 ? s2 : s1;
			lineOne += charToAppend;
		}

		for (int i = 0; i < 20; i++) {
			string charToAppend = rnd.Next(3) == 0 ? s3 : s2;
			lineTwo += charToAppend;
		}

		for (int i = 0; i < 20; i++) {
			lineThree += rnd.Next(3) switch {
				0 => s3,
				1 => s4,
				2 => " ",
				_ => " "
			};
		}

		for (int i = 0; i < 20; i++) {
			string charToAppend = rnd.Next(3) == 0 ? " " : s4;
			lineFour += charToAppend;
		}

		var coffeeDrawing = 
			""" 
			&&&XXXXXXXXXXXXXXXXXXX     
			&&&XXXXXXXXXXXXXXXXXXXXXXXX
			&&&XXXXXXXXXXXXXXXXXXX    X
			&&&XXXXXXXXXXXXXXXXXXX    X
			&&&XXXXXXXXXXXXXXXXXXX    X
			&&&XXXXXXXXXXXXXXXXXXXXXXXX
			&&&&XXXXXXXXXXXXXXXXXX     
			  &&&&&&&&&XXXXXXXXX       
			    &&&&&&&&&&&&&&         
			""";

		var coffeeImage = new System.Text.StringBuilder()
			.AppendLine(lineFour)
			.AppendLine(lineThree)
			.AppendLine(lineTwo)
			.Append(coffeeDrawing);

		var panel = new Panel($"[rosybrown]{coffeeImage.ToString()}[/]");
		panel.Border = BoxBorder.None;
		return panel;
	}

}
