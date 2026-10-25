'由于包含加解密等安全信息，本文件中的部分代码已被删除

Friend Module ModSecret

    '标注 PCL 的不同分支，仅用于替换标记
    Public Const VersionBranchMain As String = "OpenSource"
    '在开源版的注册表与常规版的注册表隔离，以防数据冲突
    Public Const RegFolder As String = "PCLDebug"
    '用于微软登录的 ClientId，依次从环境变量 PCL_MS_CLIENT_ID、启动器目录下的 MSClientId.txt 读取
    Private _OAuthClientId As String = Nothing
    Public ReadOnly Property OAuthClientId As String
        Get
            If _OAuthClientId IsNot Nothing Then Return _OAuthClientId
            Dim Id As String = Environment.GetEnvironmentVariable("PCL_MS_CLIENT_ID")
            If String.IsNullOrWhiteSpace(Id) Then
                Try
                    Dim ConfigPath As String = Paths.Base & "MSClientId.txt"
                    If FileUtils.Exists(ConfigPath) Then Id = FileUtils.TryReadAsString(ConfigPath)
                Catch
                End Try
            End If
            _OAuthClientId = If(Id, "").Trim()
            Return _OAuthClientId
        End Get
    End Property
    'CurseForge API Key
    Public CurseForgeAPIKey As String = If(Environment.GetEnvironmentVariable("PCL_CURSEFORGE_API_KEY"), "")
    '用于匿名数据收集的腾讯云日志服务上报 URL，形如 https://{region}.cls.tencentcs.com/track?topic_id={topic_id}
    Public Const ClsBaseUrl As String = ""

#Region "网络鉴权"

    Friend Function SecretCdnSign(UrlWithMark As String) As String
        If Not UrlWithMark.EndsWithF("{CDN}") Then Return UrlWithMark
        Return UrlWithMark.Replace("{CDN}", "").Replace(" ", "%20")
    End Function
    ''' <summary>
    ''' 设置 Headers 的 UA、Referer。
    ''' </summary>
    Friend Sub SecretHeadersSign(Url As String, ByRef Req As HttpRequestMessage, Optional SimulateBrowserHeaders As Boolean = False)
        If Not Req.Headers.UserAgent.Any Then
            If Url.Contains("baidupcs.com") OrElse Url.Contains("baidu.com") Then
                Req.Headers.Add("User-Agent", "LogStatistic")  '#4951
            ElseIf SimulateBrowserHeaders Then
                Req.Headers.Add("User-Agent", $"PCL2/{VersionBaseName}.{CInt(BuildType)} Mozilla/5.0 AppleWebKit/537.36 Chrome/63.0.3239.132 Safari/537.36")
            Else
                Req.Headers.Add("User-Agent", $"PCL2/{VersionBaseName}.{CInt(BuildType)}")
            End If
        End If
        If Not SimulateBrowserHeaders Then Req.Headers.Add("Referer", $"http://{VersionCode}.open.pcl2.server/")
        If Url.Contains("api.curseforge.com") OrElse Url.Contains("forgecdn.net") Then Req.Headers.Add("x-api-key", CurseForgeAPIKey)
    End Sub

#End Region

#Region "主题"

    Public Color1 As New MyColor(52, 61, 74)
    Public Color2 As New MyColor(11, 91, 203)
    Public Color3 As New MyColor(19, 112, 243)
    Public Color4 As New MyColor(72, 144, 245)
    Public Color5 As New MyColor(150, 192, 249)
    Public Color6 As New MyColor(213, 230, 253)
    Public Color7 As New MyColor(222, 236, 253)
    Public Color8 As New MyColor(234, 242, 254)
    Public ColorBg0 As New MyColor(150, 192, 249)
    Public ColorBg1 As New MyColor(190, Color7)
    Public ColorGray1 As New MyColor(64, 64, 64)
    Public ColorGray2 As New MyColor(115, 115, 115)
    Public ColorGray3 As New MyColor(140, 140, 140)
    Public ColorGray4 As New MyColor(166, 166, 166)
    Public ColorGray5 As New MyColor(204, 204, 204)
    Public ColorGray6 As New MyColor(235, 235, 235)
    Public ColorGray7 As New MyColor(240, 240, 240)
    Public ColorGray8 As New MyColor(245, 245, 245)
    Public ColorSemiTransparent As New MyColor(1, Color8)

    Public ThemeNow As Integer = -1
    Public ColorHue As Integer = 210, ColorSat As Integer = 85, ColorLightAdjust As Integer = 0, ColorHueTopbarDelta As OneOf(Of Integer, Integer()) = 0
    Public ThemeDontClick As Integer = 0

    ''' <summary>
    ''' 深色主题的编号。该主题使用独立的深色调色板，其余主题均沿用原有的浅色 HSL 配色。
    ''' </summary>
    Public Const ThemeDark As Integer = 15
    ''' <summary>
    ''' 当前是否处于深色主题。
    ''' </summary>
    Public ReadOnly Property ThemeIsDark As Boolean
        Get
            Return ThemeNow = ThemeDark
        End Get
    End Property

    Public Sub ThemeRefresh(Optional NewTheme As Integer = -1)
        Try
            If ThemeNow = NewTheme AndAlso NewTheme >= 0 Then Return
            If NewTheme >= 0 Then ThemeNow = NewTheme

            If ThemeIsDark Then
                RefreshThemeDark()
            Else
                Color1 = New MyColor().FromHSL2(ColorHue, ColorSat * 0.2, 25 + ColorLightAdjust * 0.3)
                Color2 = New MyColor().FromHSL2(ColorHue, ColorSat, 45 + ColorLightAdjust)
                Color3 = New MyColor().FromHSL2(ColorHue, ColorSat, 55 + ColorLightAdjust)
                Color4 = New MyColor().FromHSL2(ColorHue, ColorSat, 65 + ColorLightAdjust)
                Color5 = New MyColor().FromHSL2(ColorHue, ColorSat, 80 + ColorLightAdjust * 0.4)
                Color6 = New MyColor().FromHSL2(ColorHue, ColorSat, 91 + ColorLightAdjust * 0.1)
                Color7 = New MyColor().FromHSL2(ColorHue, ColorSat, 95)
                Color8 = New MyColor().FromHSL2(ColorHue, ColorSat, 97)
                RefreshThemeLightSurfaces()
            End If
            ColorBg0 = Color4 * 0.4 + Color5 * 0.4 + ColorGray4 * 0.2
            ColorBg1 = New MyColor(190, Color7)

            ColorSemiTransparent = New MyColor(1, Color8)
            Application.Current.Resources("ColorBrush1") = New SolidColorBrush(Color1)
            Application.Current.Resources("ColorBrush2") = New SolidColorBrush(Color2)
            Application.Current.Resources("ColorBrush3") = New SolidColorBrush(Color3)
            Application.Current.Resources("ColorBrush4") = New SolidColorBrush(Color4)
            Application.Current.Resources("ColorBrush5") = New SolidColorBrush(Color5)
            Application.Current.Resources("ColorBrush6") = New SolidColorBrush(Color6)
            Application.Current.Resources("ColorBrush7") = New SolidColorBrush(Color7)
            Application.Current.Resources("ColorBrush8") = New SolidColorBrush(Color8)
            Application.Current.Resources("ColorBrushBg0") = New SolidColorBrush(ColorBg0)
            Application.Current.Resources("ColorBrushBg1") = New SolidColorBrush(ColorBg1)
            Application.Current.Resources("ColorObject1") = CType(Color1, Color)
            Application.Current.Resources("ColorObject2") = CType(Color2, Color)
            Application.Current.Resources("ColorObject3") = CType(Color3, Color)
            Application.Current.Resources("ColorObject4") = CType(Color4, Color)
            Application.Current.Resources("ColorObject5") = CType(Color5, Color)
            Application.Current.Resources("ColorObject6") = CType(Color6, Color)
            Application.Current.Resources("ColorObject7") = CType(Color7, Color)
            Application.Current.Resources("ColorObject8") = CType(Color8, Color)
            Application.Current.Resources("ColorObjectBg0") = CType(ColorBg0, Color)
            Application.Current.Resources("ColorObjectBg1") = CType(ColorBg1, Color)
            '配色已写入，再刷新界面上的提示框；这一步失败不应影响配色本身
            ThemeRefreshHints()
            ThemeRefreshMain()
        Catch ex As Exception
            Logger.Error(ex, "刷新主题颜色失败", LogBehavior.Toast)
        End Try
    End Sub

    ''' <summary>
    ''' 应用深色主题的调色板。深色主题以近黑深蓝灰为底（Kimi 风格），强调色采用 Kimi 标志性的靛蓝。
    ''' </summary>
    Private Sub RefreshThemeDark()
        '正文与图标：浅色文字配合深色底
        Color1 = New MyColor("#E8ECF5")
        Color2 = New MyColor("#8DA2FF")
        'Kimi 品牌蓝：#4D6BFE，以及更深的 #3859FF
        Color3 = New MyColor("#4D6BFE")
        Color4 = New MyColor("#3859FF")
        Color5 = New MyColor("#2B3A8F")
        '6~8 在深色主题中充当「面板」而非「浅色块」，因此逐步加深
        Color6 = New MyColor("#1A1D26")
        Color7 = New MyColor("#151821")
        Color8 = New MyColor("#0F1118")
        ColorGray1 = New MyColor("#C8CEDC")
        ColorGray2 = New MyColor("#9BA5BC")
        ColorGray3 = New MyColor("#7C879E")
        ColorGray4 = New MyColor("#626C82")
        ColorGray5 = New MyColor("#454F63")
        ColorGray6 = New MyColor("#252B39")
        ColorGray7 = New MyColor("#1F2530")
        ColorGray8 = New MyColor("#191E28")
        SetDarkSurfaces(True)
    End Sub

    ''' <summary>
    ''' 切换回浅色主题时恢复灰度表并复原语义资源。
    ''' </summary>
    Private Sub RefreshThemeLightSurfaces()
        ColorGray1 = New MyColor(64, 64, 64)
        ColorGray2 = New MyColor(115, 115, 115)
        ColorGray3 = New MyColor(140, 140, 140)
        ColorGray4 = New MyColor(166, 166, 166)
        ColorGray5 = New MyColor(204, 204, 204)
        ColorGray6 = New MyColor(235, 235, 235)
        ColorGray7 = New MyColor(240, 240, 240)
        ColorGray8 = New MyColor(245, 245, 245)
        SetDarkSurfaces(False)
    End Sub

    ''' <summary>
    ''' 更新深色主题专属的语义资源（卡片、弹层、提示框等）。
    ''' </summary>
    Private Sub SetDarkSurfaces(IsDark As Boolean)
        Dim Res = Application.Current.Resources
        '灰度表
        Res("ColorBrushGray1") = New SolidColorBrush(ColorGray1)
        Res("ColorBrushGray2") = New SolidColorBrush(ColorGray2)
        Res("ColorBrushGray3") = New SolidColorBrush(ColorGray3)
        Res("ColorBrushGray4") = New SolidColorBrush(ColorGray4)
        Res("ColorBrushGray5") = New SolidColorBrush(ColorGray5)
        Res("ColorBrushGray6") = New SolidColorBrush(ColorGray6)
        Res("ColorBrushGray7") = New SolidColorBrush(ColorGray7)
        Res("ColorBrushGray8") = New SolidColorBrush(ColorGray8)
        Res("ColorObjectGray1") = CType(ColorGray1, Color)
        Res("ColorObjectGray2") = CType(ColorGray2, Color)
        Res("ColorObjectGray3") = CType(ColorGray3, Color)
        Res("ColorObjectGray4") = CType(ColorGray4, Color)
        Res("ColorObjectGray5") = CType(ColorGray5, Color)
        Res("ColorObjectGray6") = CType(ColorGray6, Color)
        Res("ColorObjectGray7") = CType(ColorGray7, Color)
        Res("ColorObjectGray8") = CType(ColorGray8, Color)
        If IsDark Then
            '卡片与弹层：比页面底色略亮的浮起面（Kimi 风格的近黑面板）
            Res("ColorBrushCard") = New SolidColorBrush(New MyColor("#F21A1D26"))
            Res("ColorBrushPopup") = New SolidColorBrush(New MyColor("#FF14161C"))
            Res("ColorBrushMsg") = New SolidColorBrush(New MyColor("#FF14161C"))
            Res("ColorBrushTextWeak") = New SolidColorBrush(New MyColor("#FF9BA5BC"))
            Res("ColorBrushSemiTransparent") = New SolidColorBrush(New MyColor("#010F1118"))
            Res("ColorBrushBackgroundTransparentSidebar") = New SolidColorBrush(New MyColor("#F10F1118"))
            Res("ColorBrushHalfWhite") = New SolidColorBrush(New MyColor("#14FFFFFF"))
            Res("ColorBrushSemiWhite") = New SolidColorBrush(New MyColor("#33151821"))
            Res("ColorBrushWhite") = New SolidColorBrush(New MyColor("#FF0F1118"))
            Res("ColorBrushRedBack") = New SolidColorBrush(New MyColor("#40301B1F"))
            '提示框配色：深底亮字
            Res("ColorBrushHintBlueBack") = New SolidColorBrush(New MyColor("#FF18203D"))
            Res("ColorBrushHintBlueFore") = New SolidColorBrush(New MyColor("#FFA5B4FF"))
            Res("ColorBrushHintBlueLine") = New SolidColorBrush(New MyColor("#FF4D6BFE"))
            Res("ColorBrushHintRedBack") = New SolidColorBrush(New MyColor("#FF35202A"))
            Res("ColorBrushHintRedFore") = New SolidColorBrush(New MyColor("#FFFF9A9A"))
            Res("ColorBrushHintRedLine") = New SolidColorBrush(New MyColor("#FFFF6B6B"))
            Res("ColorBrushHintYellowBack") = New SolidColorBrush(New MyColor("#FF33280F"))
            Res("ColorBrushHintYellowFore") = New SolidColorBrush(New MyColor("#FFFFC36B"))
            Res("ColorBrushHintYellowLine") = New SolidColorBrush(New MyColor("#FFFFA733"))
            Res("ColorObjectShadow") = CType(New MyColor("#FF000000"), Color)
        Else
            Res("ColorBrushCard") = New SolidColorBrush(New MyColor("#F5FFFFFF"))
            Res("ColorBrushPopup") = New SolidColorBrush(New MyColor("#FFFFFFFF"))
            Res("ColorBrushMsg") = New SolidColorBrush(New MyColor("#FFFBFBFB"))
            Res("ColorBrushTextWeak") = New SolidColorBrush(New MyColor("#FF5C5C5C"))
            Res("ColorBrushSemiTransparent") = New SolidColorBrush(New MyColor("#01eaf2fe"))
            Res("ColorBrushBackgroundTransparentSidebar") = New SolidColorBrush(New MyColor("#F1FFFFFF"))
            Res("ColorBrushHalfWhite") = New SolidColorBrush(New MyColor("#55ffffff"))
            Res("ColorBrushSemiWhite") = New SolidColorBrush(New MyColor("#bbffffff"))
            Res("ColorBrushWhite") = New SolidColorBrush(New MyColor("#ffffff"))
            Res("ColorBrushRedBack") = New SolidColorBrush(New MyColor("#80fbdddd"))
            Res("ColorBrushHintBlueBack") = New SolidColorBrush(New MyColor("#d9ecff"))
            Res("ColorBrushHintBlueFore") = New SolidColorBrush(New MyColor("#0F64B8"))
            Res("ColorBrushHintBlueLine") = New SolidColorBrush(New MyColor("#1172D4"))
            Res("ColorBrushHintRedBack") = New SolidColorBrush(New MyColor("#ffdddf"))
            Res("ColorBrushHintRedFore") = New SolidColorBrush(New MyColor("#bf0b0b"))
            Res("ColorBrushHintRedLine") = New SolidColorBrush(New MyColor("#d82929"))
            Res("ColorBrushHintYellowBack") = New SolidColorBrush(New MyColor("#ffebd7"))
            Res("ColorBrushHintYellowFore") = New SolidColorBrush(New MyColor("#d86c00"))
            Res("ColorBrushHintYellowLine") = New SolidColorBrush(New MyColor("#f57a00"))
            Res("ColorObjectShadow") = CType(New MyColor("#FF343D4A"), Color)
        End If
    End Sub

    ''' <summary>
    ''' 刷新界面上已有提示框的配色。
    ''' </summary>
    Private Sub ThemeRefreshHints()
        RunInUi(
        Sub()
            If FrmMain Is Nothing OrElse FrmMain.PanHint Is Nothing Then Return
            For Each Item In FrmMain.PanHint.Children
                Dim HintItem = TryCast(Item, MyHint)
                If HintItem IsNot Nothing Then HintItem.RefreshThemeColor()
            Next
        End Sub)
    End Sub

    Public Sub ThemeRefreshMain()
        RunInUi(
        Sub()
            If Not FrmMain.IsLoaded Then Return
            '顶部条背景
            Dim Brush = New LinearGradientBrush With {.EndPoint = New Point(1, 0), .StartPoint = New Point(0, 0)}
            If ThemeIsDark Then
                '深色主题（Kimi 风格）：从靛蓝到近黑的横向渐变
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0, .Color = New MyColor("#262F6E")})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0.5, .Color = New MyColor("#1A2150")})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 1, .Color = New MyColor("#101536")})
            Else
                Dim Deltas = ColorHueTopbarDelta.Switch(Function(d) New Integer() {-d, 0, d}, Function(d) d)
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0, .Color = New MyColor().FromHSL2(ColorHue + Deltas(0), ColorSat, 48 + ColorLightAdjust)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0.5, .Color = New MyColor().FromHSL2(ColorHue + Deltas(1), ColorSat, 54 + ColorLightAdjust)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 1, .Color = New MyColor().FromHSL2(ColorHue + Deltas(2), ColorSat, 48 + ColorLightAdjust)})
            End If
            FrmMain.PanTitle.Background = Brush
            FrmMain.PanTitle.Background.Freeze()
            '主页面背景
            If ThemeIsDark Then
                If Settings.Get(Of Boolean)("UiBackgroundColorful") Then
                    Brush = New LinearGradientBrush With {.EndPoint = New Point(0.1, 1), .StartPoint = New Point(0.9, 0)}
                    Brush.GradientStops.Add(New GradientStop With {.Offset = -0.1, .Color = New MyColor("#0B0C11")})
                    Brush.GradientStops.Add(New GradientStop With {.Offset = 0.4, .Color = New MyColor("#0E1017")})
                    Brush.GradientStops.Add(New GradientStop With {.Offset = 1.1, .Color = New MyColor("#090A0F")})
                    FrmMain.PanForm.Background = Brush
                Else
                    FrmMain.PanForm.Background = New MyColor("#0E1015")
                End If
            ElseIf Settings.Get(Of Boolean)("UiBackgroundColorful") Then
                Brush = New LinearGradientBrush With {.EndPoint = New Point(0.1, 1), .StartPoint = New Point(0.9, 0)}
                Brush.GradientStops.Add(New GradientStop With {.Offset = -0.1, .Color = New MyColor().FromHSL2(ColorHue - 15, ColorSat * 0.8, 91)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0.4, .Color = New MyColor().FromHSL2(ColorHue, ColorSat * 0.8, 91)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 1.1, .Color = New MyColor().FromHSL2(ColorHue + 15, ColorSat * 0.8, 91)})
                FrmMain.PanForm.Background = Brush
            Else
                FrmMain.PanForm.Background = New MyColor(245, 245, 245)
            End If
            FrmMain.PanForm.Background.Freeze()
        End Sub)
    End Sub
    Friend Sub ThemeCheckAll(EffectSetup As Boolean)
    End Sub
    Friend Function ThemeCheckOne(Id As Integer) As Boolean
        Return True
    End Function
    Friend Function ThemeUnlock(Id As Integer, Optional ShowDoubleHint As Boolean = True, Optional UnlockHint As String = Nothing) As Boolean
        Return False
    End Function

#End Region

#Region "更新"

    Friend Sub UpdateCheckByButton()
        Hint("该版本中不包含更新功能……")
    End Sub

    Friend IsUpdateWaitingRestart As Boolean = False
    Public Sub UpdateRestart(TriggerRestartAndByEnd As Boolean)
    End Sub
    Public Sub UpdateReplace(ProcessId As Integer, OldFileName As String, NewFileName As String, TriggerRestart As Boolean)
    End Sub

    ''' <summary>
    ''' 确保 PathTemp/Latest.exe 是最新正式版的 PCL，它会被用于整合包打包。
    ''' 如果不是，则下载一个。
    ''' </summary>
    Friend Sub DownloadLatestPCL(Optional LoaderToSyncProgress As LoaderBase = Nothing)
        '注意：如果要自行实现这个功能，请换用另一个文件路径，以免与官方版本冲突
    End Sub

#End Region

#Region "联网配置"

    ''' <summary>
    ''' 联网获取的配置信息。
    ''' 若获取失败或仍在获取中，可能为 Nothing。
    ''' </summary>
    Public ServerConfig As JObject

    Public ServerLoader As New LoaderTask(Of Integer, Integer)("PCL 配置更新", Sub() Logger.Info("该版本中不包含更新通知功能……"), Priority:=ThreadPriority.BelowNormal) With
        {.ReloadTimeout = 1000 * 60 * 60} '超时 1 小时

#End Region

#Region "赞助等级"

    Public ReadOnly Property CurrentRank As DonationRank
        Get
            Return DonationRank.None
        End Get
    End Property

    Public Sub InputPotatoCode(IsUpdating As Boolean)
    End Sub
    Friend Sub GeneratePotatoCode()
    End Sub

    ''' <summary>
    ''' 获取设备识别码。
    ''' </summary>
    Friend Function GetIdentify() As String
        Return "0000-0000-0000-0000"
    End Function

#End Region

End Module
