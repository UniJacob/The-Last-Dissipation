using UnityEngine;

public static class StageSettings
{
    public static string StageFilePath = "Stage_Texts/test2";

    // Stage "grid" data
    public const int VerUnits = 6 * 4 * 7 * 9; // 1512
    public const int HorUnits = 30;
    public const float TopPadding = 5500, BottomPadding = 5500, HorizontalPadding = 4000;
    public static readonly Vector3 bottomLeftBorder = GetBlBorder();
    public static readonly Vector3 topRightBorder = GetTrBorder();
    public static readonly float UnitsPerVerUnit = (topRightBorder.y - bottomLeftBorder.y) / VerUnits;
    public static readonly float UnitsPerHorUnit = (topRightBorder.x - bottomLeftBorder.x) / VerUnits;

    // Regular expressions for parsing stage files
    public const string Exp1 = @"^(?<weight>\d+)$";
    public const string Exp2 = @"^(?<weight>\d+)\((?<shortNotes>-?\d+(,-?\d+)*)\)$";
    public const string Exp3 = @"^(?<weight>\d+)\[(?<longNotes>-?\d+(,-?\d+)*)\]$";
    public const string Exp4 = @"^(?<weight>\d+)\((?<shortNotes>-?\d+(,-?\d+)*)\)\[(?<longNotes>-?\d+(,-?\d+)*)\]$";

    private static Vector3 GetBlBorder()
    {
        Vector3 tmp = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        tmp.x += HorizontalPadding;
        tmp.y += BottomPadding;
        return tmp;
    }
    private static Vector3 GetTrBorder()
    {
        Vector3 tmp = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
        tmp.x -= HorizontalPadding;
        tmp.y -= TopPadding;

        return tmp;
    }

}
