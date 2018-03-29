#ifndef __CC_WEBSOCKETSERVER_H__
#define __CC_WEBSOCKETSERVER_H__

#include <string>
#include <vector>
#include <list>
#include <mutex>
#include <map>
#include <memory>  // for std::shared_ptr
#include <atomic>

#include "platform/CCPlatformMacros.h"
#include "platform/CCStdC.h"

struct lws;
struct lws_context;
struct lws_protocols;

/**
 * @addtogroup network
 * @{
 */

NS_CC_BEGIN

class EventListenerCustom;

namespace network {

class WsServerThreadHelper;

/**
 * WebSocketServer is wrapper of the libwebsockets-protocol
 */
class CC_DLL WebSocketServer
{
public:
    

    
    /**
     * Constructor of WebSocketServer.
     *
     * @js ctor
     */
    WebSocketServer();
    /**
     * Destructor of WebSocketServer.
     *
     * @js NA
     * @lua NA
     */
    virtual ~WebSocketServer();

    /**
     * Data structure for message
     */
    struct Data
    {
        Data():bytes(nullptr), len(0), issued(0), isBinary(false), ext(nullptr){}
        char* bytes;
        ssize_t len, issued;
        bool isBinary;
        void* ext;
    };

    /**
     * ErrorCode enum used to represent the error in the websocket.
     */
    enum class ErrorCode
    {
        STARTING_FAILURE, /** &lt; value 0 */
        UNKNOWN,            /** &lt; value 1 */
    };

    /**
     *  State enum used to represent the Websocket state.
     */
    enum class State
    {
        STARTING,  /** &lt; value 0 */
        UP,        /** &lt; value 1 */
        STOPPING,     /** &lt; value 2 */
        DOWN,      /** &lt; value 3 */
    };

    /**
     * The delegate class is used to process websocket events.
     *
     * The most member function are pure virtual functions,they should be implemented the in subclass.
     * @lua NA
     */
    class Delegate
    {
    public:
        /** Destructor of Delegate. */
        virtual ~Delegate() {}
        /**
         * This function to be called after the client connection complete a handshake with the  server.
         * This means that the WebSocketServer connection is ready to send and receive data.
         * 
         * @param ws The WebSocketServer object connected
         */
        virtual void onConnection(WebSocketServer* ws, const int socketID) = 0;
        /**
         * This function to be called when data has appeared from the server for the client connection.
         *
         * @param ws The WebSocketServer object connected.
         * @param data Data object for message.
         */
        virtual void onMessage(WebSocketServer* ws, const int socketID, const Data& data) = 0;
        /**
         * When the WebSocketServer object connected wants to close or the protocol won't get used at all and current _readyState is State::STOPPING,this function is to be called.
         *
         * @param ws The WebSocketServer object connected.
         */
        virtual void onDisconnection(WebSocketServer* ws, const int socketID) = 0;
        /**
         * This function is to be called in the following cases:
         * 1. client connection is failed.
         * 2. the request client connection has been unable to complete a handshake with the remote server.
         * 3. the protocol won't get used at all after this callback and current _readyState is State::STARTING.
         * 4. when a socket descriptor needs to be removed from an external polling array. in is again the struct libwebsocket_pollargs containing the fd member to be removed. If you are using the internal polling loop, you can just ignore it and current _readyState is State::STARTING.
         *
         * @param ws The WebSocketServer object connected.
         * @param error WebSocketServer::ErrorCode enum,would be ErrorCode:: or ErrorCode::STARTING_FAILURE.
         */
        virtual void onError(WebSocketServer* ws, const ErrorCode& error) = 0;

        virtual void onServerUp(WebSocketServer* ws) = 0;

        virtual void onServerDown(WebSocketServer* ws) = 0;
    };


    /**
     *  @brief  The initialized method for websocket.
     *          It needs to be invoked right after websocket instance is allocated.
     *  @param  delegate The delegate which want to receive event from websocket.
     *  @param  url      The URL of websocket server.
     *  @return true: Success, false: Failure.
     *  @lua NA
     */
    bool init(const Delegate& delegate,
              const unsigned int port,
              const std::vector<std::string>* protocols = nullptr);

    /**
     *  @brief Sends string data to websocket server.
     *  
     *  @param message string data.
     *  @lua sendstring
     */
    void send(int socketId, const std::string& message);

    /**
     *  @brief Sends binary data to websocket server.
     *  
     *  @param binaryMsg binary string data.
     *  @param len the size of binary string data.
     *  @lua sendstring
     */
    void send(int socketId, const unsigned char* binaryMsg, unsigned int len);


    /**
     *  @brief Sends string data to websocket client.
     *  
     *  @param message string data.
     *  @lua sendstring
     */
    void broadcast(const std::string& message);

    /**
     *  @brief Sends binary data to websocket client.
     *  
     *  @param binaryMsg binary string data.
     *  @param len the size of binary string data.
     *  @lua sendstring
     */
    void broadcast(const unsigned char* binaryMsg, unsigned int len);


    
    /**
     *  @brief Closes the connection to server synchronously.
     *  @note It's a synchronous method, it will not return until websocket thread exits.
     */
    void stop();
    
    /**
     *  @brief Closes the connection to server asynchronously.
     *  @note It's an asynchronous method, it just notifies websocket thread to exit and returns directly,
     *        If using 'stopAsync' to stop websocket connection, 
     *        be careful of not using destructed variables in the callback of 'onDisconnection'.
     */
    void stopAsync();

    /**
     *  @brief Gets current state of connection.
     *  @return State the state value could be State::STARTING, State::UP, State::STOPPING or State::DOWN
     */
    State getReadyState();

private:
    void onSubThreadStarted();
    void onSubThreadLoop();
    void onSubThreadEnded();

    // The following callback functions are invoked in websocket thread
    int onSocketCallback(struct lws *wsi, int reason, void *user, void *in, ssize_t len);

    void onSendPendingMessages(int socketId);
    void onReceivePendingMessages(int socketId, void* in, ssize_t len);
    
    void onConnectionOpened(int socketId, lws* wsi);
    void onConnectionClosed(int socketId);


    void onError();

    void onServerUp();

    void onServerDown();

    void onInit();

private:
    std::mutex   _readStateMutex;
    State        _readyState;
    unsigned int _port;

    std::vector<char> _receivedData;

    friend class WsServerThreadHelper;
    friend class WebSocketServerCallbackWrapper;
    WsServerThreadHelper* _wsHelper;

    
    struct lws_context* _wsContext;
    std::shared_ptr<std::atomic<bool>> _isDestroyed;
    Delegate* _delegate;
    int _SSLConnection;
    struct lws_protocols* _wsProtocols;
    EventListenerCustom* _resetDirectorListener;
};

}

NS_CC_END

// end group
/// @}

#endif /* defined(__CC_JSB_WEBSOCKETSERVER_H__) */
