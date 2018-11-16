# egret安装pocoSDK说明

## 环境配置
* 首先下载pocoSDK 
* 然后在`egretProperties.json`中修改modules属性，添加红框内相应字段，**其中name属性必须为poco**
* 路径可以是相对路径也可以是绝对路径，具体可以参考[白鹭引擎说明文档](http://developer.egret.com/cn/github/egret-docs/Engine2D/projectConfig/configFile/index.html)中有关于 modules字段的说明

![添加和修改modules属性](doc/1.png)

- 然后通过快捷键 ctrl+` 呼出终端 在终端中执行命令egret build -e
![](doc/2.png)

- 在入口文件`main.ts`的rungame函数中新建类型为`PocoManager`的对象，并且传入this.stage
![](doc/3.png)

- 最后再终端中输入`python -m poco.utils.net.stdbroker ws://*:5003 tcp://*:15004`打开代理服务器
- 其中websocket端口默认为5003 如果有更改的需要可以在新建pocomanager的时候传入端口参数
![](doc/4.png)

## 使用AirtestIDE连接

### 连接桌面浏览器
AirtestIDE目前支持Windows窗口连接，MacOS窗口支持暂未放出。所以目前需要使用Windows环境连接桌面浏览器，步骤如下：

1. 运行项目，并在浏览器中打开页面，同时启动broker代理`python -m poco.utils.net.stdbroker ws://*:5003 tcp://*:15004`，如下： 
![image](doc/windows_egret.png)

2. AirtestIDE设置windows窗口连接模式，如下设置为使用嵌入模式：
![image](doc/ide_setting.png)

3. 使用AirtestIDE 选定窗口 功能连接浏览器窗口，连接成功后可以IDE中看到浏览器窗口：
![image](doc/ide_connect.png)

4. 在AirtestIDE中选定游戏区域，打开设置中poco window area select进行框选：
![image](doc/select_area.png)

5. 确定区域之后，在AirtestIDE中即可以使用inspector功能进行UI检视：
![image](doc/inspect.png)


### 连接手机浏览器
完善中..

