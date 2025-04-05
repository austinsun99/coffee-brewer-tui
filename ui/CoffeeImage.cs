using Spectre.Console;

namespace UI;

public static class CoffeeImage {

	readonly static Color WHITE = Color.Grey93;


	public static Canvas CoffeeCanvas() {
		const int width = 16;
		const int height = 16;
		var canvas = new Canvas(16, 16);

		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				canvas.SetPixel(i, j, Color.Plum4);
			}
		}
		return canvas;
	}

}
