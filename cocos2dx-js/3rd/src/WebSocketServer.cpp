#include "WebSocketServer.h"
#include "base/CCDirector.h"
#include "base/CCScheduler.h"
#include "base/CCEventDispatcher.h"
#include "base/CCEventListenerCustom.h"

#include <thread>
#include <mutex>
#include <queue>
#include <list>
#include <signal.h>
#include <errno.h>

#include "libwebsockets.h"

#define WS_RX_BUFFER_SIZE (65536)
#define WS_RESERVE_RECEIVE_BUFFER_SIZE (4096)

#define  LOG_TAG    "WebSocketServer.cpp"

// Since CCLOG isn't thread safe, we uses LOGD for multi-thread logging.
#if COCOS2D_DEBUG > 0
    #ifdef ANDROID
        #define  LOGD(...)  __android_log_print(ANDROID_LOG_DEBUG, LOG_TAG,__VA_ARGS__)
    #else
        #define  LOGD(...) printf(__VA_ARGS__)
    #endif
#else
    #define  LOGD(...)
#endif

static void printWebSocketServerLog(int level, const char *line)
{
#if COCOS2D_DEBUG > 0
    static const char * const log_level_names[] = {
        "ERR",
        "WARN",
        "NOTICE",
        "INFO",
        "DEBUG",
        "PARSER",
        "HEADER",
        "EXTENSION",
        "CLIENT",
        "LATENCY",
    };

    char buf[30] = {0};
    int n;

    for (n = 0; n < LLL_COUNT; n++) {
        if (level != (1 << n))
            continue;
        sprintf(buf, "%s: ", log_level_names[n]);
        break;
    }

#ifdef ANDROID
    __android_log_print(ANDROID_LOG_DEBUG, "libwebsockets", "%s%s", buf, line);
#else
    printf("%s%s\n", buf, line);
#endif

#endif // #if COCOS2D_DEBUG > 0
}

NS_CC_BEGIN

namespace network {

class WsServerMessage
{
public:
    WsServerMessage() : id(++__id), what(0), obj(nullptr){}
    unsigned int id;
    unsigned int what; // message type
    void* obj;

private:
    static unsigned int __id;
};

unsigned int WsServerMessage::__id = 0;

/**
 *  @brief Websocket thread helper, it's used for sending message between UI thread and websocket thread.
 */

struct Connection
{
    struct lws* wsInstance;
    std::list<WsServerMessage*>* messagesQueue;
};

class WsServerThreadHelper
{
public:
    WsServerThreadHelper();
    ~WsServerThreadHelper();

    // Creates a new thread
    bool createWebSocketServerThread(const WebSocketServer& ws);
    // Quits websocket thread.
    void quitWebSocketServerThread();

    // Sends message to Cocos thread. It's needed to be invoked in Websocket thread.
    void sendMessageToCocosThread(const std::function<void()>& cb);

    // Sends message to Websocket thread. It's needs to be invoked in Cocos thread.
    void sendMessageToWebSocketServerThread(int socketId,WsServerMessage *msg);

    // FIXME add broadcast send

    void closeAllConnections();

    // Waits the sub-thread (websocket thread) to exit,
    void joinWebSocketServerThread();

protected:
    void wsThreadEntryFunc();
private:
    std::map<int,Connection*> _subThreadWsServerConnections;
    std::mutex   _subThreadWsServerConnectionsMutex;
    std::thread* _subThreadInstance;
    WebSocketServer* _ws;
    bool _needQuit;
    friend class WebSocketServer;
};



// Wrapper for converting websocket callback from static function to member function of WebSocketServer class.
class WebSocketServerCallbackWrapper {
public:

    static int onSocketCallback(struct lws *wsi, enum lws_callback_reasons reason, void *user, void *in, size_t len)
    {
        // Gets the user data from context. We know that it's a 'WebSocketServer' instance.
        if (wsi == nullptr) {
            return 0;
        }

        lws_context* context = lws_get_context(wsi);
        WebSocketServer* wsInstance = (WebSocketServer*)lws_context_user(context);
        
        if (wsInstance)
        {
            return wsInstance->onSocketCallback(wsi, reason, user, in, len);
        }
        return 0;
    }
};

// Implementation of WsServerThreadHelper
WsServerThreadHelper::WsServerThreadHelper()
: _subThreadInstance(nullptr)
, _ws(nullptr)
, _needQuit(false)
{
    std::lock_guard<std::mutex> lk(_subThreadWsServerConnectionsMutex);
}

WsServerThreadHelper::~WsServerThreadHelper()
{
    joinWebSocketServerThread();
    closeAllConnections();
    CC_SAFE_DELETE(_subThreadInstance);
}

bool WsServerThreadHelper::createWebSocketServerThread(const WebSocketServer& ws)
{
    _ws = const_cast<WebSocketServer*>(&ws);

    // Creates websocket thread
    _subThreadInstance = new (std::nothrow) std::thread(&WsServerThreadHelper::wsThreadEntryFunc, this);
    return true;
}

void WsServerThreadHelper::quitWebSocketServerThread()
{
    _needQuit = true;
}

void WsServerThreadHelper::wsThreadEntryFunc()
{
    LOGD("WebSocketServer thread start, helper instance: %p\n", this);
    _ws->onSubThreadStarted();

    while (!_needQuit)
    {
        _ws->onSubThreadLoop();
    }

    _ws->onSubThreadEnded();

    LOGD("WebSocketServer thread exit, helper instance: %p\n", this);
}

void WsServerThreadHelper::sendMessageToCocosThread(const std::function<void()>& cb)
{
    Director::getInstance()->getScheduler()->performFunctionInCocosThread(cb);
}

void WsServerThreadHelper::sendMessageToWebSocketServerThread(int socketId, WsServerMessage *msg)
{
    _subThreadWsServerConnections[socketId]->messagesQueue->push_back(msg);
}

void WsServerThreadHelper::joinWebSocketServerThread()
{
    if (_subThreadInstance->joinable())
    {
        _subThreadInstance->join();
    }
}

void WsServerThreadHelper::closeAllConnections()
{
    _subThreadWsServerConnectionsMutex.lock();
    _subThreadWsServerConnections.clear();
    _subThreadWsServerConnectionsMutex.unlock();
}


// Define a WebSocketServer frame
class WebSocketServerFrame
{
public:
    WebSocketServerFrame()
        : _payload(nullptr)
        , _payloadLength(0)
        , _frameLength(0)
    {
    }

    bool init(unsigned char* buf, ssize_t len)
    {
        if (buf == nullptr && len > 0)
            return false;

        if (!_data.empty())
        {
            LOGD("WebSocketServerFrame was initialized, should not init it again!\n");
            return false;
        }

        _data.reserve(LWS_PRE + len);
        _data.resize(LWS_PRE, 0x00);
        if (len > 0)
        {
            _data.insert(_data.end(), buf, buf + len);
        }

        _payload = _data.data() + LWS_PRE;
        _payloadLength = len;
        _frameLength = len;
        return true;
    }

    void update(ssize_t issued)
    {
        _payloadLength -= issued;
        _payload += issued;
    }

    unsigned char* getPayload() const { return _payload; }
    ssize_t getPayloadLength() const { return _payloadLength; }
    ssize_t getFrameLength() const { return _frameLength; }
private:
    unsigned char* _payload;
    ssize_t _payloadLength;

    ssize_t _frameLength;
    std::vector<unsigned char> _data;
};
//


enum WS_MSG {
    WS_MSG_TO_SUBTRHEAD_SENDING_STRING = 0,
    WS_MSG_TO_SUBTRHEAD_SENDING_BINARY,
};

WebSocketServer::WebSocketServer()
: _readyState(State::STARTING)
, _port(80)
, _wsHelper(nullptr)
, _wsContext(nullptr)
, _isDestroyed(std::make_shared<std::atomic<bool>>(false))
, _delegate(nullptr)
, _SSLConnection(0)
, _wsProtocols(nullptr)
{
    // reserve data buffer to avoid allocate memory frequently
    _receivedData.reserve(WS_RESERVE_RECEIVE_BUFFER_SIZE);
    
    std::shared_ptr<std::atomic<bool>> isDestroyed = _isDestroyed;
    _resetDirectorListener = Director::getInstance()->getEventDispatcher()->addCustomEventListener(Director::EVENT_RESET, [this, isDestroyed](EventCustom*){
        if (*isDestroyed)
            return;
        stop();
    });
}

WebSocketServer::~WebSocketServer()
{
    LOGD("In the destructor of WebSocketServer (%p)\n", this);
    CC_SAFE_DELETE(_wsHelper);

    if (_wsProtocols != nullptr)
    {
        for (int i = 0; _wsProtocols[i].callback != nullptr; ++i)
        {
            CC_SAFE_DELETE_ARRAY(_wsProtocols[i].name);
        }
    }
    CC_SAFE_DELETE_ARRAY(_wsProtocols);
    
    Director::getInstance()->getEventDispatcher()->removeEventListener(_resetDirectorListener);
    
    *_isDestroyed = true;
}

bool WebSocketServer::init(const Delegate& delegate,
                     const unsigned int port,
                     const std::vector<std::string>* protocols/* = nullptr*/)
{
    bool ret = false;
    bool useSSL = false;
    size_t pos = 0;

    _delegate = const_cast<Delegate*>(&delegate);


    _port = port;
    _SSLConnection = useSSL ? 1 : 0;

    LOGD("[WebSocketServer::init] _port: %d \n", _port);

    size_t protocolCount = 0;
    if (protocols && protocols->size() > 0)
    {
        protocolCount = protocols->size();
    }
    else
    {
        protocolCount = 1;
    }

    _wsProtocols = new (std::nothrow) lws_protocols[protocolCount+1];
    memset(_wsProtocols, 0, sizeof(lws_protocols)*(protocolCount+1));

    if (protocols && protocols->size() > 0)
    {
        int i = 0;
        for (std::vector<std::string>::const_iterator iter = protocols->begin(); iter != protocols->end(); ++iter, ++i)
        {
            char* name = new (std::nothrow) char[(*iter).length()+1];
            strcpy(name, (*iter).c_str());
            _wsProtocols[i].name = name;
            _wsProtocols[i].callback = WebSocketServerCallbackWrapper::onSocketCallback;
            _wsProtocols[i].rx_buffer_size = WS_RX_BUFFER_SIZE;
        }
    }
    else
    {
        char* name = new (std::nothrow) char[20];
        strcpy(name, "default-protocol");
        _wsProtocols[0].name = name;
        _wsProtocols[0].callback = WebSocketServerCallbackWrapper::onSocketCallback;
        _wsProtocols[0].rx_buffer_size = WS_RX_BUFFER_SIZE;
    }

    _wsHelper = new (std::nothrow) WsServerThreadHelper();
    ret = _wsHelper->createWebSocketServerThread(*this);

    return ret;
}

void WebSocketServer::send(int socketId, const std::string& message)
{
    if (_readyState == State::UP)
    {
        // In main thread

        Data* data = new (std::nothrow) Data();
        data->bytes = (char*)malloc(message.length() + 1);
        // Make sure the last byte is '\0'
        data->bytes[message.length()] = '\0';
        strcpy(data->bytes, message.c_str());
        data->len = static_cast<ssize_t>(message.length());

        WsServerMessage* msg = new (std::nothrow) WsServerMessage();
        msg->what = WS_MSG_TO_SUBTRHEAD_SENDING_STRING;
        msg->obj = data;
        _wsHelper->sendMessageToWebSocketServerThread(socketId,msg);
    }
    else
    {
        LOGD("Couldn't send message since websocketserver wasn't up!\n");
    }
}

void WebSocketServer::send(int socketId, const unsigned char* binaryMsg, unsigned int len)
{
    if (_readyState == State::UP)
    {
        // In main thread
        Data* data = new (std::nothrow) Data();
        if (len == 0)
        {
            // If data length is zero, allocate 1 byte for safe.
            data->bytes = (char*)malloc(1);
            data->bytes[0] = '\0';
        }
        else
        {
            data->bytes = (char*)malloc(len);
            memcpy((void*)data->bytes, (void*)binaryMsg, len);
        }
        data->len = len;

        WsServerMessage* msg = new (std::nothrow) WsServerMessage();
        msg->what = WS_MSG_TO_SUBTRHEAD_SENDING_BINARY;
        msg->obj = data;
        _wsHelper->sendMessageToWebSocketServerThread(socketId,msg);
    }
    else
    {
        LOGD("Couldn't send message since websocketserver wasn't up!\n");
    }
}

void WebSocketServer::broadcast(const std::string& message)
{
    for(auto const& ent1 : _wsHelper->_subThreadWsServerConnections) 
    {
        send(ent1.first, message);
    }
}


void WebSocketServer::broadcast(const unsigned char* binaryMsg, unsigned int len)
{
    for(auto const& ent1 : _wsHelper->_subThreadWsServerConnections) 
    {
        send(ent1.first, binaryMsg, len);
    }
}

void WebSocketServer::stop()
{
    _readStateMutex.lock();
    if (_readyState == State::DOWN)
    {
        LOGD("stop: WebSocketServer (%p) was stoped, no need to stop it again!\n", this);
        _readStateMutex.unlock();
        return;
    }

    _wsHelper->closeAllConnections();
    // Sets the state to 'stopd' to make sure 'onConnectionClosed' which is
    // invoked by websocket thread don't post 'close' message to Cocos thread since
    // WebSocketServer instance is destroyed at next frame.
    // 'closed' state has to be set before quit websocket thread.
    _readyState = State::DOWN;
    _readStateMutex.unlock();

    _wsHelper->quitWebSocketServerThread();
    LOGD("Waiting WebSocketServer (%p) to exit!\n", this);
    
    std::shared_ptr<std::atomic<bool>> isDestroyed = _isDestroyed;
    _wsHelper->sendMessageToCocosThread([this, isDestroyed](){
        if (*isDestroyed)
        {
            LOGD("WebSocketServer instance was destroyed!\n");
        }
        else
        {
            _delegate->onServerDown(this);
        }
    });

    _wsHelper->joinWebSocketServerThread();
}

void WebSocketServer::stopAsync()
{
    _wsHelper->quitWebSocketServerThread();
}

void WebSocketServer::onInit()
{
    _readStateMutex.lock();
    _readyState = State::UP;
    _readStateMutex.unlock();

    std::shared_ptr<std::atomic<bool>> isDestroyed = _isDestroyed;
    _wsHelper->sendMessageToCocosThread([this, isDestroyed](){
        if (*isDestroyed)
        {
            LOGD("WebSocketServer instance was destroyed!\n");
        }
        else
        {
            _delegate->onServerUp(this);
        }
    });
}

WebSocketServer::State WebSocketServer::getReadyState()
{
    std::lock_guard<std::mutex> lk(_readStateMutex);
    return _readyState;
}

void WebSocketServer::onSubThreadLoop()
{
    _readStateMutex.lock();
    if (_wsContext && _readyState != State::DOWN && _readyState != State::STOPPING)
    {
        _readStateMutex.unlock();


        _wsHelper->_subThreadWsServerConnectionsMutex.lock();
        for(auto const& ent1 : _wsHelper->_subThreadWsServerConnections) 
        {
            if (!ent1.second->messagesQueue->empty())
            {
                lws_callback_on_writable(ent1.second->wsInstance);
            }
        }
        _wsHelper->_subThreadWsServerConnectionsMutex.unlock();


        lws_service(_wsContext, 50);
    }
    else
    {
        LOGD("WebSocketServer state is down, code=%d, quit websocket thread!\n", static_cast<int>(_readyState));
        _readStateMutex.unlock();
        _wsHelper->quitWebSocketServerThread();
    }
}

void WebSocketServer::onSubThreadStarted()
{
    struct lws_context_creation_info info;
    memset(&info, 0, sizeof info);

    info.port = _port;
    info.protocols = _wsProtocols;
    
    info.gid = -1;
    info.uid = -1;
    info.options = 0;
    info.user = this;
    info.iface = NULL;
    info.ssl_cert_filepath = NULL;
    info.ssl_private_key_filepath = NULL;

    info.ka_time = 60; 
    info.ka_probes = 10; 
    info.ka_interval = 10; 

    int log_level = LLL_ERR | LLL_WARN | LLL_NOTICE | LLL_INFO | LLL_DEBUG | LLL_PARSER | LLL_HEADER | LLL_EXT | LLL_CLIENT | LLL_LATENCY;
    lws_set_log_level(log_level, printWebSocketServerLog);

    _wsContext = lws_create_context(&info);

    if (nullptr == _wsContext)
    {
        CCLOGERROR("Create websocket context failed!");
    }
}

void WebSocketServer::onSubThreadEnded()
{
    if (_wsContext != nullptr)
    {
        lws_context_destroy(_wsContext);
    }
}

void WebSocketServer::onSendPendingMessages(int socketId) 
{
    Connection* con = _wsHelper->_subThreadWsServerConnections[socketId];

    if (con->messagesQueue->empty())
    {
        return;
    }

    std::list<WsServerMessage*>::iterator iter = con->messagesQueue->begin();

    ssize_t bytesWrite = 0;
    if (iter != con->messagesQueue->end())
    {
        WsServerMessage* subThreadMsg = *iter;
        Data* data = (Data*)subThreadMsg->obj;

        const ssize_t c_bufferSize = WS_RX_BUFFER_SIZE;

        const ssize_t remaining = data->len - data->issued;
        const ssize_t n = std::min(remaining, c_bufferSize);

        WebSocketServerFrame* frame = nullptr;

        if (data->ext)
        {
            frame = (WebSocketServerFrame*)data->ext;
        }
        else
        {
            frame = new (std::nothrow) WebSocketServerFrame();
            bool success = frame && frame->init((unsigned char*)(data->bytes + data->issued), n);
            if (success)
            {
                data->ext = frame;
            }
            else
            { // If frame initialization failed, delete the frame and drop the sending data
              // These codes should never be called.
                LOGD("WebSocketServerFrame initialization failed, drop the sending data, msg(%d)\n", (int)subThreadMsg->id);
                delete frame;
                CC_SAFE_FREE(data->bytes);
                CC_SAFE_DELETE(data);
                con->messagesQueue->erase(iter);
                CC_SAFE_DELETE(subThreadMsg);
                return;
            }
        }

        int writeProtocol;

        if (data->issued == 0)
        {
            if (WS_MSG_TO_SUBTRHEAD_SENDING_STRING == subThreadMsg->what)
            {
                writeProtocol = LWS_WRITE_TEXT;
            }
            else
            {
                writeProtocol = LWS_WRITE_BINARY;
            }

            // If we have more than 1 fragment
            if (data->len > c_bufferSize)
                writeProtocol |= LWS_WRITE_NO_FIN;
        } else {
            // we are in the middle of fragments
            writeProtocol = LWS_WRITE_CONTINUATION;
            // and if not in the last fragment
            if (remaining != n)
                writeProtocol |= LWS_WRITE_NO_FIN;
        }

        bytesWrite = lws_write(con->wsInstance, frame->getPayload(), frame->getPayloadLength(), (lws_write_protocol)writeProtocol);

        // Handle the result of lws_write
        // Buffer overrun?
        if (bytesWrite < 0)
        {
            LOGD("ERROR: msg(%u), lws_write return: %d, but it should be %d, drop this message.\n", subThreadMsg->id, (int)bytesWrite, (int)n);
            // socket error, we need to close the socket connection
            onConnectionClosed(socketId);

            CC_SAFE_FREE(data->bytes);
            delete ((WebSocketServerFrame*)data->ext);
            data->ext = nullptr;
            CC_SAFE_DELETE(data);
            con->messagesQueue->erase(iter);
            CC_SAFE_DELETE(subThreadMsg);
        }
        else if (bytesWrite < frame->getPayloadLength())
        {
            frame->update(bytesWrite);
            LOGD("frame wasn't sent completely, bytesWrite: %d, remain: %d\n", (int)bytesWrite, (int)frame->getPayloadLength());
        }
        // Do we have another fragments to send?
        else if (remaining > frame->getFrameLength() && bytesWrite == frame->getPayloadLength())
        {
            // A frame was totally sent, plus data->issued to send next frame
            LOGD("msg(%u) append: %d + %d = %d\n", subThreadMsg->id, (int)data->issued, (int)frame->getFrameLength(), (int)(data->issued + frame->getFrameLength()));
            data->issued += frame->getFrameLength();
            delete ((WebSocketServerFrame*)data->ext);
            data->ext = nullptr;
        }
        // Safely done!
        else
        {
            LOGD("Safely done, msg(%d)!\n", subThreadMsg->id);
            if (remaining == frame->getFrameLength())
            {
                LOGD("msg(%u) append: %d + %d = %d\n", subThreadMsg->id, (int)data->issued, (int)frame->getFrameLength(), (int)(data->issued + frame->getFrameLength()));
                LOGD("msg(%u) was totally sent!\n", subThreadMsg->id);
            }
            else
            {
                LOGD("ERROR: msg(%u), remaining(%d) < bytesWrite(%d)\n", subThreadMsg->id, (int)remaining, (int)frame->getFrameLength());
                LOGD("Drop the msg(%u)\n", subThreadMsg->id);
                onConnectionClosed(socketId);
            }

            CC_SAFE_FREE(data->bytes);
            delete ((WebSocketServerFrame*)data->ext);
            data->ext = nullptr;
            CC_SAFE_DELETE(data);
            con->messagesQueue->erase(iter);
            CC_SAFE_DELETE(subThreadMsg);

            LOGD("-----------------------------------------------------------\n");
        }
    }
}

void WebSocketServer::onReceivePendingMessages(int socketId, void* in, ssize_t len)
{
    Connection* con = _wsHelper->_subThreadWsServerConnections[socketId];
    // In websocket thread
    static int packageIndex = 0;
    packageIndex++;
    if (in != nullptr && len > 0)
    {
        LOGD("Receiving data:index:%d, len=%d\n", packageIndex, (int)len);

        unsigned char* inData = (unsigned char*)in;
        _receivedData.insert(_receivedData.end(), inData, inData + len);
    }
    else
    {
        LOGD("Empty message received, index=%d!\n", packageIndex);
    }

    // If no more data pending, send it to the client thread
    size_t remainingSize = lws_remaining_packet_payload(con->wsInstance);
    int isFinalFragment = lws_is_final_fragment(con->wsInstance);
//    LOGD("remainingSize: %d, isFinalFragment: %d\n", (int)remainingSize, isFinalFragment);

    if (remainingSize == 0 && isFinalFragment)
    {
        std::vector<char>* frameData = new (std::nothrow) std::vector<char>(std::move(_receivedData));

        // reset capacity of received data buffer
        _receivedData.reserve(WS_RESERVE_RECEIVE_BUFFER_SIZE);

        ssize_t frameSize = frameData->size();

        bool isBinary = (lws_frame_is_binary(con->wsInstance) != 0);

        if (!isBinary)
        {
            frameData->push_back('\0');
        }

        std::shared_ptr<std::atomic<bool>> isDestroyed = _isDestroyed;
        _wsHelper->sendMessageToCocosThread([this, frameData, frameSize, isBinary, isDestroyed, socketId](){
            // In UI thread
            LOGD("Notify data len %d to Cocos thread.\n", (int)frameSize);

            Data data;
            data.isBinary = isBinary;
            data.bytes = (char*)frameData->data();
            data.len = frameSize;

            if (*isDestroyed)
            {
                LOGD("WebSocketServer instance was destroyed!\n");
            }
            else
            {
                _delegate->onMessage(this, socketId, data);
            }

            delete frameData;
        });
    }
}

void WebSocketServer::onConnectionOpened(const int socketId, lws *wsi)
{
    Connection* con = new Connection;
    con->wsInstance = wsi;
    con->messagesQueue = new (std::nothrow) std::list<WsServerMessage*>();

    _wsHelper->_subThreadWsServerConnectionsMutex.lock();
    _wsHelper->_subThreadWsServerConnections[ socketId ] = con;
    _wsHelper->_subThreadWsServerConnectionsMutex.unlock();

    std::shared_ptr<std::atomic<bool>> isDestroyed = _isDestroyed;
    _wsHelper->sendMessageToCocosThread([this, isDestroyed, socketId](){
        if (*isDestroyed)
        {
            LOGD("WebSocketServer instance was destroyed!\n");
        }
        else
        {
            _delegate->onConnection(this, socketId);
        }
    });
}

void WebSocketServer::onError()
{
    LOGD("WebSocketServer (%p) onError ...\n", this);

    _readStateMutex.lock();
    _readyState = State::STOPPING;
    _readStateMutex.unlock();

    std::shared_ptr<std::atomic<bool>> isDestroyed = _isDestroyed;
    _wsHelper->sendMessageToCocosThread([this, isDestroyed](){
        if (*isDestroyed)
        {
            LOGD("WebSocketServer instance was destroyed!\n");
        }
        else
        {
            _delegate->onError(this, ErrorCode::STARTING_FAILURE);
        }
    });
}

void WebSocketServer::onConnectionClosed(int socketId)
{
    _wsHelper->_subThreadWsServerConnectionsMutex.lock();
    
    Connection* con = _wsHelper->_subThreadWsServerConnections[ socketId ];
    _wsHelper->_subThreadWsServerConnections.erase( socketId );

    _wsHelper->_subThreadWsServerConnectionsMutex.unlock();
 
    std::shared_ptr<std::atomic<bool>> isDestroyed = _isDestroyed;
    _wsHelper->sendMessageToCocosThread([this, isDestroyed, socketId](){
        if (*isDestroyed)
        {
            LOGD("WebSocketServer instance was destroyed!\n");
        }
        else
        {
            _delegate->onDisconnection(this, socketId);
        }
    });

    delete con;
}

int WebSocketServer::onSocketCallback(struct lws *wsi,
                     int reason,
                     void *user, void *in, ssize_t len)
{

    int socketId = lws_get_socket_fd( wsi );

    switch (reason)
    {
        case LWS_CALLBACK_PROTOCOL_INIT:
            onInit();
            break;

        case LWS_CALLBACK_ESTABLISHED:
            onConnectionOpened(socketId, wsi);
            lws_callback_on_writable(wsi);
            break;

        case LWS_CALLBACK_SERVER_WRITEABLE:
            onSendPendingMessages(socketId);
            lws_callback_on_writable(wsi);
            break;
        
        case LWS_CALLBACK_RECEIVE:
            onReceivePendingMessages(socketId, in, len);
            break;

        case LWS_CALLBACK_CLOSED:
            onConnectionClosed(socketId);
            break;

        case LWS_CALLBACK_GET_THREAD_ID:
            return static_cast<int>(std::hash<std::thread::id>()(std::this_thread::get_id()));

        case LWS_CALLBACK_CHANGE_MODE_POLL_FD:
            break;

        case LWS_CALLBACK_LOCK_POLL:
            break;

        case LWS_CALLBACK_UNLOCK_POLL:
            break;

        default:
            LOGD("Unhandled websocket event: %d\n", reason);
            break;
    }

    return 0;
}

}

NS_CC_END
