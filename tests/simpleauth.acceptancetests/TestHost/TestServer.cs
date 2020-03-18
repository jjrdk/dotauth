//namespace SimpleAuth.AcceptanceTests
//{
//    using System;
//    using System.Buffers;
//    using System.Collections.Generic;
//    using System.Diagnostics.Contracts;
//    using System.IO;
//    using System.IO.Pipelines;
//    using System.Linq;
//    using System.Net;
//    using System.Net.Http;
//    using System.Net.WebSockets;
//    using System.Security.Cryptography;
//    using System.Threading;
//    using System.Threading.Tasks;

//    using Microsoft.AspNetCore.Hosting;
//    using Microsoft.AspNetCore.Hosting.Server;
//    using Microsoft.AspNetCore.Http;
//    using Microsoft.AspNetCore.Http.Features;

//    public class TestServer : IServer
//    {
//        private IWebHost _hostInstance;
//        private bool _disposed = false;
//        private ApplicationWrapper _application;

//        /// <summary>
//        /// For use with IHostBuilder.
//        /// </summary>
//        /// <param name="services"></param>
//        public TestServer(IServiceProvider services)
//            : this(services, new FeatureCollection())
//        {
//        }

//        /// <summary>
//        /// For use with IHostBuilder.
//        /// </summary>
//        /// <param name="services"></param>
//        /// <param name="featureCollection"></param>
//        public TestServer(IServiceProvider services, IFeatureCollection featureCollection)
//        {
//            this.Services = services ?? throw new ArgumentNullException(nameof(services));
//            this.Features = featureCollection ?? throw new ArgumentNullException(nameof(featureCollection));
//        }

//        /// <summary>
//        /// For use with IWebHostBuilder.
//        /// </summary>
//        /// <param name="builder"></param>
//        public TestServer(IWebHostBuilder builder)
//            : this(builder, new FeatureCollection())
//        {
//        }

//        /// <summary>
//        /// For use with IWebHostBuilder.
//        /// </summary>
//        /// <param name="builder"></param>
//        /// <param name="featureCollection"></param>
//        public TestServer(IWebHostBuilder builder, IFeatureCollection featureCollection)
//        {
//            if (builder == null)
//            {
//                throw new ArgumentNullException(nameof(builder));
//            }

//            this.Features = featureCollection ?? throw new ArgumentNullException(nameof(featureCollection));

//            var host = builder.UseServer(this).Build();
//            host.StartAsync().GetAwaiter().GetResult();
//            this._hostInstance = host;

//            this.Services = host.Services;
//        }

//        public Uri BaseAddress { get; set; } = new Uri("http://localhost/");

//        public IWebHost Host
//        {
//            get
//            {
//                return this._hostInstance
//                       ?? throw new InvalidOperationException("The TestServer constructor was not called with a IWebHostBuilder so IWebHost is not available.");
//            }
//        }

//        public IServiceProvider Services { get; }

//        public IFeatureCollection Features { get; }

//        /// <summary>
//        /// Gets or sets a value that controls whether synchronous IO is allowed for the <see cref="HttpContext.Request"/> and <see cref="HttpContext.Response"/>. The default value is <see langword="false" />.
//        /// </summary>
//        public bool AllowSynchronousIO { get; set; }

//        /// <summary>
//        /// Gets or sets a value that controls if <see cref="ExecutionContext"/> and <see cref="AsyncLocal{T}"/> values are preserved from the client to the server. The default value is <see langword="false" />.
//        /// </summary>
//        public bool PreserveExecutionContext { get; set; }

//        private ApplicationWrapper Application
//        {
//            get => this._application ?? throw new InvalidOperationException("The server has not been started or no web application was configured.");
//        }

//        public HttpMessageHandler CreateHandler()
//        {
//            var pathBase = this.BaseAddress == null ? PathString.Empty : PathString.FromUriComponent(this.BaseAddress);
//            return new ClientHandler(pathBase, this.Application) { AllowSynchronousIO = this.AllowSynchronousIO, PreserveExecutionContext = this.PreserveExecutionContext };
//        }

//        public HttpClient CreateClient()
//        {
//            return new HttpClient(this.CreateHandler()) { BaseAddress = this.BaseAddress };
//        }

//        public WebSocketClient CreateWebSocketClient()
//        {
//            var pathBase = this.BaseAddress == null ? PathString.Empty : PathString.FromUriComponent(this.BaseAddress);
//            return new WebSocketClient(pathBase, this.Application) { AllowSynchronousIO = this.AllowSynchronousIO, PreserveExecutionContext = this.PreserveExecutionContext };
//        }

//        /// <summary>
//        /// Begins constructing a request message for submission.
//        /// </summary>
//        /// <param name="path"></param>
//        /// <returns><see cref="RequestBuilder"/> to use in constructing additional request details.</returns>
//        public RequestBuilder CreateRequest(string path)
//        {
//            return new RequestBuilder(this, path);
//        }

//        /// <summary>
//        /// Creates, configures, sends, and returns a <see cref="HttpContext"/>. This completes as soon as the response is started.
//        /// </summary>
//        /// <returns></returns>
//        public async Task<HttpContext> SendAsync(System.Action<HttpContext> configureContext, CancellationToken cancellationToken = default)
//        {
//            if (configureContext == null)
//            {
//                throw new ArgumentNullException(nameof(configureContext));
//            }

//            var builder = new HttpContextBuilder(this.Application, this.AllowSynchronousIO, this.PreserveExecutionContext);
//            builder.Configure((context, reader) =>
//                {
//                    var request = context.Request;
//                    request.Scheme = this.BaseAddress.Scheme;
//                    request.Host = HostString.FromUriComponent(this.BaseAddress);
//                    if (this.BaseAddress.IsDefaultPort)
//                    {
//                        request.Host = new HostString(request.Host.Host);
//                    }
//                    var pathBase = PathString.FromUriComponent(this.BaseAddress);
//                    if (pathBase.HasValue && pathBase.Value.EndsWith("/"))
//                    {
//                        pathBase = new PathString(pathBase.Value.Substring(0, pathBase.Value.Length - 1));
//                    }
//                    request.PathBase = pathBase;
//                });
//            builder.Configure((context, reader) => configureContext(context));
//            // TODO: Wrap the request body if any?
//            return await builder.SendAsync(cancellationToken).ConfigureAwait(false);
//        }

//        public void Dispose()
//        {
//            if (!this._disposed)
//            {
//                this._disposed = true;
//                this._hostInstance?.Dispose();
//            }
//        }

//        Task IServer.StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
//        {
//            this._application = new ApplicationWrapper<TContext>(application, () =>
//                {
//                    if (this._disposed)
//                    {
//                        throw new ObjectDisposedException(this.GetType().FullName);
//                    }
//                });

//            return Task.CompletedTask;
//        }

//        Task IServer.StopAsync(CancellationToken cancellationToken)
//        {
//            return Task.CompletedTask;
//        }
//    }

//    internal abstract class ApplicationWrapper
//    {
//        internal abstract object CreateContext(IFeatureCollection features);

//        internal abstract Task ProcessRequestAsync(object context);

//        internal abstract void DisposeContext(object context, Exception exception);
//    }

//    internal class ApplicationWrapper<TContext> : ApplicationWrapper, IHttpApplication<TContext>
//    {
//        private readonly IHttpApplication<TContext> _application;
//        private readonly Action _preProcessRequestAsync;

//        public ApplicationWrapper(IHttpApplication<TContext> application, Action preProcessRequestAsync)
//        {
//            _application = application;
//            _preProcessRequestAsync = preProcessRequestAsync;
//        }

//        internal override object CreateContext(IFeatureCollection features)
//        {
//            return ((IHttpApplication<TContext>)this).CreateContext(features);
//        }

//        TContext IHttpApplication<TContext>.CreateContext(IFeatureCollection features)
//        {
//            return _application.CreateContext(features);
//        }

//        internal override void DisposeContext(object context, Exception exception)
//        {
//            ((IHttpApplication<TContext>)this).DisposeContext((TContext)context, exception);
//        }

//        void IHttpApplication<TContext>.DisposeContext(TContext context, Exception exception)
//        {
//            _application.DisposeContext(context, exception);
//        }

//        internal override Task ProcessRequestAsync(object context)
//        {
//            return ((IHttpApplication<TContext>)this).ProcessRequestAsync((TContext)context);
//        }

//        Task IHttpApplication<TContext>.ProcessRequestAsync(TContext context)
//        {
//            _preProcessRequestAsync();
//            return _application.ProcessRequestAsync(context);
//        }
//    }

//    /// <summary>
//    /// This adapts HttpRequestMessages to ASP.NET Core requests, dispatches them through the pipeline, and returns the
//    /// associated HttpResponseMessage.
//    /// </summary>
//    public class ClientHandler : HttpMessageHandler
//    {
//        private readonly ApplicationWrapper _application;
//        private readonly PathString _pathBase;

//        /// <summary>
//        /// Create a new handler.
//        /// </summary>
//        /// <param name="pathBase">The base path.</param>
//        /// <param name="application">The <see cref="IHttpApplication{TContext}"/>.</param>
//        internal ClientHandler(PathString pathBase, ApplicationWrapper application)
//        {
//            _application = application ?? throw new ArgumentNullException(nameof(application));

//            // PathString.StartsWithSegments that we use below requires the base path to not end in a slash.
//            if (pathBase.HasValue && pathBase.Value.EndsWith("/"))
//            {
//                pathBase = new PathString(pathBase.Value.Substring(0, pathBase.Value.Length - 1));
//            }
//            _pathBase = pathBase;
//        }

//        internal bool AllowSynchronousIO { get; set; }

//        internal bool PreserveExecutionContext { get; set; }

//        /// <summary>
//        /// This adapts HttpRequestMessages to ASP.NET Core requests, dispatches them through the pipeline, and returns the
//        /// associated HttpResponseMessage.
//        /// </summary>
//        /// <param name="request"></param>
//        /// <param name="cancellationToken"></param>
//        /// <returns></returns>
//        protected override async Task<HttpResponseMessage> SendAsync(
//            HttpRequestMessage request,
//            CancellationToken cancellationToken)
//        {
//            if (request == null)
//            {
//                throw new ArgumentNullException(nameof(request));
//            }

//            var contextBuilder = new HttpContextBuilder(_application, AllowSynchronousIO, PreserveExecutionContext);

//            var requestContent = request.Content ?? new StreamContent(Stream.Null);

//            // Read content from the request HttpContent into a pipe in a background task. This will allow the request
//            // delegate to start before the request HttpContent is complete. A background task allows duplex streaming scenarios.
//            contextBuilder.SendRequestStream(async writer =>
//            {
//                if (requestContent is StreamContent)
//                {
//                    // This is odd but required for backwards compat. If StreamContent is passed in then seek to beginning.
//                    // This is safe because StreamContent.ReadAsStreamAsync doesn't block. It will return the inner stream.
//                    var body = await requestContent.ReadAsStreamAsync();
//                    if (body.CanSeek)
//                    {
//                        // This body may have been consumed before, rewind it.
//                        body.Seek(0, SeekOrigin.Begin);
//                    }

//                    await body.CopyToAsync(writer);
//                }
//                else
//                {
//                    await requestContent.CopyToAsync(writer.AsStream());
//                }

//                await writer.CompleteAsync();
//            });

//            contextBuilder.Configure((context, reader) =>
//            {
//                var req = context.Request;

//                if (request.Version == HttpVersion.Version20)
//                {
//                    // https://tools.ietf.org/html/rfc7540
//                    req.Protocol = "HTTP/2";
//                }
//                else
//                {
//                    req.Protocol = "HTTP/" + request.Version.ToString(fieldCount: 2);
//                }
//                req.Method = request.Method.ToString();

//                req.Scheme = request.RequestUri.Scheme;

//                foreach (var header in request.Headers)
//                {
//                    req.Headers.Append(header.Key, header.Value.ToArray());
//                }

//                if (!req.Host.HasValue)
//                {
//                    // If Host wasn't explicitly set as a header, let's infer it from the Uri
//                    req.Host = HostString.FromUriComponent(request.RequestUri);
//                    if (request.RequestUri.IsDefaultPort)
//                    {
//                        req.Host = new HostString(req.Host.Host);
//                    }
//                }

//                req.Path = PathString.FromUriComponent(request.RequestUri);
//                req.PathBase = PathString.Empty;
//                if (req.Path.StartsWithSegments(_pathBase, out var remainder))
//                {
//                    req.Path = remainder;
//                    req.PathBase = _pathBase;
//                }
//                req.QueryString = QueryString.FromUriComponent(request.RequestUri);

//                if (requestContent != null)
//                {
//                    foreach (var header in requestContent.Headers)
//                    {
//                        req.Headers.Append(header.Key, header.Value.ToArray());
//                    }
//                }

//                req.Body = new AsyncStreamWrapper(reader.AsStream(), () => contextBuilder.AllowSynchronousIO);
//            });

//            var response = new HttpResponseMessage();

//            // Copy trailers to the response message when the response stream is complete
//            contextBuilder.RegisterResponseReadCompleteCallback(context =>
//            {
//                var responseTrailersFeature = context.Features.Get<IHttpResponseTrailersFeature>();

//                foreach (var trailer in responseTrailersFeature.Trailers)
//                {
//                    bool success = response.TrailingHeaders.TryAddWithoutValidation(trailer.Key, (IEnumerable<string>)trailer.Value);
//                    Contract.Assert(success, "Bad trailer");
//                }
//            });

//            var httpContext = await contextBuilder.SendAsync(cancellationToken);

//            response.StatusCode = (HttpStatusCode)httpContext.Response.StatusCode;
//            response.ReasonPhrase = httpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase;
//            response.RequestMessage = request;

//            response.Content = new StreamContent(httpContext.Response.Body);

//            foreach (var header in httpContext.Response.Headers)
//            {
//                if (!response.Headers.TryAddWithoutValidation(header.Key, (IEnumerable<string>)header.Value))
//                {
//                    bool success = response.Content.Headers.TryAddWithoutValidation(header.Key, (IEnumerable<string>)header.Value);
//                    Contract.Assert(success, "Bad header");
//                }
//            }
//            return response;
//        }
//    }

//    public class WebSocketClient
//    {
//        private readonly ApplicationWrapper _application;
//        private readonly PathString _pathBase;

//        internal WebSocketClient(PathString pathBase, ApplicationWrapper application)
//        {
//            _application = application ?? throw new ArgumentNullException(nameof(application));

//            // PathString.StartsWithSegments that we use below requires the base path to not end in a slash.
//            if (pathBase.HasValue && pathBase.Value.EndsWith("/"))
//            {
//                pathBase = new PathString(pathBase.Value.Substring(0, pathBase.Value.Length - 1));
//            }
//            _pathBase = pathBase;

//            SubProtocols = new List<string>();
//        }

//        public IList<string> SubProtocols
//        {
//            get;
//            private set;
//        }

//        public Action<HttpRequest> ConfigureRequest
//        {
//            get;
//            set;
//        }

//        internal bool AllowSynchronousIO { get; set; }
//        internal bool PreserveExecutionContext { get; set; }

//        public async Task<WebSocket> ConnectAsync(Uri uri, CancellationToken cancellationToken)
//        {
//            WebSocketClient.WebSocketFeature webSocketFeature = null;
//            var contextBuilder = new HttpContextBuilder(_application, AllowSynchronousIO, PreserveExecutionContext);
//            contextBuilder.Configure((context, reader) =>
//            {
//                var request = context.Request;
//                var scheme = uri.Scheme;
//                scheme = (scheme == "ws") ? "http" : scheme;
//                scheme = (scheme == "wss") ? "https" : scheme;
//                request.Scheme = scheme;
//                if (!request.Host.HasValue)
//                {
//                    request.Host = uri.IsDefaultPort
//                        ? new HostString(HostString.FromUriComponent(uri).Host)
//                        : HostString.FromUriComponent(uri);
//                }
//                request.Path = PathString.FromUriComponent(uri);
//                request.PathBase = PathString.Empty;
//                if (request.Path.StartsWithSegments(_pathBase, out var remainder))
//                {
//                    request.Path = remainder;
//                    request.PathBase = _pathBase;
//                }
//                request.QueryString = QueryString.FromUriComponent(uri);
//                request.Headers.Add("Connection", new string[] { "Upgrade" });
//                request.Headers.Add("Upgrade", new string[] { "websocket" });
//                request.Headers.Add("Sec-WebSocket-Version", new string[] { "13" });
//                request.Headers.Add("Sec-WebSocket-Key", new string[] { CreateRequestKey() });
//                request.Body = Stream.Null;

//                // WebSocket
//                webSocketFeature = new WebSocketClient.WebSocketFeature(context);
//                context.Features.Set<IHttpWebSocketFeature>(webSocketFeature);

//                ConfigureRequest?.Invoke(context.Request);
//            });

//            var httpContext = await contextBuilder.SendAsync(cancellationToken);

//            if (httpContext.Response.StatusCode != StatusCodes.Status101SwitchingProtocols)
//            {
//                throw new InvalidOperationException("Incomplete handshake, status code: " + httpContext.Response.StatusCode);
//            }
//            if (webSocketFeature.ClientWebSocket == null)
//            {
//                throw new InvalidOperationException("Incomplete handshake");
//            }

//            return webSocketFeature.ClientWebSocket;
//        }

//        private string CreateRequestKey()
//        {
//            byte[] data = new byte[16];
//            var rng = RandomNumberGenerator.Create();
//            rng.GetBytes(data);
//            return Convert.ToBase64String(data);
//        }

//        private class WebSocketFeature : IHttpWebSocketFeature
//        {
//            private readonly HttpContext _httpContext;

//            public WebSocketFeature(HttpContext context)
//            {
//                _httpContext = context;
//            }

//            bool IHttpWebSocketFeature.IsWebSocketRequest => true;

//            public WebSocket ClientWebSocket { get; private set; }

//            public WebSocket ServerWebSocket { get; private set; }

//            async Task<WebSocket> IHttpWebSocketFeature.AcceptAsync(WebSocketAcceptContext context)
//            {
//                var websockets = TestWebSocket.CreatePair(context.SubProtocol);
//                if (_httpContext.Response.HasStarted)
//                {
//                    throw new InvalidOperationException("The response has already started");
//                }

//                _httpContext.Response.StatusCode = StatusCodes.Status101SwitchingProtocols;
//                ClientWebSocket = websockets.Item1;
//                ServerWebSocket = websockets.Item2;
//                await _httpContext.Response.Body.FlushAsync(_httpContext.RequestAborted); // Send headers to the client
//                return ServerWebSocket;
//            }
//        }
//    }

//    internal class TestWebSocket : WebSocket
//    {
//        private ReceiverSenderBuffer _receiveBuffer;
//        private ReceiverSenderBuffer _sendBuffer;
//        private readonly string _subProtocol;
//        private WebSocketState _state;
//        private WebSocketCloseStatus? _closeStatus;
//        private string _closeStatusDescription;
//        private Message _receiveMessage;

//        public static Tuple<TestWebSocket, TestWebSocket> CreatePair(string subProtocol)
//        {
//            var buffers = new[] { new ReceiverSenderBuffer(), new ReceiverSenderBuffer() };
//            return Tuple.Create(
//                new TestWebSocket(subProtocol, buffers[0], buffers[1]),
//                new TestWebSocket(subProtocol, buffers[1], buffers[0]));
//        }

//        public override WebSocketCloseStatus? CloseStatus
//        {
//            get { return _closeStatus; }
//        }

//        public override string CloseStatusDescription
//        {
//            get { return _closeStatusDescription; }
//        }

//        public override WebSocketState State
//        {
//            get { return _state; }
//        }

//        public override string SubProtocol
//        {
//            get { return _subProtocol; }
//        }

//        public async override Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
//        {
//            ThrowIfDisposed();

//            if (State == WebSocketState.Open || State == WebSocketState.CloseReceived)
//            {
//                // Send a close message.
//                await CloseOutputAsync(closeStatus, statusDescription, cancellationToken);
//            }

//            if (State == WebSocketState.CloseSent)
//            {
//                // Do a receiving drain
//                var data = new byte[1024];
//                WebSocketReceiveResult resultKind;
//                do
//                {
//                    resultKind = await ReceiveAsync(new ArraySegment<byte>(data), cancellationToken);
//                }
//                while (resultKind.MessageType != WebSocketMessageType.Close);
//            }
//        }

//        public async override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
//        {
//            ThrowIfDisposed();
//            ThrowIfOutputClosed();

//            var message = new Message(closeStatus, statusDescription);
//            await _sendBuffer.SendAsync(message, cancellationToken);

//            if (State == WebSocketState.Open)
//            {
//                _state = WebSocketState.CloseSent;
//            }
//            else if (State == WebSocketState.CloseReceived)
//            {
//                _state = WebSocketState.Closed;
//                Close();
//            }
//        }

//        public override void Abort()
//        {
//            if (_state >= WebSocketState.Closed) // or Aborted
//            {
//                return;
//            }

//            _state = WebSocketState.Aborted;
//            Close();
//        }

//        public override void Dispose()
//        {
//            if (_state >= WebSocketState.Closed) // or Aborted
//            {
//                return;
//            }

//            _state = WebSocketState.Closed;
//            Close();
//        }

//        public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
//        {
//            ThrowIfDisposed();
//            ThrowIfInputClosed();
//            ValidateSegment(buffer);
//            // TODO: InvalidOperationException if any receives are currently in progress.

//            Message receiveMessage = _receiveMessage;
//            _receiveMessage = null;
//            if (receiveMessage == null)
//            {
//                receiveMessage = await _receiveBuffer.ReceiveAsync(cancellationToken);
//            }
//            if (receiveMessage.MessageType == WebSocketMessageType.Close)
//            {
//                _closeStatus = receiveMessage.CloseStatus;
//                _closeStatusDescription = receiveMessage.CloseStatusDescription ?? string.Empty;
//                var resultKind = new WebSocketReceiveResult(0, WebSocketMessageType.Close, true, _closeStatus, _closeStatusDescription);
//                if (_state == WebSocketState.Open)
//                {
//                    _state = WebSocketState.CloseReceived;
//                }
//                else if (_state == WebSocketState.CloseSent)
//                {
//                    _state = WebSocketState.Closed;
//                    Close();
//                }
//                return resultKind;
//            }
//            else
//            {
//                int count = Math.Min(buffer.Count, receiveMessage.Buffer.Count);
//                bool endOfMessage = count == receiveMessage.Buffer.Count;
//                Array.Copy(receiveMessage.Buffer.Array, receiveMessage.Buffer.Offset, buffer.Array, buffer.Offset, count);
//                if (!endOfMessage)
//                {
//                    receiveMessage.Buffer = new ArraySegment<byte>(receiveMessage.Buffer.Array, receiveMessage.Buffer.Offset + count, receiveMessage.Buffer.Count - count);
//                    _receiveMessage = receiveMessage;
//                }
//                endOfMessage = endOfMessage && receiveMessage.EndOfMessage;
//                return new WebSocketReceiveResult(count, receiveMessage.MessageType, endOfMessage);
//            }
//        }

//        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
//        {
//            ValidateSegment(buffer);
//            if (messageType != WebSocketMessageType.Binary && messageType != WebSocketMessageType.Text)
//            {
//                // Block control frames
//                throw new ArgumentOutOfRangeException(nameof(messageType), messageType, string.Empty);
//            }

//            var message = new Message(buffer, messageType, endOfMessage, cancellationToken);
//            return _sendBuffer.SendAsync(message, cancellationToken);
//        }

//        private void Close()
//        {
//            _receiveBuffer.SetReceiverClosed();
//            _sendBuffer.SetSenderClosed();
//        }

//        private void ThrowIfDisposed()
//        {
//            if (_state >= WebSocketState.Closed) // or Aborted
//            {
//                throw new ObjectDisposedException(typeof(TestWebSocket).FullName);
//            }
//        }

//        private void ThrowIfOutputClosed()
//        {
//            if (State == WebSocketState.CloseSent)
//            {
//                throw new InvalidOperationException("Close already sent.");
//            }
//        }

//        private void ThrowIfInputClosed()
//        {
//            if (State == WebSocketState.CloseReceived)
//            {
//                throw new InvalidOperationException("Close already received.");
//            }
//        }

//        private void ValidateSegment(ArraySegment<byte> buffer)
//        {
//            if (buffer.Array == null)
//            {
//                throw new ArgumentNullException(nameof(buffer));
//            }
//            if (buffer.Offset < 0 || buffer.Offset > buffer.Array.Length)
//            {
//                throw new ArgumentOutOfRangeException(nameof(buffer.Offset), buffer.Offset, string.Empty);
//            }
//            if (buffer.Count < 0 || buffer.Count > buffer.Array.Length - buffer.Offset)
//            {
//                throw new ArgumentOutOfRangeException(nameof(buffer.Count), buffer.Count, string.Empty);
//            }
//        }

//        private TestWebSocket(string subProtocol, ReceiverSenderBuffer readBuffer, ReceiverSenderBuffer writeBuffer)
//        {
//            _state = WebSocketState.Open;
//            _subProtocol = subProtocol;
//            _receiveBuffer = readBuffer;
//            _sendBuffer = writeBuffer;
//        }

//        private class Message
//        {
//            public Message(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken token)
//            {
//                Buffer = buffer;
//                CloseStatus = null;
//                CloseStatusDescription = null;
//                EndOfMessage = endOfMessage;
//                MessageType = messageType;
//            }

//            public Message(WebSocketCloseStatus? closeStatus, string closeStatusDescription)
//            {
//                Buffer = new ArraySegment<byte>(new byte[0]);
//                CloseStatus = closeStatus;
//                CloseStatusDescription = closeStatusDescription;
//                MessageType = WebSocketMessageType.Close;
//                EndOfMessage = true;
//            }

//            public WebSocketCloseStatus? CloseStatus { get; set; }
//            public string CloseStatusDescription { get; set; }
//            public ArraySegment<byte> Buffer { get; set; }
//            public bool EndOfMessage { get; set; }
//            public WebSocketMessageType MessageType { get; set; }
//        }

//        private class ReceiverSenderBuffer
//        {
//            private bool _receiverClosed;
//            private bool _senderClosed;
//            private bool _disposed;
//            private SemaphoreSlim _sem;
//            private Queue<Message> _messageQueue;

//            public ReceiverSenderBuffer()
//            {
//                _sem = new SemaphoreSlim(0);
//                _messageQueue = new Queue<Message>();
//            }

//            public async virtual Task<Message> ReceiveAsync(CancellationToken cancellationToken)
//            {
//                if (_disposed)
//                {
//                    ThrowNoReceive();
//                }
//                await _sem.WaitAsync(cancellationToken);
//                lock (_messageQueue)
//                {
//                    if (_messageQueue.Count == 0)
//                    {
//                        _disposed = true;
//                        _sem.Dispose();
//                        ThrowNoReceive();
//                    }
//                    return _messageQueue.Dequeue();
//                }
//            }

//            public virtual Task SendAsync(Message message, CancellationToken cancellationToken)
//            {
//                lock (_messageQueue)
//                {
//                    if (_senderClosed)
//                    {
//                        throw new ObjectDisposedException(typeof(TestWebSocket).FullName);
//                    }
//                    if (_receiverClosed)
//                    {
//                        throw new IOException("The remote end closed the connection.", new ObjectDisposedException(typeof(TestWebSocket).FullName));
//                    }

//                    // we return immediately so we need to copy the buffer since the sender can re-use it
//                    var array = new byte[message.Buffer.Count];
//                    Array.Copy(message.Buffer.Array, message.Buffer.Offset, array, 0, message.Buffer.Count);
//                    message.Buffer = new ArraySegment<byte>(array);

//                    _messageQueue.Enqueue(message);
//                    _sem.Release();

//                    return Task.FromResult(true);
//                }
//            }

//            public void SetReceiverClosed()
//            {
//                lock (_messageQueue)
//                {
//                    if (!_receiverClosed)
//                    {
//                        _receiverClosed = true;
//                        if (!_disposed)
//                        {
//                            _sem.Release();
//                        }
//                    }
//                }
//            }

//            public void SetSenderClosed()
//            {
//                lock (_messageQueue)
//                {
//                    if (!_senderClosed)
//                    {
//                        _senderClosed = true;
//                        if (!_disposed)
//                        {
//                            _sem.Release();
//                        }
//                    }
//                }
//            }

//            private void ThrowNoReceive()
//            {
//                if (_receiverClosed)
//                {
//                    throw new ObjectDisposedException(typeof(TestWebSocket).FullName);
//                }
//                else // _senderClosed
//                {
//                    throw new IOException("The remote end closed the connection.", new ObjectDisposedException(typeof(TestWebSocket).FullName));
//                }
//            }
//        }
//    }

//    /// <summary>
//    /// Used to construct a HttpRequestMessage object.
//    /// </summary>
//    public class RequestBuilder
//    {
//        private readonly HttpRequestMessage _req;

//        /// <summary>
//        /// Construct a new HttpRequestMessage with the given path.
//        /// </summary>
//        /// <param name="server"></param>
//        /// <param name="path"></param>
//        public RequestBuilder(TestServer server, string path)
//        {
//            TestServer = server ?? throw new ArgumentNullException(nameof(server));
//            _req = new HttpRequestMessage(HttpMethod.Get, path);
//        }

//        /// <summary>
//        /// Gets the <see cref="TestServer"/> instance for which the request is being built.
//        /// </summary>
//        public TestServer TestServer { get; }

//        /// <summary>
//        /// Configure any HttpRequestMessage properties.
//        /// </summary>
//        /// <param name="configure"></param>
//        /// <returns>This <see cref="RequestBuilder"/> for chaining.</returns>
//        public RequestBuilder And(Action<HttpRequestMessage> configure)
//        {
//            if (configure == null)
//            {
//                throw new ArgumentNullException(nameof(configure));
//            }

//            configure(_req);
//            return this;
//        }

//        /// <summary>
//        /// Add the given header and value to the request or request content.
//        /// </summary>
//        /// <param name="name"></param>
//        /// <param name="value"></param>
//        /// <returns>This <see cref="RequestBuilder"/> for chaining.</returns>
//        public RequestBuilder AddHeader(string name, string value)
//        {
//            if (!_req.Headers.TryAddWithoutValidation(name, value))
//            {
//                if (_req.Content == null)
//                {
//                    _req.Content = new StreamContent(Stream.Null);
//                }
//                if (!_req.Content.Headers.TryAddWithoutValidation(name, value))
//                {
//                    // TODO: throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.InvalidHeaderName, name), "name");
//                    throw new ArgumentException("Invalid header name: " + name, "name");
//                }
//            }
//            return this;
//        }

//        /// <summary>
//        /// Set the request method and start processing the request.
//        /// </summary>
//        /// <param name="method"></param>
//        /// <returns>The resulting <see cref="HttpResponseMessage"/>.</returns>
//        public Task<HttpResponseMessage> SendAsync(string method)
//        {
//            _req.Method = new HttpMethod(method);
//            return TestServer.CreateClient().SendAsync(_req);
//        }

//        /// <summary>
//        /// Set the request method to GET and start processing the request.
//        /// </summary>
//        /// <returns>The resulting <see cref="HttpResponseMessage"/>.</returns>
//        public Task<HttpResponseMessage> GetAsync()
//        {
//            _req.Method = HttpMethod.Get;
//            return TestServer.CreateClient().SendAsync(_req);
//        }

//        /// <summary>
//        /// Set the request method to POST and start processing the request.
//        /// </summary>
//        /// <returns>The resulting <see cref="HttpResponseMessage"/>.</returns>
//        public Task<HttpResponseMessage> PostAsync()
//        {
//            _req.Method = HttpMethod.Post;
//            return TestServer.CreateClient().SendAsync(_req);
//        }
//    }

//    internal class ResponseTrailersFeature : IHttpResponseTrailersFeature
//    {
//        public IHeaderDictionary Trailers { get; set; } = new HeaderDictionary();
//    }

//    internal class HttpContextBuilder : IHttpBodyControlFeature, IHttpResetFeature
//    {
//        private readonly ApplicationWrapper _application;
//        private readonly bool _preserveExecutionContext;
//        private readonly HttpContext _httpContext;

//        private readonly TaskCompletionSource<HttpContext> _responseTcs = new TaskCompletionSource<HttpContext>(TaskCreationOptions.RunContinuationsAsynchronously);
//        private readonly ResponseBodyReaderStream _responseReaderStream;
//        private readonly ResponseBodyPipeWriter _responsePipeWriter;
//        private readonly ResponseFeature _responseFeature;
//        private readonly RequestLifetimeFeature _requestLifetimeFeature;
//        private readonly ResponseTrailersFeature _responseTrailersFeature = new ResponseTrailersFeature();
//        private bool _pipelineFinished;
//        private bool _returningResponse;
//        private object _testContext;
//        private Pipe _requestPipe;

//        private Action<HttpContext> _responseReadCompleteCallback;
//        private Task _sendRequestStreamTask;

//        internal HttpContextBuilder(ApplicationWrapper application, bool allowSynchronousIO, bool preserveExecutionContext)
//        {
//            _application = application ?? throw new ArgumentNullException(nameof(application));
//            AllowSynchronousIO = allowSynchronousIO;
//            _preserveExecutionContext = preserveExecutionContext;
//            _httpContext = new DefaultHttpContext();
//            _responseFeature = new ResponseFeature(Abort);
//            _requestLifetimeFeature = new RequestLifetimeFeature(Abort);

//            var request = _httpContext.Request;
//            request.Protocol = "HTTP/1.1";
//            request.Method = HttpMethods.Get;

//            _requestPipe = new Pipe();

//            var responsePipe = new Pipe();
//            _responseReaderStream = new ResponseBodyReaderStream(responsePipe, ClientInitiatedAbort, () => _responseReadCompleteCallback?.Invoke(_httpContext));
//            _responsePipeWriter = new ResponseBodyPipeWriter(responsePipe, ReturnResponseMessageAsync);
//            _responseFeature.Body = new ResponseBodyWriterStream(_responsePipeWriter, () => AllowSynchronousIO);
//            _responseFeature.BodyWriter = _responsePipeWriter;

//            _httpContext.Features.Set<IHttpBodyControlFeature>(this);
//            _httpContext.Features.Set<IHttpResponseFeature>(_responseFeature);
//            _httpContext.Features.Set<IHttpResponseBodyFeature>(_responseFeature);
//            _httpContext.Features.Set<IHttpRequestLifetimeFeature>(_requestLifetimeFeature);
//            _httpContext.Features.Set<IHttpResponseTrailersFeature>(_responseTrailersFeature);
//        }

//        public bool AllowSynchronousIO { get; set; }

//        internal void Configure(Action<HttpContext, PipeReader> configureContext)
//        {
//            if (configureContext == null)
//            {
//                throw new ArgumentNullException(nameof(configureContext));
//            }

//            configureContext(_httpContext, _requestPipe.Reader);
//        }

//        internal void SendRequestStream(Func<PipeWriter, Task> sendRequestStream)
//        {
//            if (sendRequestStream == null)
//            {
//                throw new ArgumentNullException(nameof(sendRequestStream));
//            }

//            _sendRequestStreamTask = sendRequestStream(_requestPipe.Writer);
//        }

//        internal void RegisterResponseReadCompleteCallback(Action<HttpContext> responseReadCompleteCallback)
//        {
//            _responseReadCompleteCallback = responseReadCompleteCallback;
//        }

//        /// <summary>
//        /// Start processing the request.
//        /// </summary>
//        /// <returns></returns>
//        internal Task<HttpContext> SendAsync(CancellationToken cancellationToken)
//        {
//            var registration = cancellationToken.Register(ClientInitiatedAbort);

//            // Everything inside this function happens in the SERVER's execution context (unless PreserveExecutionContext is true)
//            async Task RunRequestAsync()
//            {
//                // HTTP/2 specific features must be added after the request has been configured.
//                if (string.Equals("HTTP/2", _httpContext.Request.Protocol, StringComparison.OrdinalIgnoreCase))
//                {
//                    _httpContext.Features.Set<IHttpResetFeature>(this);
//                }

//                // This will configure IHttpContextAccessor so it needs to happen INSIDE this function,
//                // since we are now inside the Server's execution context. If it happens outside this cont
//                // it will be lost when we abandon the execution context.
//                _testContext = _application.CreateContext(_httpContext.Features);
//                try
//                {
//                    await _application.ProcessRequestAsync(_testContext);
//                    await CompleteRequestAsync();
//                    await CompleteResponseAsync();
//                    _application.DisposeContext(_testContext, exception: null);
//                }
//                catch (Exception ex)
//                {
//                    Abort(ex);
//                    _application.DisposeContext(_testContext, ex);
//                }
//                finally
//                {
//                    registration.Dispose();
//                }
//            }

//            // Async offload, don't let the test code block the caller.
//            if (_preserveExecutionContext)
//            {
//                _ = Task.Factory.StartNew(RunRequestAsync);
//            }
//            else
//            {
//                ThreadPool.UnsafeQueueUserWorkItem(_ =>
//                {
//                    _ = RunRequestAsync();
//                }, null);
//            }

//            return _responseTcs.Task;
//        }

//        // Triggered by request CancellationToken canceling or response stream Disposal.
//        internal void ClientInitiatedAbort()
//        {
//            if (!_pipelineFinished)
//            {
//                // We don't want to trigger the token for already completed responses.
//                _requestLifetimeFeature.Cancel();
//            }

//            // Writes will still succeed, the app will only get an error if they check the CT.
//            _responseReaderStream.Abort(new IOException("The client aborted the request."));

//            // Cancel any pending request async activity when the client aborts a duplex
//            // streaming scenario by disposing the HttpResponseMessage.
//            CancelRequestBody();
//        }

//        private async Task CompleteRequestAsync()
//        {
//            if (!_requestPipe.Reader.TryRead(out var resultKind) || !resultKind.IsCompleted)
//            {
//                // If request is still in progress then abort it.
//                CancelRequestBody();
//            }
//            else
//            {
//                // Writer was already completed in send request callback.
//                await _requestPipe.Reader.CompleteAsync();
//            }

//            if (_sendRequestStreamTask != null)
//            {
//                try
//                {
//                    // Ensure duplex request is either completely read or has been aborted.
//                    await _sendRequestStreamTask;
//                }
//                catch (OperationCanceledException)
//                {
//                    // Request was canceled, likely because it wasn't read before the request ended.
//                }
//            }
//        }

//        internal async Task CompleteResponseAsync()
//        {
//            _pipelineFinished = true;
//            await ReturnResponseMessageAsync();
//            _responsePipeWriter.Complete();
//            await _responseFeature.FireOnResponseCompletedAsync();
//        }

//        internal async Task ReturnResponseMessageAsync()
//        {
//            // Check if the response is already returning because the TrySetResult below could happen a bit late
//            // (as it happens on a different thread) by which point the CompleteResponseAsync could run and calls this
//            // method again.
//            if (!_returningResponse)
//            {
//                _returningResponse = true;

//                try
//                {
//                    await _responseFeature.FireOnSendingHeadersAsync();
//                }
//                catch (Exception ex)
//                {
//                    Abort(ex);
//                    return;
//                }

//                // Copy the feature collection so we're not multi-threading on the same collection.
//                var newFeatures = new FeatureCollection();
//                foreach (var pair in _httpContext.Features)
//                {
//                    newFeatures[pair.Key] = pair.Value;
//                }
//                var serverResponseFeature = _httpContext.Features.Get<IHttpResponseFeature>();
//                // The client gets a deep copy of this so they can interact with the body stream independently of the server.
//                var clientResponseFeature = new HttpResponseFeature()
//                {
//                    StatusCode = serverResponseFeature.StatusCode,
//                    ReasonPhrase = serverResponseFeature.ReasonPhrase,
//                    Headers = serverResponseFeature.Headers,
//                    Body = _responseReaderStream
//                };
//                newFeatures.Set<IHttpResponseFeature>(clientResponseFeature);
//                newFeatures.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(_responseReaderStream));
//                _responseTcs.TrySetResult(new DefaultHttpContext(newFeatures));
//            }
//        }

//        internal void Abort(Exception exception)
//        {
//            _responsePipeWriter.Abort(exception);
//            _responseReaderStream.Abort(exception);
//            _requestLifetimeFeature.Cancel();
//            _responseTcs.TrySetException(exception);
//            CancelRequestBody();
//        }

//        private void CancelRequestBody()
//        {
//            _requestPipe.Writer.CancelPendingFlush();
//            _requestPipe.Reader.CancelPendingRead();
//        }

//        void IHttpResetFeature.Reset(int errorCode)
//        {
//            Abort(new HttpResetTestException(errorCode));
//        }
//    }

//    /// <summary>
//    /// Used to surface to the test client that the application invoked <see cref="IHttpResetFeature.Reset"/>
//    /// </summary>
//    public class HttpResetTestException : Exception
//    {
//        /// <summary>
//        /// Creates a new test exception
//        /// </summary>
//        /// <param name="errorCode">The error code passed to <see cref="IHttpResetFeature.Reset"/></param>
//        public HttpResetTestException(int errorCode)
//            : base($"The application reset the request with error code {errorCode}.")
//        {
//            ErrorCode = errorCode;
//        }

//        /// <summary>
//        /// The error code passed to <see cref="IHttpResetFeature.Reset"/>
//        /// </summary>
//        public int ErrorCode { get; }
//    }

//    internal class AsyncStreamWrapper : Stream
//    {
//        private Stream _inner;
//        private Func<bool> _allowSynchronousIO;

//        internal AsyncStreamWrapper(Stream inner, Func<bool> allowSynchronousIO)
//        {
//            _inner = inner;
//            _allowSynchronousIO = allowSynchronousIO;
//        }

//        public override bool CanRead => _inner.CanRead;

//        public override bool CanSeek => false;

//        public override bool CanWrite => _inner.CanWrite;

//        public override long Length => throw new NotSupportedException("The stream is not seekable.");

//        public override long Position
//        {
//            get => throw new NotSupportedException("The stream is not seekable.");
//            set => throw new NotSupportedException("The stream is not seekable.");
//        }

//        public override void Flush()
//        {
//            // Not blocking Flush because things like StreamWriter.Dispose() always call it.
//            _inner.Flush();
//        }

//        public override Task FlushAsync(CancellationToken cancellationToken)
//        {
//            return _inner.FlushAsync(cancellationToken);
//        }

//        public override int Read(byte[] buffer, int offset, int count)
//        {
//            if (!_allowSynchronousIO())
//            {
//                throw new InvalidOperationException("Synchronous operations are disallowed. Call ReadAsync or set AllowSynchronousIO to true.");
//            }

//            return _inner.Read(buffer, offset, count);
//        }

//        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
//        {
//            return _inner.ReadAsync(buffer, offset, count, cancellationToken);
//        }

//        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
//        {
//            return _inner.ReadAsync(buffer, cancellationToken);
//        }

//        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
//        {
//            return _inner.BeginRead(buffer, offset, count, callback, state);
//        }

//        public override int EndRead(IAsyncResult asyncResult)
//        {
//            return _inner.EndRead(asyncResult);
//        }

//        public override long Seek(long offset, SeekOrigin origin)
//        {
//            throw new NotSupportedException("The stream is not seekable.");
//        }

//        public override void SetLength(long value)
//        {
//            throw new NotSupportedException("The stream is not seekable.");
//        }

//        public override void Write(byte[] buffer, int offset, int count)
//        {
//            if (!_allowSynchronousIO())
//            {
//                throw new InvalidOperationException("Synchronous operations are disallowed. Call WriteAsync or set AllowSynchronousIO to true.");
//            }

//            _inner.Write(buffer, offset, count);
//        }

//        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
//        {
//            return _inner.BeginWrite(buffer, offset, count, callback, state);
//        }

//        public override void EndWrite(IAsyncResult asyncResult)
//        {
//            _inner.EndWrite(asyncResult);
//        }

//        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
//        {
//            return _inner.WriteAsync(buffer, offset, count, cancellationToken);
//        }

//        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
//        {
//            return _inner.WriteAsync(buffer, cancellationToken);
//        }

//        public override void Close()
//        {
//            // Don't dispose the inner stream, we don't want to impact the client stream
//        }

//        protected override void Dispose(bool disposing)
//        {
//            // Don't dispose the inner stream, we don't want to impact the client stream
//        }

//        public override ValueTask DisposeAsync()
//        {
//            // Don't dispose the inner stream, we don't want to impact the client stream
//            return default;
//        }
//    }

//    /// <summary>
//    /// The client's view of the response body.
//    /// </summary>
//    internal class ResponseBodyReaderStream : Stream
//    {
//        private bool _readerComplete;
//        private bool _aborted;
//        private Exception _abortException;

//        private readonly Action _abortRequest;
//        private readonly Action _readComplete;
//        private readonly Pipe _pipe;

//        internal ResponseBodyReaderStream(Pipe pipe, Action abortRequest, Action readComplete)
//        {
//            _pipe = pipe ?? throw new ArgumentNullException(nameof(pipe));
//            _abortRequest = abortRequest ?? throw new ArgumentNullException(nameof(abortRequest));
//            _readComplete = readComplete;
//        }

//        public override bool CanRead => true;

//        public override bool CanSeek => false;

//        public override bool CanWrite => false;

//        #region NotSupported

//        public override long Length => throw new NotSupportedException();

//        public override long Position
//        {
//            get => throw new NotSupportedException();
//            set => throw new NotSupportedException();
//        }

//        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

//        public override void SetLength(long value) => throw new NotSupportedException();

//        public override void Flush() => throw new NotSupportedException();

//        public override Task FlushAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

//        // Write with count 0 will still trigger OnFirstWrite
//        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

//        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new NotSupportedException();

//        #endregion NotSupported

//        public override int Read(byte[] buffer, int offset, int count)
//        {
//            return ReadAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();
//        }

//        public async override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
//        {
//            VerifyBuffer(buffer, offset, count);
//            CheckAborted();

//            if (_readerComplete)
//            {
//                return 0;
//            }

//            using var registration = cancellationToken.Register(Cancel);
//            var resultKind = await _pipe.Reader.ReadAsync(cancellationToken);

//            if (resultKind.IsCanceled)
//            {
//                throw new OperationCanceledException();
//            }

//            if (resultKind.Buffer.IsEmpty && resultKind.IsCompleted)
//            {
//                _readComplete();
//                _readerComplete = true;
//                return 0;
//            }

//            var readableBuffer = resultKind.Buffer;
//            var actual = Math.Min(readableBuffer.Length, count);
//            readableBuffer = readableBuffer.Slice(0, actual);
//            readableBuffer.CopyTo(new Span<byte>(buffer, offset, count));
//            _pipe.Reader.AdvanceTo(readableBuffer.End);
//            return (int)actual;
//        }

//        private static void VerifyBuffer(byte[] buffer, int offset, int count)
//        {
//            if (buffer == null)
//            {
//                throw new ArgumentNullException("buffer");
//            }
//            if (offset < 0 || offset > buffer.Length)
//            {
//                throw new ArgumentOutOfRangeException("offset", offset, string.Empty);
//            }
//            if (count <= 0 || count > buffer.Length - offset)
//            {
//                throw new ArgumentOutOfRangeException("count", count, string.Empty);
//            }
//        }

//        internal void Cancel()
//        {
//            Abort(new OperationCanceledException());
//        }

//        internal void Abort(Exception innerException)
//        {
//            Contract.Requires(innerException != null);
//            _aborted = true;
//            _abortException = innerException;
//            _pipe.Reader.CancelPendingRead();
//        }

//        private void CheckAborted()
//        {
//            if (_aborted)
//            {
//                throw new IOException(string.Empty, _abortException);
//            }
//        }

//        protected override void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                _abortRequest();
//            }

//            _pipe.Reader.Complete();

//            base.Dispose(disposing);
//        }
//    }

//    internal class ResponseBodyPipeWriter : PipeWriter
//    {
//        private readonly Func<Task> _onFirstWriteAsync;
//        private readonly Pipe _pipe;

//        private bool _firstWrite;
//        private bool _complete;

//        internal ResponseBodyPipeWriter(Pipe pipe, Func<Task> onFirstWriteAsync)
//        {
//            _pipe = pipe ?? throw new ArgumentNullException(nameof(pipe));
//            _onFirstWriteAsync = onFirstWriteAsync ?? throw new ArgumentNullException(nameof(onFirstWriteAsync));
//            _firstWrite = true;
//        }

//        public override async ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken)
//        {
//            cancellationToken.ThrowIfCancellationRequested();
//            CheckNotComplete();

//            await FirstWriteAsync();
//            return await _pipe.Writer.FlushAsync(cancellationToken);
//        }

//        private Task FirstWriteAsync()
//        {
//            if (_firstWrite)
//            {
//                _firstWrite = false;
//                return _onFirstWriteAsync();
//            }
//            return Task.CompletedTask;
//        }

//        internal void Abort(Exception innerException)
//        {
//            Contract.Requires(innerException != null);
//            _complete = true;
//            _pipe.Writer.Complete(new IOException(string.Empty, innerException));
//        }

//        internal void Complete()
//        {
//            if (_complete)
//            {
//                return;
//            }

//            // Throw for further writes, but not reads. Allow reads to drain the buffered data and then return 0 for further reads.
//            _complete = true;
//            _pipe.Writer.Complete();
//        }

//        private void CheckNotComplete()
//        {
//            if (_complete)
//            {
//                throw new IOException("The request was aborted or the pipeline has finished.");
//            }
//        }

//        public override void Complete(Exception exception = null)
//        {
//            // No-op in the non-error case
//            if (exception != null)
//            {
//                Abort(exception);
//            }
//        }

//        public override void CancelPendingFlush() => _pipe.Writer.CancelPendingFlush();

//        public override void Advance(int bytes)
//        {
//            CheckNotComplete();
//            _pipe.Writer.Advance(bytes);
//        }

//        public override Memory<byte> GetMemory(int sizeHint = 0)
//        {
//            CheckNotComplete();
//            return _pipe.Writer.GetMemory(sizeHint);
//        }

//        public override Span<byte> GetSpan(int sizeHint = 0)
//        {
//            CheckNotComplete();
//            return _pipe.Writer.GetSpan(sizeHint);
//        }
//    }

//    internal class ResponseFeature : IHttpResponseFeature, IHttpResponseBodyFeature
//    {
//        private readonly HeaderDictionary _headers = new HeaderDictionary();
//        private readonly Action<Exception> _abort;

//        private Func<Task> _responseStartingAsync = () => Task.FromResult(true);
//        private Func<Task> _responseCompletedAsync = () => Task.FromResult(true);
//        private int _statusCode;
//        private string _reasonPhrase;

//        public ResponseFeature(Action<Exception> abort)
//        {
//            Headers = _headers;

//            // 200 is the default status code all the way down to the host, so we set it
//            // here to be consistent with the rest of the hosts when writing tests.
//            StatusCode = 200;
//            _abort = abort;
//        }

//        public int StatusCode
//        {
//            get => _statusCode;
//            set
//            {
//                if (HasStarted)
//                {
//                    throw new InvalidOperationException("The status code cannot be set, the response has already started.");
//                }
//                if (value < 100)
//                {
//                    throw new ArgumentOutOfRangeException(nameof(value), value, "The status code cannot be set to a value less than 100");
//                }

//                _statusCode = value;
//            }
//        }

//        public string ReasonPhrase
//        {
//            get => _reasonPhrase;
//            set
//            {
//                if (HasStarted)
//                {
//                    throw new InvalidOperationException("The reason phrase cannot be set, the response has already started.");
//                }

//                _reasonPhrase = value;
//            }
//        }

//        public IHeaderDictionary Headers { get; set; }

//        public Stream Body { get; set; }

//        public Stream Stream => Body;

//        internal PipeWriter BodyWriter { get; set; }

//        public PipeWriter Writer => BodyWriter;

//        public bool HasStarted { get; set; }

//        public void OnStarting(Func<object, Task> callback, object state)
//        {
//            if (HasStarted)
//            {
//                throw new InvalidOperationException();
//            }

//            var prior = _responseStartingAsync;
//            _responseStartingAsync = async () =>
//            {
//                await callback(state);
//                await prior();
//            };
//        }

//        public void OnCompleted(Func<object, Task> callback, object state)
//        {
//            var prior = _responseCompletedAsync;
//            _responseCompletedAsync = async () =>
//            {
//                try
//                {
//                    await callback(state);
//                }
//                finally
//                {
//                    await prior();
//                }
//            };
//        }

//        public async Task FireOnSendingHeadersAsync()
//        {
//            if (!HasStarted)
//            {
//                try
//                {
//                    await _responseStartingAsync();
//                }
//                finally
//                {
//                    HasStarted = true;
//                    _headers.IsReadOnly = true;
//                }
//            }
//        }

//        public Task FireOnResponseCompletedAsync()
//        {
//            return _responseCompletedAsync();
//        }

//        public async Task StartAsync(CancellationToken token = default)
//        {
//            try
//            {
//                await FireOnSendingHeadersAsync();
//            }
//            catch (Exception ex)
//            {
//                _abort(ex);
//                throw;
//            }
//        }

//        public void DisableBuffering()
//        {
//        }

//        public Task SendFileAsync(string path, long offset, long? count, CancellationToken cancellation)
//        {
//            return SendFileFallback.SendFileAsync(Stream, path, offset, count, cancellation);
//        }

//        public Task CompleteAsync()
//        {
//            return Writer.CompleteAsync().AsTask();
//        }
//    }

//    internal class RequestLifetimeFeature : IHttpRequestLifetimeFeature
//    {
//        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
//        private readonly Action<Exception> _abort;

//        public RequestLifetimeFeature(Action<Exception> abort)
//        {
//            RequestAborted = _cancellationTokenSource.Token;
//            _abort = abort;
//        }

//        public CancellationToken RequestAborted { get; set; }

//        internal void Cancel()
//        {
//            _cancellationTokenSource.Cancel();
//        }

//        void IHttpRequestLifetimeFeature.Abort()
//        {
//            _abort(new Exception("The application aborted the request."));
//            _cancellationTokenSource.Cancel();
//        }
//    }

//    internal class ResponseBodyWriterStream : Stream
//    {
//        private readonly ResponseBodyPipeWriter _responseWriter;
//        private readonly Func<bool> _allowSynchronousIO;

//        public ResponseBodyWriterStream(ResponseBodyPipeWriter responseWriter, Func<bool> allowSynchronousIO)
//        {
//            _responseWriter = responseWriter;
//            _allowSynchronousIO = allowSynchronousIO;
//        }

//        public override bool CanRead => false;

//        public override bool CanSeek => false;

//        public override bool CanWrite => true;

//        public override long Length => throw new NotSupportedException();

//        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

//        public override int Read(byte[] buffer, int offset, int count)
//        {
//            throw new NotSupportedException();
//        }

//        public override long Seek(long offset, SeekOrigin origin)
//        {
//            throw new NotSupportedException();
//        }

//        public override void SetLength(long value)
//        {
//            throw new NotSupportedException();
//        }

//        public override void Flush()
//        {
//            FlushAsync().GetAwaiter().GetResult();
//        }

//        public override async Task FlushAsync(CancellationToken cancellationToken)
//        {
//            await _responseWriter.FlushAsync(cancellationToken);
//        }

//        public override void Write(byte[] buffer, int offset, int count)
//        {
//            if (!_allowSynchronousIO())
//            {
//                throw new InvalidOperationException("Synchronous operations are disallowed. Call WriteAsync or set AllowSynchronousIO to true.");
//            }

//            // The Pipe Write method requires calling FlushAsync to notify the reader. Call WriteAsync instead.
//            WriteAsync(buffer, offset, count).GetAwaiter().GetResult();
//        }

//        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
//        {
//            await _responseWriter.WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken);
//        }
//    }
//}