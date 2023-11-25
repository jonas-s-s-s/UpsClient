using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpsClient.Utils;

public enum MethodName
{
    CONNECTED_OK,
    REQ_ACCEPTED,
    REQ_DENIED,
    TERMINATE_CONNECTION,
    PARSING_FAILED,
    UNINITIALIZED,
    ENTER_USERNAME,
    GET_ROOM_LIST,
    JOIN_GAME,
    GAME_IDLE,
    GAME_LEAVE,
    GAME_COMMAND,
    GAME_STATE,
    GAME_PING
}

public static class MethodNameTable
{
    public static readonly Dictionary<string, MethodName> Table = new Dictionary<string, MethodName>
    {
        {"CONNECTED_OK", MethodName.CONNECTED_OK},
        {"REQ_ACCEPTED", MethodName.REQ_ACCEPTED},
        {"REQ_DENIED", MethodName.REQ_DENIED},
        {"TERMINATE_CONNECTION", MethodName.TERMINATE_CONNECTION},
        {"PARSING_FAILED", MethodName.PARSING_FAILED},
        {"UNINITIALIZED", MethodName.UNINITIALIZED},
        {"ENTER_USERNAME", MethodName.ENTER_USERNAME},
        {"GET_ROOM_LIST", MethodName.GET_ROOM_LIST},
        {"JOIN_GAME", MethodName.JOIN_GAME},
        {"GAME_IDLE", MethodName.GAME_IDLE},
        {"GAME_LEAVE", MethodName.GAME_LEAVE},
        {"GAME_COMMAND", MethodName.GAME_COMMAND},
        {"GAME_STATE", MethodName.GAME_STATE},
        {"GAME_PING", MethodName.GAME_PING}
    };
}

public class ProtocolData
{
    public MethodName Method { get; }
    public Dictionary<string, string> Data { get; }

    public ProtocolData(MethodName method, Dictionary<string, string> data)
    {
        Method = method;
        Data = data;
    }

    public bool hasField(string name)
    {
        return Data.ContainsKey(name);
    }

    public string getField(string name)
    {
        return Data[name];
    }
}

public static class ProtocolSerializer
{
    private const string EndSequence = "\r\n\r\n";
    private const string LineDelimiter = ":";
    private const string EndOfLine = "\n";

    public static string serializeMethodName(MethodName name)
    {
        var reverseLookup = MethodNameTable.Table.ToDictionary(kv => kv.Value, kv => kv.Key);

        if (reverseLookup.TryGetValue(name, out var methodName))
        {
            return methodName;
        }
        else
        {
            throw new InvalidOperationException("serializeMethodName: Error - Entered method name was not found. Is it contained in MethodNameTable?");
        }
    }

    public static string serializeProtocolData(ProtocolData data)
    {
        var output = new StringBuilder();

        // First add method name
        output.Append(serializeMethodName(data.Method));
        output.Append(EndOfLine);

        // Then add all attributes
        foreach (var (attrName, attrValue) in data.Data)
        {
            output.Append(attrName);
            output.Append(LineDelimiter);
            output.Append(attrValue);
            output.Append(EndOfLine);
        }

        // Add message end sequence
        output.Append(EndSequence);

        return output.ToString();
    }

    public static string serializeObjectList(List<List<string>> list)
    {
        var output = new StringBuilder();

        bool isFirstLine = true;
        foreach (var line in list)
        {
            if (!isFirstLine)
            {
                output.Append(",");
            }
            else
            {
                isFirstLine = false;
            }

            bool isFirstItem = true;
            output.Append("{");
            foreach (var lineItem in line)
            {
                if (!isFirstItem)
                {
                    output.Append(",");
                }
                else
                {
                    isFirstItem = false;
                }

                output.Append(lineItem);
            }
            output.Append("}");
        }

        return output.ToString();
    }

    public static ProtocolData newProtocolMessage(MethodName method, params (string, string)[] dataFields)
    {
        var data = dataFields.ToDictionary(pair => pair.Item1, pair => pair.Item2);
        return new ProtocolData(method, data);
    }

    public static ProtocolData newProtocolMessage(MethodName method)
    {
        return new ProtocolData(method, new Dictionary<string, string>());
    }
}
