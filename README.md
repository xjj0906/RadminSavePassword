# RadminSavePassword

[![Build status](https://ci.appveyor.com/api/projects/status/rdbyia8eoogscfed?svg=true)](https://ci.appveyor.com/project/xjj0906/radminsavepassword)

程序会自动记录并管理 Radmin 的密码

## **适用于以下 Radmin**

| 属性 |   值   |
|------|:------:|
| 版本 |3.4、3.5|
| 语言 |  多国  |

## **如何使用：**

* 运行 RadminSavePassword 程序
* 打开 Radmin Viewer 的连接，如连接的名称为：192.168.151.76
* 勾选`另存为缺省值`，点击`确认`

    ![20190226132523](/Images/20190226132523.png)

* 程序则自动保存名称为 192.168.151.76 的用户名密码

    ![20190226132619](/Images/20190226132619.png)

## **实现原理：** 

- 程序在启动后，通过安装系统钩子(Hook)，对 Windows 窗体创建销毁、鼠标、键盘进行监控，从而达到获取 Radmin 程序的登陆信息

## **关于钩子(Hook)：**

- 钩子(Hook)，是 Windows 消息处理机制的一个平台，应用程序可以在上面设置子程以监视指定窗口的某种消息，而且所监视的窗口可以是其他进程所创建的。当消息到达后，在目标窗口处理函数之前处理它。钩子机制允许应用程序截获处理 Window 消息或特定事件。
- 钩子实际上是一个处理消息的程序段，通过系统调用，把它挂入系统。每当特定的消息发出，在没有到达目的窗口前，钩子程序就先捕获该消息，亦即钩子函数先得到控制权。这时钩子函数即可以加工处理（改变）该消息，也可以不作处理而继续传递该消息，还可以强制结束消息的传递。
- 来源：[百度百科]

> **注意：** 由于 .Net 程序安装 WH_SHELL 和 WH_CBT Hook 这两个Hook会失败，因此通过 C++ 的 DLL 间接地安装 Hook，然后再通知 .Net 的程序进行处理的手段
>
> 详情参阅：
> [http://www.codeproject.com/Articles/18638/Using-Window-Messages-to-Implement-Global-System-H](http://www.codeproject.com/Articles/18638/Using-Window-Messages-to-Implement-Global-System-H)

[百度百科]:http://baike.baidu.com/link?url=vvhHuJDnkVN4IaE319drtMogwGv4Jf-ra3Cik8IcMHvf8iqUsa2noXU42twUMYq9VZyfK1Aml_ApNXzx80C0Q_
