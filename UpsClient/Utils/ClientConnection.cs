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
using System.Threading;

namespace UpsClient.Utils;

public class ClientConnection
{
    private static readonly SemaphoreLocker _sendLock = new SemaphoreLocker();
    private static readonly object _readLock = new object();

    private TcpClient _client;
    private NetworkStream? _stream;
    private ProtocolParser _parser;
    public string port;
    public string hostname;

    public ClientConnection()
    {
        _client = new();
        _stream = null;
        _parser = new ProtocolParser();
    }

    public async Task connect(string hostname, string port)
    {
        this.port = port;
        this.hostname = hostname;

        int portInt = 0;

        if (!Int32.TryParse(port, out portInt))
        {
            throw new Exception("ClientConnection() - Can't parse port: " + port);
        }

        var ipEndPoint = _createEndpoint(hostname, portInt);

        await _client.ConnectAsync(ipEndPoint);
        _stream = _client.GetStream();
    }

    public List<ProtocolData>? readMsg()
    {
        if (_stream == null)
            throw new IOException("readMsg() stream can't be null!");
        if (!_client.Connected)
            throw new IOException("readMsg() Client has to be connected!");

        byte[] buffer = new byte[4096];

        List<ProtocolData>? protocolData = null;

        lock(_readLock)
        {
            int bytesRead = _stream.Read(buffer, 0, buffer.Length);
            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            if(receivedMessage != null)
                protocolData = _parser.parse(receivedMessage);
        }

        return protocolData;
    }

    public async Task sendMsg(ProtocolData msg)
    {
        if (_stream == null)
            throw new IOException("sendMsg() stream can't be null!");

        if (!_client.Connected)
            throw new IOException("sendMsg() Client has to be connected!");

        byte[] messageBytes = Encoding.UTF8.GetBytes(ProtocolSerializer.serializeProtocolData(msg));

        await _sendLock.LockAsync(async () =>
        {
            await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);
        });

        Console.WriteLine($"Sent message: {msg}");
    }

    private static IPEndPoint _createEndpoint(string hostNameOrAddress, int port)
    {
        IPAddress addr;
        bool gotAddr = IPAddress.TryParse(hostNameOrAddress, out addr);
        if (!gotAddr)
        {
            IPHostEntry dnsInfo = Dns.GetHostEntry(hostNameOrAddress);
            IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(hostNameOrAddress).AddressList,a => a.AddressFamily == AddressFamily.InterNetwork);
            addr = ipv4Addresses.First();
        }
        IPEndPoint instance = new IPEndPoint(addr, port);
        return instance;
    }

    public bool isAlive()
    {
        try
        {
            return _client.Connected;
        }
        catch (Exception)
        {
            return false;
        }
    }

    ~ClientConnection()
    {
        _client.Dispose();
        if (_stream != null)
            _stream.Dispose();
    }
}

internal class SemaphoreLocker
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public async Task LockAsync(Func<Task> worker)
    {
        var isTaken = false;
        try
        {
            do
            {
                try
                {
                }
                finally
                {
                    isTaken = await _semaphore.WaitAsync(TimeSpan.FromSeconds(1));
                }
            }
            while (!isTaken);
            await worker();
        }
        finally
        {
            if (isTaken)
            {
                _semaphore.Release();
            }
        }
    }
}
