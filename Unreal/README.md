#### 具体接入步骤
1. 从 [UE4 Poco SDK](https://github.com/AirtestProject/Poco-SDK/tree/master/Unreal) 将 `PocoSDK` 这个文件夹克隆下来，放置到您项目的 `Plugins` 目录下。如果您项目没有名为 `Plugins` 的目录的话，需要先创建该目录。


2. 重新编译项目。确认在编辑器的 `Edit > Plugins` 当中能看到 `Poco SDK` ，且 `Enabled` 为被勾选的状态。

    如果此时 `Enabled` 未被勾选，请勾选 `Enabled` 并按提示重启编辑器/VS。
    
    ![image](Images/PocoSDK.png)
    
3. 运行游戏。


#### 使用方法

1. 更新最新版的Poco，指令为：
    ```
    pip install --upgrade pocoui
    ```
    注意包名为`pocoui`，而不是`poco`。更新完毕后请确认`poco`的版本号至少为1.0.79。
    
2. 连接游戏。

    启动游戏进程，在IDE中获取UI树。
    
    ![image](Images/UI.png)
    
    如图中UI树只存在`root`，则获取UI失败。
    
    在脚本层面的具体使用方法如下：
    ```
    from poco.drivers.ue4 import UE4Poco
    poco = UE4Poco()
    # example
    poco("StartButton").click()
    ```
    如果想用编辑器模式连接，可以在初始化`Poco`的时候传入参数, 此时编辑器语言需设置为英文。
    ```
    poco = UE4Poco(ue4_editor=True)
    ```
    
    ![image](Images/Modes.png)
    
    此方式连接的窗口为编辑器PIE独立窗口模式，如果失败，可以自行修改`poco/drivers/ue4/device.py`下连接设备的句柄。如下列代码就是连接UE4窗口中带有`Game Preview Standalone`字样的窗口。
    ```
    dev = connect_device("Windows:///?class_name=UnrealWindow&title_re=.*Game Preview Standalone.*")
    ```
    ![image](Images/Window.png)
    UE4引擎版本号在4.26以上，UE4窗口名"Game Preview Standalone"已经修改为Preview [NetMode: Standalone]。
    

#### 一些常见的问题：

1. `Poco SDK` 未能正常启动。

    确认 `Poco SDK `为 `Enabled` 的状态。
    确认 `.uproject` 文件中包含有`Poco SDK`, 且为Enabled状态。

2. UE4选择`Standalone Game`运行游戏无法获取UI树。

    这个问题是由于插件的type为Runtime, 导致插件在编辑器运行时启动，无法获取独立进程的游戏。请使用编辑器模式运行游戏，或直接打包运行。
3. UE4打包`windows版本`游戏运行时无法获取UI树。

    运行游戏先需要先关闭UE4编辑器。