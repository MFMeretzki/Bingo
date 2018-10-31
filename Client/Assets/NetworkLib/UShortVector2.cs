
public struct UShortVector2
{
    public readonly ushort x;
    public readonly ushort y;


    public UShortVector2 (ushort x, ushort y)
    {
        this.x = x;
        this.y = y;
    }


    public override bool Equals (object obj)
    {
        if (!(obj is UShortVector2)) return false;

        UShortVector2 v = (UShortVector2)obj;
        return x == v.x && y == v.y;
    }

    public bool Equals (UShortVector2 v)
    {
        return x == v.x && y == v.y;
    }

    public override int GetHashCode ()
    {
        return x ^ y;
    }

    public static bool operator == (UShortVector2 left, UShortVector2 right)
    {
        return left.x == right.x && left.y == right.y;
    }

    public static bool operator != (UShortVector2 left, UShortVector2 right)
    {
        return !(left == right);
    }

    public static UShortVector2 operator + (UShortVector2 left, ushort right)
    {
        return new UShortVector2((ushort)(left.x + right), (ushort)(left.y + right));
    }

    public static UShortVector2 operator + (UShortVector2 left, UShortVector2 right)
    {
        return new UShortVector2((ushort)(left.x + right.x), (ushort)(left.y + right.y));
    }

    public static UShortVector2 operator - (UShortVector2 left, ushort right)
    {
        return new UShortVector2((ushort)(left.x - right), (ushort)(left.y - right));
    }

    public static UShortVector2 operator - (UShortVector2 left, UShortVector2 right)
    {
        return new UShortVector2((ushort)(left.x - right.x), (ushort)(left.y - right.y));
    }

    public static UShortVector2 operator * (UShortVector2 left, ushort right)
    {
        return new UShortVector2((ushort)(left.x * right), (ushort)(left.y * right));
    }

    public static UShortVector2 operator * (UShortVector2 left, UShortVector2 right)
    {
        return new UShortVector2((ushort)(left.x * right.x), (ushort)(left.y * right.y));
    }

    public static UShortVector2 operator / (UShortVector2 left, ushort right)
    {
        return new UShortVector2((ushort)(left.x / right), (ushort)(left.y / right));
    }

    public static UShortVector2 operator / (UShortVector2 left, UShortVector2 right)
    {
        return new UShortVector2((ushort)(left.x / right.x), (ushort)(left.y / right.y));
    }

}
