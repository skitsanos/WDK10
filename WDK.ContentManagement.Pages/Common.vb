Imports System.Web.Configuration
Imports System.Configuration
Imports WDK.Providers.Logs


Module Common

#Region " .ExtractValue() "
	Private Function ExtractValue(ByVal VariableName As String, ByVal QueryString As String, Optional ByVal Splitter As String = "&") As String
		If QueryString = "" Then Return ""

		Dim myVar As String() = Split(QueryString, Splitter)
		Dim maxVar As Integer = UBound(myVar)

		Dim x As Integer = 0
		Dim extVar As String = ""
		Dim ret As String = ""

		For x = 0 To maxVar
			extVar = Mid(myVar(x), 1, InStr(1, myVar(x), "=") - 1)
			If VariableName = extVar Then
				ret = Mid(myVar(x), InStr(1, myVar(x), "=") + 1)
			End If
		Next

		Return ret
	End Function
#End Region

#Region " getConnectionSettings "
	Friend Function getConnectionSettings() As CouchDbConnectionSettings
		Dim conf As Configuration = WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath)
		Dim _temp As String = conf.ConnectionStrings.ConnectionStrings("SiteAdmin").ConnectionString

		Dim settings As New CouchDbConnectionSettings
		settings.host = ExtractValue("host", _temp, ";")
		settings.port = ExtractValue("port", _temp, ";")
		settings.db = ExtractValue("db", _temp, ";")
		settings.username = ExtractValue("username", _temp, ";")
		settings.password = ExtractValue("password", _temp, ";")

		Return settings
	End Function
#End Region

#Region " GetApplicationName "
    Friend Function GetApplicationName() As String
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

#Region " .Log() "
    Friend Sub Log(ByVal Data As String, Optional ByVal IsError As Boolean = False)
        Dim logs As New ApplicationLog
		logs.write(Data, IsError)
    End Sub
#End Region

#Region " GetEmbeddedContent "
    Friend Function GetEmbeddedContent(ByVal Resource As String) As Byte()
        Dim assembly As System.Reflection.Assembly = System.Reflection.Assembly.GetExecutingAssembly()
        Dim ns As String = "WDK.ContentManagement.Pages"

        Try
            Dim resStream As System.IO.Stream = assembly.GetManifestResourceStream(ns & "." & Resource)
            If resStream IsNot Nothing Then
                Dim myBuffer(resStream.Length - 1) As Byte
                resStream.Read(myBuffer, 0, resStream.Length)
                Return myBuffer
            Else
                Return BytesOf("<!--- Resource {" & Resource.ToUpper & "} not found under " & ns & " -->")
            End If

        Catch ex As Exception
            Return BytesOf(ex.ToString)
        End Try
    End Function
#End Region

#Region " BytesOf "
    Friend Function BytesOf(ByVal Data As String) As Byte()
        Return Text.Encoding.UTF8.GetBytes(Data)
    End Function
#End Region

#Region " MasterPageUrl "
	Friend Function MasterPageUrl() As String
		Dim defMasterPage As String = "~/siteadmin/DefaultSite/default.master"

		If System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory & "bin\WDK.Providers.Settings.dll") = False Then
			Return defMasterPage
		Else
			Dim Settings As Object = Activator.CreateInstance(Type.GetType("WDK.Providers.Settings,WDK.Providers.Settings"))
			Dim res As Object = Settings.Get("MasterPage")
			If res IsNot Nothing Then
				Return res
			Else
				If System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory & "default.master") Then
					Return "~/default.master"
				Else
					Return defMasterPage
				End If

			End If
		End If
	End Function
#End Region

End Module

Friend Class CouchDbConnectionSettings
	Friend host As String = "localhost"
	Friend port As Integer = 5984
	Friend db As String
	Friend username As String
	Friend password As String
End Class

