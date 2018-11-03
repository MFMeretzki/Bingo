
public struct UShortVector3
{
    public readonly ushort x;
    public readonly ushort y;
    public readonly ushort z;


    public UShortVector3 (ushort x, ushort y, ushort z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }


    public override bool Equals (object obj)
    {
        if (!(obj is UShortVector3)) return false;

        UShortVector3 v = (UShortVector3)obj;
        return x == v.x && y == v.y && v.z == z;
    }

    public bool Equals (UShortVector3 v)
    {        return x == v.x && y == v.y && v.z == z;
    }

    public override int GetHashCode ()
    {
        return x+z ^ y;
    }

    public static bool operator == (UShortVector3 left, UShortVector3 right)
    {
        return left.x == right.x && left.y == right.y && left.z == right.z;
    }

    public static bool operator != (UShortVector3 left, UShortVector3 right)
    {
        return !(left == right);
    }

    public static UShortVector3 operator + (UShortVector3 left, ushort right)
    {
        return new UShortVector3((ushort)(left.x + right), (ushort)(left.y + right), (ushort)(left.z + right));
    }

    public static UShortVector3 operator + (UShortVector3 left, UShortVector3 right)
    {
        return new UShortVector3((ushort)(left.x + right.x), (ushort)(left.y + right.y), (ushort)(left.z + right.z));
    }

    public static UShortVector3 operator - (UShortVector3 left, ushort right)
    {
        return new UShortVector3((ushort)(left.x - right), (ushort)(left.y - right), (ushort)(left.z - right));
    }

    public static UShortVector3 operator - (UShortVector3 left, UShortVector3 right)
    {
        return new UShortVector3((ushort)(left.x - right.x), (ushort)(left.y - right.y), (ushort)(left.z - right.z));
    }

    public static UShortVector3 operator * (UShortVector3 left, ushort right)
    {
        return new UShortVector3((ushort)(left.x * right), (ushort)(left.y * right), (ushort)(left.z * right));
    }

    public static UShortVector3 operator * (UShortVector3 left, UShortVector3 right)
    {
        return new UShortVector3((ushort)(left.x * right.x), (ushort)(left.y * right.y), (ushort)(left.z * right.z));
    }

    public static UShortVector3 operator / (UShortVector3 left, ushort right)
    {
        return new UShortVector3((ushort)(left.x / right), (ushort)(left.y / right), (ushort)(left.z / right));
    }

    public static UShortVector3 operator / (UShortVector3 left, UShortVector3 right)
    {
        return new UShortVector3((ushort)(left.x / right.x), (ushort)(left.y / right.y), (ushort)(left.z / right.z));
    }

}
