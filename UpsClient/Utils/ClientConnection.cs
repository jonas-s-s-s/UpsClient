using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using System.IO;
using System.Net.Http;

namespace UpsClient.Utils;

public class ClientConnection
{
    private TcpClient _client;
    private NetworkStream? _stream;
    private ProtocolParser _parser;

    public ClientConnection()
    {
        _client = new();
        _stream = null;
        _parser = new ProtocolParser();
    }

    public async void connect(string hostname, string port)
    {
        int portInt = 0;

        if (!Int32.TryParse(port, out portInt))
        {
            throw new Exception("ClientConnection() - Can't parse port: " + port);
        }

        var ipEndPoint = _createEndpoint(hostname, portInt);

        await _client.ConnectAsync(ipEndPoint);
        _stream = _client.GetStream();
    }

    public async Task<List<ProtocolData>?> readMsg()
    {
        if (_stream == null)
            throw new Exception("readMsg() stream can't be null!");
        if (!_client.Connected)
            throw new Exception("readMsg() Client has to be connected!");

        byte[] buffer = new byte[4096];
        int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);

        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        return _parser.parse(receivedMessage);
    }

    public async void sendMsg(ProtocolData msg)
    {
        if (_stream == null)
            throw new Exception("sendMsg() stream can't be null!");

        if (!_client.Connected)
            throw new Exception("sendMsg() Client has to be connected!");

        byte[] messageBytes = Encoding.UTF8.GetBytes(ProtocolSerializer.serializeProtocolData(msg));

        await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);
        Console.WriteLine($"Sent message: {msg}");
    }

    public static IPEndPoint _createEndpoint(string hostNameOrAddress, int port)
    {
        IPAddress addr;
        bool gotAddr = IPAddress.TryParse(hostNameOrAddress, out addr);
        if (!gotAddr)
        {
            IPHostEntry dnsInfo = Dns.GetHostEntry(hostNameOrAddress);
            addr = dnsInfo.AddressList.First();
        }
        IPEndPoint instance = new IPEndPoint(addr, port);
        return instance;
    }

    ~ClientConnection()
    {
        _client.Dispose();
        if (_stream != null)
            _stream.Dispose();
    }
}
