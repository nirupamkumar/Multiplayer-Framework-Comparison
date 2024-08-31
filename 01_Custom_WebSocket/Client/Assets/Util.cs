public class Util
{
    public static int[,] Transform1DArrayTo2DArray(int[] inputArray, int columns, int rows)
    {
        var resultArray = new int[columns, rows];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                int index = i * columns + j;
                if (index < inputArray.Length)
                {
                    resultArray[j, i] = inputArray[index];
                }
                else
                {
                    resultArray[i, j] = 0;
                }
            }
        }

        return resultArray;
    }
}