using System.Collections.Generic;

public static class Encoder
{

    //short size in bytes
    private const int SHORT_SIZE = 2;

    //int size in bytes
    private const int INT_SIZE = 4;

    //bytes used to encode the packet length
    private const int PACKET_LENGTH_SIZE = 2;

    //bytes used to encode the command
    private const int COMMAND_SIZE = 2;

    //bytes used to encode the msg header
    private const int HEADER_SIZE = PACKET_LENGTH_SIZE + COMMAND_SIZE;

    //offset where the packet length is encoded
    private const int PACKET_LENGTH_START = 0;

    //offset where the command is encoded
    private const int COMMAND_START = 2;


    //using big endian encoding

    public static void EncodeUShort (byte[] buffer, int position, ushort value)
    {
        buffer[position] = (byte)((value >> 8) & 0xff);
        buffer[position + 1] = (byte)(value & 0xff);
    }

    public static ushort DecodeUShort (byte[] buffer, int position)
    {
        ushort value = (ushort)((buffer[position] << 8) | buffer[position + 1]);
        return value;
    }

    public static void EncodeInt (byte[] buffer, int position, int value)
    {
        buffer[position] = (byte)((value >> 24) & 0xff);
        buffer[position + 1] = (byte)((value >> 16) & 0xff);
        buffer[position + 2] = (byte)((value >> 8) & 0xff);
        buffer[position + 3] = (byte)(value & 0xff);
    }

    public static int DecodeInt (byte[] buffer, int position)
    {
        int value = (buffer[position] << 24) | (buffer[position + 1] << 16)
            | (buffer[position + 2] << 8) | buffer[position + 3];

        return value;
    }

    public static void EncodeULong (byte[] buffer, int position, ulong value)
    {
        buffer[position] = (byte)((value >> 56) & 0xff);
        buffer[position + 1] = (byte)((value >> 48) & 0xff);
        buffer[position + 2] = (byte)((value >> 40) & 0xff);
        buffer[position + 3] = (byte)((value >> 32) & 0xff);
        buffer[position + 4] = (byte)((value >> 24) & 0xff);
        buffer[position + 5] = (byte)((value >> 16) & 0xff);
        buffer[position + 6] = (byte)((value >> 8) & 0xff);
        buffer[position + 7] = (byte)(value & 0xff);
    }

    public static ulong DecodeULong (byte[] buffer, int position)
    {
        ulong value = (ulong)((buffer[position] << 56) | (buffer[position + 1] << 48)
            | (buffer[position + 2] << 40) | (buffer[position + 3] << 32)
            | (buffer[position + 4] << 24) | (buffer[position + 5] << 16)
            | (buffer[position + 6] << 8) | buffer[position + 7]);

        return value;
    }

    public static void EncodeUShortVector2 (byte[] buffer, int position, UShortVector2 vec)
    {
        EncodeUShort(buffer, position, vec.x);
        EncodeUShort(buffer, position + SHORT_SIZE, vec.y);
    }

    public static UShortVector2 DecodeUShortVector2 (byte[] buffer, int position)
    {
        ushort x = DecodeUShort(buffer, position);
        ushort y = DecodeUShort(buffer, position + SHORT_SIZE);
        return new UShortVector2(x, y);
    }


    public static void SetPacketLength (byte[] buffer, ushort length)
    {
        EncodeUShort(buffer, PACKET_LENGTH_START, length);
    }

    public static ushort GetPacketLength (byte[] buffer, int offset)
    {
        ushort length = DecodeUShort(buffer, offset + PACKET_LENGTH_START);
        return length;
    }

    public static void SetCommand (byte[] buffer, ushort command)
    {
        EncodeUShort(buffer, COMMAND_START, command);
    }

    public static ushort GetCommand (byte[] buffer, int offset)
    {
        ushort command = DecodeUShort(buffer, offset + COMMAND_START);
        return command;
    }


    public static int Encode (byte[] buffer, BaseNetData data)
    {
        int packetLength = 0;

        switch (data.command)
        {
            case ServerCommands.GET_CARD:
                packetLength = EncodeBaseNetData(buffer, data);
                break;
            case ServerCommands.STARTING_NEW_GAME:
                packetLength = EncodeBaseNetData(buffer, data);
                break;
            default:
                break;
        }

        return packetLength;
    }


    public static int Decode (byte[] buffer, int offset, out BaseNetData data)
    {
        data = new BaseNetData(0);

        ushort packetLength = GetPacketLength(buffer, offset);

        //if buffer doesnt contains all the packet return
        if (buffer.Length - offset < packetLength) return 0;

        ushort command = GetCommand(buffer, offset);
        switch (command)
        {
            case ServerCommands.GET_CARD:
                DecodeUShortNetData(buffer, offset, out data);
                break;
            case ServerCommands.STARTING_NEW_GAME:
                DecodeBaseNetData(buffer, offset, out data);
                break;
            default:
                break;
        }

        return packetLength;
    }


    #region Encoders

    private static int EncodeBaseNetData (byte[] buffer, BaseNetData data)
    {
        int packetLength = HEADER_SIZE;

        if (buffer.Length < packetLength) return 0;

        SetPacketLength(buffer, (ushort)packetLength);
        SetCommand(buffer, data.command);

        return packetLength;
    }

    private static int EncodeBoolNetData (byte[] buffer, BaseNetData data)
    {
        BoolNetData bData = (BoolNetData)data;

        //header size + 1 byte for bool
        int packetLength = HEADER_SIZE + 1;

        if (buffer.Length < packetLength) return 0;

        int pos = HEADER_SIZE;
        if (bData.value) buffer[pos] = 1;
        else buffer[pos] = 0;

        SetPacketLength(buffer, (ushort)packetLength);
        SetCommand(buffer, bData.command);

        return packetLength;
    }

    private static int EncodeUShortNetData (byte[] buffer, BaseNetData data)
    {
        UShortNetData usData = (UShortNetData)data;

        //header size + 2 bytes for ushort
        int packetLength = HEADER_SIZE + SHORT_SIZE;

        if (buffer.Length < packetLength) return 0;

        int pos = HEADER_SIZE;
        EncodeUShort(buffer, pos, usData.value);

        SetPacketLength(buffer, (ushort)packetLength);
        SetCommand(buffer, usData.command);

        return packetLength;
    }

    private static int EncodeUShortBoolNetData (byte[] buffer, BaseNetData data)
    {
        UShortBoolNetData ubData = (UShortBoolNetData)data;

        //header size + short size + 1 byte for bool
        int packetLength = HEADER_SIZE + SHORT_SIZE + 1;

        if (buffer.Length < packetLength) return 0;

        int pos = HEADER_SIZE;
        EncodeUShort(buffer, pos, ubData.num);
        pos += SHORT_SIZE;
        if (ubData.value) buffer[pos] = 1;
        else buffer[pos] = 0;

        SetPacketLength(buffer, (ushort)packetLength);
        SetCommand(buffer, ubData.command);

        return packetLength;
    }

    private static int EncodeUShortVector2NetData (byte[] buffer, BaseNetData data)
    {
        UShortVector2NetData vData = (UShortVector2NetData)data;

        int packetLength = HEADER_SIZE + SHORT_SIZE * 2;

        if (buffer.Length < packetLength) return 0;

        int pos = HEADER_SIZE;
        EncodeUShortVector2(buffer, pos, vData.vector);

        SetPacketLength(buffer, (ushort)packetLength);
        SetCommand(buffer, vData.command);

        return packetLength;
    }

    #endregion

    #region Decoders

    private static void DecodeBaseNetData (byte[] buffer, int offset, out BaseNetData data)
    {
        ushort command = GetCommand(buffer, offset);

        data = new BaseNetData(command);
    }

    private static void DecodeBoolNetData (byte[] buffer, int offset, out BaseNetData data)
    {
        int pos = offset + HEADER_SIZE;
        bool value;
        if (buffer[pos] == 0) value = false;
        else value = true;

        ushort command = GetCommand(buffer, offset);
        data = new BoolNetData(command, value);
    }

    private static void DecodeUShortNetData (byte[] buffer, int offset, out BaseNetData data)
    {
        int pos = offset + HEADER_SIZE;
        ushort value = DecodeUShort(buffer, pos);

        ushort command = GetCommand(buffer, offset);
        data = new UShortNetData(command, value);
    }

    private static void DecodeUShortBoolNetData (byte[] buffer, int offset, out BaseNetData data)
    {
        int pos = offset + HEADER_SIZE;
        ushort num = DecodeUShort(buffer, pos);
        pos += SHORT_SIZE;
        bool value;
        if (buffer[pos] == 0) value = false;
        else value = true;

        ushort command = GetCommand(buffer, offset);
        UShortBoolNetData ubData = new UShortBoolNetData(command, num, value);

        data = ubData;
    }

    private static void DecodeUShortVector2NetData (byte[] buffer, int offset, out BaseNetData data)
    {
        int pos = offset + HEADER_SIZE;
        UShortVector2 vector = DecodeUShortVector2(buffer, pos);

        ushort command = GetCommand(buffer, offset);
        data = new UShortVector2NetData(command, vector);
    }

    private static void DecodeSimulationNetData (byte[] buffer, int offset, out BaseNetData data)
    {
        throw new System.NotImplementedException();
    }

    #endregion

}
