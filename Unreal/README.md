#### 具体接入步骤
1. 从 [UE4 Poco SDK](https://github.com/AirtestProject/Poco-SDK/tree/master/Unreal) 将 `PocoSDK` 这个文件夹克隆下来，放置到您项目的 `Plugins` 目录下。如果您项目没有名为 `Plugins` 的目录的话，需要先创建该目录。


2. 重新编译项目。确认在编辑器的 `Edit > Plugins` 当中能看到 `Poco SDK` ，且 `Enabled` 为被勾选的状态。

    如果此时 `Enabled` 未被勾选，请勾选 `Enabled` 并按提示重启编辑器/VS。
    
    ![image](https://note.youdao.com/yws/public/resource/df1f7128bebe35c6759cfc2f5193a498/xmlnote/WEBRESOURCEb44cd535cade696b1ed6c44086a4d00c/81)
    
3. 运行游戏。


#### 使用方法

1. 更新最新版的Poco，指令为
    ```
    pip install --upgrade pocoui
    ```
    注意包名为`pocoui`，而不是`poco`。更新完毕后请确认`poco`的版本号至少为1.0.79。
    
2. 当前版本的IDE尚未外放UI树查看时UE4的选项，此功能将会在下个版本的IDE中放出。因为端口与Unity一致，目前可以使用Unity的选项来查看UI树。

    而在脚本层面的具体使用方法如下：
    ```
    from poco.drivers.ue4 import UE4Poco
    poco = UE4Poco()
    # example
    poco("StartButton").click()
    ```
    如果想用编辑器模式，可以在初始化`Poco`的时候传入参数,
    ```
    poco = UE4Poco(ue4_editor=True)
    ```
    
    ![image](https://note.youdao.com/yws/public/resource/36a5b0555a9102f0e3a76efc0a0e02dd/xmlnote/7A389B6954DD40978028F556671D9853/111)
    
    此方式连接的窗口为编辑器PIE独立窗口模式，如果失败，可以自行修改`poco/drivers/ue4/device.py`下连接设备的句柄。如下列代码就是连接UE4窗口中带有`Game Preview Standalone`字样的窗口。
    ```
    dev = connect_device("Windows:///?class_name=UnrealWindow&title_re=.*Game Preview Standalone.*")
    ```
    ![image](https://note.youdao.com/yws/public/resource/36a5b0555a9102f0e3a76efc0a0e02dd/xmlnote/C37A20D77A804FCAB4E57E9D005DABE0/103)
    

#### 一些常见的问题：

1. `Poco SDK` 未能正常启动。

    确认 `Poco SDK `为 `Enabled` 的状态。
    确认 `.uproject` 文件中包含有`Poco SDK`, 且为Enabled状态。
    
2. 启动游戏时报错，`Output Log` 中信息为
    ```
    LogPluginManager: Error: Unable to load plugin 'PocoSDK'. Aborting.
    ```
    请确认`PocoSDK.uplugin`文件中的`"Installed"`这一项对应的值是`true`，且没有`"Enterprise"`这一项。
    