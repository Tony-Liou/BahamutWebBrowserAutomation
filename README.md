# Bahamut Web Browser Automation

使用 [Selenium](https://www.selenium.dev/) WebDriver 讓網頁瀏覽器自動化。

## Development

### Prerequisites

1. 到[下載頁面](https://dotnet.microsoft.com/en-us/download)根據不同 OS 下載所需要的套件。
   - 若只想運行已編譯完成的 `dll` 檔案，下載 .NET **Runtime**。
   - 若想要開發、編譯和運行程式，下載 .NET **SDK**。

   *注意請不要下載到 **.NET Framework**，此專案使用 **.NET 6** 為目標框架。*
2. 安裝 IDE 或編輯器，推薦 [Visual Studio](https://visualstudio.microsoft.com/zh-hant/downloads/) 2022 以上或 [Rider](https://www.jetbrains.com/rider/) 2022.1 以上。

### Configuration

#### 登入巴哈姆特所需的帳號密碼

##### 開發階段

使用 [app secrets](https://docs.microsoft.com/zh-tw/aspnet/core/security/app-secrets?view=aspnetcore-6.0) 來儲存。
進入到 `*.csproj` 專案所在檔案夾後：

```shell
dotnet user-secrets init
dotnet user-secrets set "BAHAMUT_USERNAME" "username"
dotnet user-secrets set "BAHAMUT_PASSWORD" "password"
```

初始化完成後，會發現 csproj 檔中多了 `<UserSecretsId>...</UserSecretsId>`，請勿將此變更加入版本控制。

##### 已發布

推薦使用環境變數來設定，命令列參數也可以。

```shell
export BAHAMUT_USERNAME=username
export BAHAMUT_PASSWORD=password
```

若還想知道有其他設定來源可以參考[官方文件](https://docs.microsoft.com/zh-tw/dotnet/core/extensions/configuration#configure-console-apps)

#### Logging

此專案使用 Serilog，可以參考[官方文件](https://github.com/serilog/serilog-settings-configuration/blob/dev/README.md) 來修改 `appsettings.json` 裡面的設定。

#### Web browser

目前僅使用 Google Chrome。
