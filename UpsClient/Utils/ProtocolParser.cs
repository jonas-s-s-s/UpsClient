using System;
using System.Collections.Generic;
using System.Linq;

namespace UpsClient.Utils;
public class ProtocolParser
{
    private MethodName savedMethod = MethodName.UNINITIALIZED;
    private Dictionary<string, string> savedData = new Dictionary<string, string>();
    private string savedLine = string.Empty;
    private bool isSavedLineFirst = true;

    private const int MaxLineLength = 10000000;
    private const int MaxMethodNameLength = 32;
    private const string EndSequence = "\r\n\r\n";
    private const int EndSequenceLen = 4;
    private const string LineDelimiter = ":";
    private const string EndOfLine = "\n";

    public List<ProtocolData>? parse(string data)
    {
        List<ProtocolData> output = new List<ProtocolData>();

        // Split messages contained in data by END_SEQUENCE
        List<string> messages = split(data, EndSequence);

        // Split each message by END_OF_LINE and process its content
        foreach (string msg in messages)
        {
            string msgTemp = msg;

            bool containsEndSeq = msgTemp.EndsWith(EndSequence);
            // Remove the ending sequence if it's present
            if (containsEndSeq)
            {
                msgTemp = msgTemp.Substring(0, msgTemp.Length - EndSequenceLen);
            }

            // Will parse each line and save parsed data to class fields
            parseMsgLines(split(msgTemp, EndOfLine));

            // If the end of the message was detected, put all cached data into the output vector
            if (containsEndSeq)
            {
                output.Add(new ProtocolData(savedMethod, savedData));
                // Clear any old cached data
                clearCache();
            }
        }

        // Return results
        return output.Count > 0 ? output : null;
    }

    private void parseMsgLines(List<string> lines)
    {
        foreach (string line in lines)
        {
            // Error checking
            if (savedLine.Length + line.Length > MaxLineLength)
            {
                throw new InvalidOperationException("ProtocolParser::parse: Error - max line length has been exceeded!");
            }

            if (isSavedLineFirst && line.Length + savedLine.Length > MaxMethodNameLength)
            {
                throw new InvalidOperationException("ProtocolParser::parse: Error - invalid msg format - method name cannot be longer than 32 chars.");
            }

            // If we didn't receive the full line, save it for later
            if (!line.EndsWith(EndOfLine))
            {
                savedLine += line;
                continue;
            }
            // \n can now be removed
            savedLine = line.Remove(line.Length - 1);

            // We have a full line, add any saved value to it
            if (!string.IsNullOrEmpty(savedLine))
            {
                savedLine = savedLine.Insert(0, savedLine);
                // Clear the line cache
                savedLine = string.Empty;
            }

            if (isSavedLineFirst)
            {
                // First line of the message is the method name
                MethodName parsedName = parseMethodName(line);
                if (parsedName == MethodName.PARSING_FAILED)
                    throw new InvalidOperationException("ProtocolParser::parse: Error - This method name is not allowed.");
                else
                    savedMethod = parsedName;

                isSavedLineFirst = false;
            }
            else
            {
                // All other lines are in the format <attr_name>:<attr_value>\n
                List<string> lineFields = split(savedLine, LineDelimiter);
                if (lineFields.Count < 2)
                {
                    throw new InvalidOperationException("ProtocolParser::parse: Error - Invalid line, missing line delimiter.");
                }
                // Save the results
                string attrName = lineFields[0];
                string attrValue = lineFields[1];
                savedData[attrName] = attrValue;
            }
        }
    }

    private void clearCache()
    {
        savedLine = string.Empty;
        isSavedLineFirst = true;
        savedMethod = MethodName.UNINITIALIZED;
        savedData.Clear();
    }

    private List<string> split(string s, string delimiter)
    {
        int posStart = 0;
        int posEnd;
        int delimLen = delimiter.Length;
        string token;
        List<string> res = new List<string>();

        while ((posEnd = s.IndexOf(delimiter, posStart)) != -1)
        {
            token = s.Substring(posStart, (posEnd - posStart) + delimLen);
            posStart = posEnd + delimLen;
            res.Add(token);
        }

        // Do NOT add an empty string to the end of the vector
        string end = s.Substring(posStart);
        if (!string.IsNullOrEmpty(end))
            res.Add(end);

        return res;
    }

    private MethodName parseMethodName(string name)
    {
        if (MethodNameTable.Table.TryGetValue(name, out var method))
        {
            return method;
        }
        else
        {
            return MethodName.PARSING_FAILED;
        }
    }
}