Imports System.Configuration
Imports System.Web.Configuration

Friend Module _Common
#Region " .ExtractValue() "
	Private Function ExtractValue(ByVal variableName As String, ByVal queryString As String, Optional ByVal splitter As String = "&") As String
		If queryString = "" Then Return ""

		Dim myVar As String() = Split(queryString, Splitter)
		Dim maxVar As Integer = UBound(myVar)

		Dim x As Integer = 0
		Dim extVar As String = ""
		Dim ret As String = ""

		For x = 0 To maxVar
			extVar = Mid(myVar(x), 1, InStr(1, myVar(x), "=") - 1)
			If variableName = extVar Then
				ret = Mid(myVar(x), InStr(1, myVar(x), "=") + 1)
			End If
		Next

		Return ret
	End Function
#End Region

#Region " getConnectionSettings "
	Friend Function getConnectionSettings() As CouchDbConnectionSettings
		Dim conf As Configuration = WebConfigurationManager.OpenWebConfiguration(Web.Hosting.HostingEnvironment.ApplicationVirtualPath)
		Dim temp As String = conf.ConnectionStrings.ConnectionStrings("SiteAdmin").ConnectionString

		Dim settings As New CouchDbConnectionSettings
		settings.host = ExtractValue("host", temp, ";")
		settings.port = ExtractValue("port", temp, ";")
		settings.db = ExtractValue("db", temp, ";")
		settings.username = ExtractValue("username", temp, ";")
		settings.password = ExtractValue("password", temp, ";")

		Return settings
	End Function
#End Region

#Region " GetApplicationName "
	Friend Function getApplicationName() As String
		Dim conf As Configuration = WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath)
		Dim mSection As New MembershipSection
		mSection = conf.GetSection("system.web/membership")

		Dim appName As String = mSection.Providers(mSection.DefaultProvider).Parameters("applicationName")
		If appName = "" Then
			appName = System.Web.HttpContext.Current.Request.Url.Host

			If System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath <> "/" Then _
			appName += System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath
		End If
		Return appName
	End Function
#End Region

#Region " Log "
	Friend Sub Log(ByVal data As String, Optional ByVal isError As Boolean = False)
		Dim logs As New WDK.Providers.Logs.ApplicationLog
		logs.write(data, isError)
	End Sub
#End Region

End Module

Friend Class CouchDbConnectionSettings
	Friend host As String = "localhost"
	Friend port As Integer = 5984
	Friend db As String
	Friend username As String
	Friend password As String
End Class
