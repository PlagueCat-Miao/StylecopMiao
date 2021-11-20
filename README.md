# StylecopMiao
Stylecop自制规则试验场

### quick-start

#### 插件安装

- 安装nupkgLib中依赖 
- 将以下依赖的dll放置与项目Settings.StyleCop同目录下
  - 插件安装在`C:\Users\{username}\AppData\Local\Microsoft\VisualStudio`
  - dll包括 Newtonsoft.Json.dll、StyleCop.dll、StyleCop.CSharp.dll和自定义规则MiaoRule.dll
- 启动项目，对待测试项目右键运行‘Run StyleCop’，下方输出结果。
  - 通过重启vs或者运行‘StyleCop Settings’可以使StyleCop重新加载dll

#### Msbuild安装

- MsBuild安装在本项目内 [StyleCop.MSBuild](https://www.nuget.org/packages/StyleCop.MSBuild)项目下